# 01：体验统筹与 Unity 场景集成

指定模型：GPT-5.6 Sol。你负责可运行的完整场景和交付，全部美术资产必须交给 02 / Astra 制作。

## 共同目标、位置与权限

完成约 30 秒完整、可实际驾驶的赛博朋克反重力赛车示范段，覆盖密集多样的城市、霓虹/雾、平顺倾斜、速度模糊、饱满蓝色尾焰、船身 boost 变化、真实碰撞反馈、赛车字体/UI 动效和声音同步。保留现有玩法；用户评审示范段后才扩展全赛道和全部既有界面。

主仓库（先只读，不能直接当作你的工作区）：
`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/opening-city`

总计划：`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/opening-city/docs/plans/2026-09-15-integrated-racing-experience/PLAN.md`
验收表：`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/opening-city/docs/plans/2026-09-15-integrated-racing-experience/ACCEPTANCE.md`
任务顺序与归属：`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/opening-city/docs/plans/2026-09-15-integrated-racing-experience/START_HERE.md`

共享协调目录（由 01 建立）：
`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/coordination/integrated-racing-experience`

若共享目录已有 `coordination.md`，先核对真实 workspace、branch、baseline commit、状态文件路径与契约，接着已有进度做。若尚未建立，你作为 01 按下文第一阶段创建它及四个工作区，不等待别人发布 WORKSPACES_READY。不要在原主仓库直接实施或虚构工作区路径。用户在新任务里要求“执行此交接”即授权该任务的实现；本次编写交接本身未开始实现。

已知运行时起点为 `7358f77`（道路直接光候选），在 `b987c23`（蓝色粒子尾焰）之上。后续文档提交不等于运行时变更，仍须核对当前 HEAD 与脏文件。旧美术候选未获用户整体认可，可以重做呈现，但必须保留恢复点。

参考视频：
`/Users/anping.wang/Library/Containers/com.tencent.xinWeChat/Data/Documents/xwechat_files/emrick_ee64/msg/video/2026-09/229355dc35da682a84e16c2cb0379898_raw.mp4`

参考碰撞截图：
`/var/folders/yx/hf7ht73550q2156rvymtwnrr0000gn/T/codex-clipboard-33ecef45-723d-430c-b58f-336234386fa1.png`
截图若失效，从视频提取。HOLOGRAPHIC PHASE 的真实机制未知，只借鉴现有 boost 的船身表现，不新增护盾/变形玩法。

既有证据与 app：
`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/artifacts/road-light-response`
`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/artifacts/exhaust-blue-v2`

模型分工是用户的明确要求：所有美术资产选择、设计、制作、修改（含 Blender 与其生成脚本、模型、UV、LOD、贴图、材质视觉参数、VFX 图集、UI 图形、字体及新增声音素材）归 GPT-6 Astra。GPT-5.6 Sol 做程序、shader 代码、技术导入、绑定和运行时集成；不得自行生成一套美术资产代替 Astra，也不得私自改 Astra 的外观设计。程序化建筑几何若是制作资产，仍归 Astra；读取/实例化其清单的 Unity 代码归 Sol。既有资产可暂用于验证接口，不能作为最终资产交付冒充完成。

只写你被分配的文件和自己的状态报告。跨归属变更写进自己的请求条目，由文件主人实施。不要自动创建/唤醒用户任务、发送跨任务消息或启动代理；本轮由用户创建四个任务。共享文件是交接依据，不是假装任务结束后还会自动监控。

## 第一阶段：为其他三个任务准备真实工作区

这是唯一允许在 WORKSPACES_READY 前准备基线的任务。先审计主仓库、最新提交、未提交文件、现有原生 app 与正在运行的 Editor。保留这些内容，不能盲目提交/丢弃整个工作树或恢复旧版本。对必要的环境/资产差异记录取舍与来源，在新的集成分支保存可恢复的基线，再从同一个起点建立 02/03/04 独立工作区。把源码与必要资产迁入，不复制 Unity 缓存或 native builds。

推荐将新工作区放在 `/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/experience-workspaces`，实际路径由你创建并写入 coordination，禁止假称已经存在。为各任务记录分支、baseline commit、读写归属与当前工作区状态。Astra 的 Blender 与资产目录也必须可追溯、可合并。

建立共享目录，发布 coordination 与 contracts 初版。明确 30 秒路线、三处真实镜头、源目录/目标目录、资产 ID/坐标规范、现有事件接口、偏好挂载点、渲染管线版本与租约。无需自己设计最终建筑/字体/贴图；这些由 02 交第一批美术方案补齐。

