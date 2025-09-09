# RosOnlyClient (C#，无渲染，仅 ROS 通信样例)

说明：如果当前阶段**不需要渲染**，仅需打通 **C# SDK → ROS** 的通道，建议：
- 使用 `rosbridge_suite`（WebSocket/JSON）或自建简易 TCP 协议。
- 本样例给出一个最小的 C# 控制台程序结构，演示如何组织 Publisher/Subscriber 代码结构（未引入第三方依赖）。

## 目录
- Program.cs：入口
- RosClient.cs：抽象发布/订阅接口（你可以换成 rosbridge/WebSocket 实现）
- Messages/：示例消息结构（CompressedImage、PointCloud2、PoseStamped、Joy 的极简占位）

> 实战中更推荐直接在 Unity 工程里运行 **-batchmode -nographics**，利用 `ROSConnection` 现成协议与消息定义。
