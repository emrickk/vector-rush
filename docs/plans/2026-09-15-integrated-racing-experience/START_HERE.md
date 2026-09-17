# 四任务启动与协作说明

[English version, including all handoffs, the plan, and acceptance criteria](START_HERE.en.md).

用户明确分工：**GPT-6 Astra 负责所有美术资产与 Blender；GPT-5.6 Sol 负责 Unity 程序与集成。** 此分工覆盖旧文档的“全部使用 Sol”与“单任务完成所有内容”。总体验目标、验收标准与不新增玩法的边界不变。当前只准备交接，由用户创建并启动任务。

## 启动顺序

1. 先启动 [01 统筹与 Unity 场景集成](01-integration-sol.md)，选择 GPT-5.6 Sol。它先保存基线、建立独立工作区、发布协调文件与接口分工，并返回 WORKSPACES_READY。
2. 收到 WORKSPACES_READY 后，分别启动 [02 全部美术资产](02-art-assets-astra.md)（GPT-6 Astra）、[03 速度与飞船特效实现](03-motion-vfx-sol.md)（GPT-5.6 Sol）、[04 UI 与声音实现](04-ui-audio-sol.md)（GPT-5.6 Sol）。三项可并行，但按共享契约工作。
3. 把各任务的阶段交付或完成情况反馈给 01，让它读取共享报告并继续集成。先把 Astra 的早期样件放入原生场景，再逐批扩充，不等全部资产做完才合并。
4. 01 交付完整示范段、含声原生主片、碰撞片、可玩 app 与验收结果；用户评审整体后再决定全赛道扩展。

任务之间不会因写文件就自动唤醒。01 在第一次 WORKSPACES_READY 报告后可由用户继续；若其他任务尚未交付，它可先处理自己拥有的环境/灯光基础，不虚构完成或持续监控。

## 唯一归属

以下均相对各自同源仓库；共享源码不能多人改。01 在实际审计后可以将新增文件具体化，并在 coordination 中发布；不能违反模型分工。

| 任务 | 唯一归属 | 必须交给别人的内容 |
| --- | --- | --- |
| 01 / Sol | Unity 场景与 Bootstrap；WorldBuilder、TrackPath、道路/灯光/环境布置代码；Packages/ProjectSettings/asmdef；Importer 与生产组合 prefab；诊断、构建与总报告；共享偏好/事件契约 | 全部模型、纹理、字体、图标、VFX 图集及美术选型交 02；相机逻辑与特效代码交 03；HUD/音频逻辑交 04 |
| 02 / Astra | `SourceAssets/Experience/`；`UnityProject/Assets/Resources/ExperienceArt/` 的模型/纹理/字体/音频及美术材质；其稳定元数据；Blender、UV/LOD、导出/资产生成脚本；构图/放置清单、设计 tokens 和视觉参考 | C#、运行时 shader 代码、生产场景绑定交对应 Sol；全局渲染设置交 01 |
| 03 / Sol | ChaseCamera；IonPropulsion、IonFlameParticles、VehicleVFX 及新增特效/镜头代码；相应 shader 代码和专属行为测试 | 视觉贴图/mesh/材质设计向 02 要求；Bootstrap、Volume/renderer 挂载、TrackPath 与 PlayerPreferences 改动交 01 |
| 04 / Sol | RaceHUD、RaceAudio；新增 UI 动效/声音播放混音代码及专属测试 | 字体与图形/新增声音素材由 02 提供；偏好持久化和共享事件契约交 01；碰撞发射/强度事件接 03 |

`Assets/Resources/ExperienceArt/` 里不允许混入运行时代码。Sol 的 shader 代码放在单独的 `Assets/Resources/Shaders/Experience/`；03 拥有该处特效 shader，01 如需环境 shader 使用不同的 Environment 子目录，发布清单避免同名覆盖。Astra 定义外观与材质输入，Sol 实现其数学/渲染代码。只有技术格式转换与明确约定的参数绑定属于 Sol 的导入工作；改变外观需回给 02。

Astra 交付的是资产源及可导入 payload；01 独占生产场景和装配 prefab。Astra 可在自己工作区制作预览场景，但不把它当作生产场景覆盖。原生试验由相应工作区与重型任务租约控制。

## 共享协调规则

共享目录：`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/coordination/integrated-racing-experience`。01 写 `coordination.md` 和 `contracts.md`；02/03/04 分别只写 `02-art.md`、`03-motion-vfx.md`、`04-ui-audio.md`。每份报告写状态、commit、文件清单、资源/契约版本、所需回应、实际证据和剩余问题。共享目录在源码工作区外，报告链接指向稳定文件，不能只给另一个任务看不到的临时图。

基线由 01 核对当前源码与未提交修改后一次建立。禁止复制 Library/Temp/Builds 充当新工作区；Unity 工作区独立，原生集成工作区只由 01 操作。元数据/GUID 在首次导入后保持稳定。每次合并使用明确的 commit/文件清单，冲突由归属人解决，不整目录覆盖。

01 建立早期契约，02 的首批视觉方案补齐美术部分：

- 同一条约 30 秒路线、三个代表镜头、场景坐标/单位、轨道切线与净空。
- 资产 ID、导出坐标、pivot、材质槽、图集格式/通道、HDR 与 roughness/smoothness 约定、LOD/碰撞代理、字体许可和尺寸。
- 已有 throttle/boost/碰撞/排名状态的唯一来源，事件含义、冷却和暂停/重开规则。只读桥接优先，不能重复执行 gameplay。
- Astra 的事件表现稿；03/04 实现的共同时间轴；01 协调 Volume、动态偏好与最终混音。
- 分域预算和重型运行的租约。性能采样不能与 Blender 渲染、其他游戏实例或编码同时跑。

遇到依赖时继续可独立完成的接口/审计/原生基线工作，并在自己报告中明确需求。不得凭空代做另一个模型的资产，也不得用“等待”掩盖自己未完成的工作。

## 质量责任

02 负责美术方向与资产外观；01 对原生完整画面的实现与最终交付负责。发现冲突时由双方结合原生画面修正，Sol 不另起一套视觉设计。用户拥有最终艺术接受权。所有任务都围绕同一个示范段，子任务通过不能自动推出整体通过。
