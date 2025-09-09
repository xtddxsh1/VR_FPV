# VR FPV Repo v2 (Unity + ROS) — 渲染可选 / 仅通信可选

本仓库提供两种形态：
1) **UnityProject/**：完整 Unity 工程（可视化 + ROS 通信）。支持 VR/点云/RGB；新增**坐标系转换**（Unity → ROS REP-103）。
2) **RosOnlyClient/**：C# 控制台工程样例（无渲染，仅通信的骨架，建议连 rosbridge）。

## 统一答复你的问题
- **C# 脚本是否已接上 VR？**  
  Unity 侧 `ControllerPosePublisher.cs` 使用 `UnityEngine.XR` 获取手柄位姿；若暂不接 VR，可在无设备环境下运行，或通过 `-batchmode -nographics` 仅走 ROS 通道。
- **打包好的原始文件与工程文件**  
  见下方“下载链接”。Unity 工程包含所有脚本与 Editor 工具，可直接生成 Prefab+场景联调。
- **坐标系转换（左/右手系）**  
  新增 `CoordinateConverter.cs`。默认 **开启 convertToRosCoord** 将 Unity 左手系（X右/Y上/Z前）转换为 **ROS REP-103 右手系**（X前/Y左/Z上）：
  - 位置：`pos_ros = (z, -x, y)`
  - 旋转：采用基向量重映射构建旋转矩阵后再转四元数，稳定可靠。
- **引擎**  
  Unity 2022.3 LTS；如果你只需要 ROS 通道，不需要渲染，可用 `-batchmode -nographics` 运行 Unity，或参考 `RosOnlyClient/` 用 C# 直接连 ROS（建议 rosbridge）。

## Unity 快速使用
1) 安装 Unity Hub + Unity 2022.3（Ubuntu 20.04）。
2) 打开 `UnityProject/` → 菜单 **Tools → Create VR Viz Prefab and Scene**。
3) 在 `Project → ROS Settings` 设置 ROS 地址。话题：`/useRGB`、`/usePointCloud`、`/usePose`、`/useJoy`。
4) 运行 `Assets/Scenes/VRVizSample.unity`。若仅通信：
```bash
/opt/Unity/Editor/Unity -projectPath $(pwd)/UnityProject -batchmode -nographics -quit
```

## RosOnlyClient（仅通信，无渲染）
- 这是一个结构样例，建议配合 `rosbridge_suite`（WebSocket）。把 `RosClient` 换成真实实现即可。
