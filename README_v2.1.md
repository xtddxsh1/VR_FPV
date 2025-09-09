# VR_FPV_Repo v2.1 — 合并 rosbridge 客户端 & PointCloud2 解析示例

本版本在 v2 基础上合并：
1) **RosOnlyClient（rosbridge WebSocket 可用版）**：`RosClient.cs` 可直接连接 `rosbridge_websocket`；提供 Advertise/Publish/Subscribe 等。
2) **/usePointCloud 解析示例**：订阅 `sensor_msgs/PointCloud2`，解析 base64 数据为 XYZRGB（示例打印前 5 个点）。

## 运行 RosOnlyClient（.NET 6+）
先启动 rosbridge：
```bash
roslaunch rosbridge_server rosbridge_websocket.launch
# 或
ros2 launch rosbridge_server rosbridge_websocket.launch
```
然后在 `RosOnlyClient/`：
```bash
dotnet build
dotnet run
```
输出会显示：
- `/usePointCloud` 点云尺寸与前 5 个点坐标+颜色
- `/useRGB` 的格式与 base64 长度

> 注意：rosbridge 默认将 `PointCloud2.data` 与 `CompressedImage.data` 编码为 **base64**。

## Unity 工程（UnityProject/）
保持与 v2 一致：包含四个脚本、坐标系转换、点云 Shader、一键生成场景工具。需要渲染/VR 时使用 Unity；仅通信时使用 RosOnlyClient。
