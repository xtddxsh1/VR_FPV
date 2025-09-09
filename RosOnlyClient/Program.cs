using System;
using System.Text.Json;
using System.Threading.Tasks;
using RosOnlyClient.Messages;

namespace RosOnlyClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("RosOnlyClient v2.1 (rosbridge WebSocket)");

            var ros = new RosClient("127.0.0.1", 9090, useTls: false);
            await ros.ConnectAsync();

            // Subscribe /usePointCloud and parse XYZRGB
            await ros.SubscribeAsync("/usePointCloud", "sensor_msgs/PointCloud2", msg =>
            {
                try
                {
                    var pc = JsonSerializer.Deserialize<PointCloud2>(msg.GetRawText());
                    var list = PointCloud2Parser.ParseXYZRGB(pc!, maxPoints: 5);
                    Console.WriteLine($"/usePointCloud width*height={pc!.width}x{pc!.height}, show first {list.Count} points:");
                    for (int i=0;i<list.Count;i++)
                    {
                        var p = list[i];
                        Console.WriteLine($"  [{i}] x={p.x:F3}, y={p.y:F3}, z={p.z:F3}, rgb=({p.r},{p.g},{p.b})");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"PointCloud parse error: {ex.Message}");
                }
            });

            // (Optional) Also subscribe /useRGB
            await ros.SubscribeAsync("/useRGB", "sensor_msgs/CompressedImage", msg =>
            {
                var format = msg.GetProperty("format").GetString();
                var data = msg.GetProperty("data").GetString();
                Console.WriteLine($"/useRGB format={format}, base64Len={data?.Length}");
            });

            Console.WriteLine("Running. Press ENTER to exit...");
            Console.ReadLine();

            await ros.UnsubscribeAsync("/usePointCloud");
            await ros.UnsubscribeAsync("/useRGB");
            await ros.DisposeAsync();
        }
    }
}
