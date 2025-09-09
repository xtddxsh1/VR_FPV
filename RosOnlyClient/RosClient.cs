using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace RosOnlyClient
{
    /// <summary>
    /// Minimal rosbridge WebSocket client.
    /// Supports: connect/close, advertise, unadvertise, publish, subscribe, unsubscribe.
    /// </summary>
    public class RosClient : IAsyncDisposable
    {
        private readonly Uri _uri;
        private readonly ClientWebSocket _ws = new ClientWebSocket();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private long _idSeq = 0;

        private readonly JsonSerializerOptions _jsonOpt = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // topic -> callback (JsonElement msg)
        private readonly ConcurrentDictionary<string, Action<JsonElement>> _subs = new();

        public RosClient(string host = "127.0.0.1", int port = 9090, bool useTls = false)
        {
            var scheme = useTls ? "wss" : "ws";
            _uri = new Uri($"{scheme}://{host}:{port}");
        }

        public async Task ConnectAsync(CancellationToken ct = default)
        {
            await _ws.ConnectAsync(_uri, ct);
            _ = Task.Run(ReceiveLoopAsync);
        }

        public async ValueTask DisposeAsync()
        {
            try { _cts.Cancel(); } catch {}
            if (_ws.State == WebSocketState.Open)
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
            _ws.Dispose();
            _cts.Dispose();
        }

        public async Task AdvertiseAsync(string topic, string type, CancellationToken ct = default)
            => await SendAsync(new { op = "advertise", topic, type, id = NextId("adv") }, ct);

        public async Task UnadvertiseAsync(string topic, CancellationToken ct = default)
            => await SendAsync(new { op = "unadvertise", topic, id = NextId("uadv") }, ct);

        public async Task PublishAsync<T>(string topic, T rosMsg, CancellationToken ct = default)
            => await SendAsync(new { op = "publish", topic, msg = rosMsg, id = NextId("pub") }, ct);

        public async Task SubscribeAsync(string topic, string type, Action<JsonElement> onMessage, int throttleMs = 0, int queueLength = 1, CancellationToken ct = default)
        {
            _subs[topic] = onMessage;
            await SendAsync(new { op = "subscribe", topic, type, throttle_rate = throttleMs, queue_length = queueLength, id = NextId("sub") }, ct);
        }

        public async Task UnsubscribeAsync(string topic, CancellationToken ct = default)
        {
            _subs.TryRemove(topic, out _);
            await SendAsync(new { op = "unsubscribe", topic, id = NextId("unsub") }, ct);
        }

        private async Task SendAsync(object payload, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(payload, _jsonOpt);
            var buf = Encoding.UTF8.GetBytes(json);
            await _ws.SendAsync(buf, WebSocketMessageType.Text, true, ct);
        }

        private string NextId(string pfx) => $"{pfx}:{Interlocked.Increment(ref _idSeq)}";

        private async Task ReceiveLoopAsync()
        {
            var buf = new byte[1<<20];
            var seg = new ArraySegment<byte>(buf);
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var res = await _ws.ReceiveAsync(seg, _cts.Token);
                    if (res.MessageType == WebSocketMessageType.Close) break;
                    int count = res.Count;
                    while (!res.EndOfMessage)
                    {
                        res = await _ws.ReceiveAsync(seg.Slice(count), _cts.Token);
                        count += res.Count;
                    }
                    var json = Encoding.UTF8.GetString(buf, 0, count);
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("op", out var opProp) && opProp.GetString() == "publish")
                    {
                        var topic = root.GetProperty("topic").GetString();
                        if (topic != null && _subs.TryGetValue(topic, out var cb))
                        {
                            cb(root.GetProperty("msg"));
                        }
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RosClient] recv error: {ex.Message}");
                    await Task.Delay(200);
                }
            }
        }
    }
}