核对各工作区能读取自己需要的源文件、规范和参考后，向用户返回 WORKSPACES_READY 及 02–04 的启动顺序。这一阶段只表示协作条件就绪，不是游戏品质完成。

## 后续执行：继续你的实质实现与持续集成

用户继续本任务后，不等所有资产完工才工作：核对 WorldBuilder、道路网格/TrackFrame、现有灯光、雾、后处理、构建与录制路径，完成必要的集成接口；已有资产仅用来验证技术链路。接收 02 的第一组建筑/路边模块，立即原生验证尺度、包围感、视差、受光与导向，再把结果写回协调报告供用户转达。

你拥有环境实例化与组合代码、轨道连续性与碰撞面、全局灯光/雾/曝光基准、Bootstrap/Volume/renderer 连接。遵循 Astra 的布局与美术规范；需要环境模型、材质贴图或路面资产修改时给 02 具体请求，不自己生成。

源码位于各工作区的 `UnityProject/Assets/Scripts/`。环境入口包括 `World/WorldBuilder.cs`、`World/TrackPath.cs`、`World/OpeningRoadFinish.cs`、`World/NightTrackLighting.cs` 与 `Presentation/OpeningRoadLightResponse.cs`；初始化入口为 `VectorBootstrap.cs`。现有 gameplay 若需只读事件出口，由你添加最小桥接并保持规则与状态更新不变，不能把该改动留成无人负责的依赖。

03 拥有镜头和特效代码。你只实现它提出的 Volume/renderer/depth/motion-vector 挂载和偏好持久化变更，避免同文件双写。04 拥有 UI/声音代码。你协调 02 的图形/字体包与 03 的真实事件，解决集成编译和跨组件生命周期问题。

先保留赛车路径中心线、规则和速度。若倾斜/网格需调整，把 TrackFrame、可见路面、护栏与碰撞面一起验证，不用相机滚转掩盖几何错误。调整光照可重平衡之前道路候选，但保留对照，不能把更亮当作更好。

## 早期验证与最终门槛

第一批原生样例必须包含实际新建筑、霓虹/雾、03 的早期推进效果以及 04 的字体/事件样式，逐批在同一场景汇合。样例暴露根本方向问题就纠正，不把问题推到交付前一轮“polish”。资产外观问题回 02，代码问题交文件主人。

最终使用 ACCEPTANCE.md 逐项提供证据。交付约 30 秒含声的连续驾驶片、真实碰撞补充片、普通启动即生效的 app、三组对照及可追溯的源码/资产清单。UI 的完整扩展在后续阶段，本阶段确保 HUD 与代表菜单语言一致并保持所有既有功能可用。

性能目标为当前 M4 Max 原生 1080p 的持续 60 fps 级别，必须重新测整段/碰撞压力段，报告长帧和可用 CPU/GPU 数据。基线拍摄、诊断 simulation-time 和实时速度/声音演示分别标注。变更相机/倾斜时不能伪称像素或姿态相同。

阶段 0–2 真正交付后停下让用户评审，不自动扩展全赛道，不用三份子任务完成报告代替整体判断。某个核心项仍差就明确报告，不宣布 owner acceptance。

## 工具与历史

已有 Unity 6000.6.0f1 / URP 17.6.0，Pipeline 版本与工作树存在变化，启动前重新核实。之前 native build 遇到 Pipeline 元数据依赖问题；证据包保留了临时排除 Pipeline、显式保留 Newtonsoft JSON 3.2.2、恢复 manifest/lock 的流程。诊断后才复用，不盲目改 PackageCache 或升级。

`VectorRushSetup.BuildMac()` 会调用 `Prepare()` 重写场景/材质/设置，不能未经核对用于当前场景构建。用真实保存场景并核实启动默认值。`tools/opening-city-run.py` 的租约与当前项目路径要统一；你管理跨工作区重型任务预约，性能时关闭其他负载。

阅读工作区中的 `docs/road-light-response-review.md` 和 `docs/blue-exhaust-review.md` 获取旧候选的真实验证范围。按需复用 `tools/capture-stage2.py` 与 `Stage2Evidence.cs` 的输入/帧/姿态记录；新版仍须重新获取测试、连续运动/声音和性能证据。

只有你更新集成工作区的总 implementation-plan / development-history；其他人提供 scoped commits 与自己的报告。发布阶段合并记录、实际检查与限制。origin 原先是本地 integration 路径，不是假定的 GitHub 目标，不擅自推送外部。
