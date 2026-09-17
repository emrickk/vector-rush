# 03：速度、飞船状态与碰撞特效实现

指定模型：GPT-5.6 Sol。你负责运行时代码与 shader 实现。所有视觉素材、模型、图集、材质外观和 Blender 制作由 02 / Astra 负责。

## 共同目标、位置与权限

完成约 30 秒完整、可实际驾驶的赛博朋克反重力赛车示范段，覆盖密集多样的城市、霓虹/雾、平顺倾斜、速度模糊、饱满蓝色尾焰、船身 boost 变化、真实碰撞反馈、赛车字体/UI 动效和声音同步。保留现有玩法；用户评审示范段后才扩展全赛道和全部既有界面。

主仓库（先只读，不能直接当作你的工作区）：
`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/opening-city`

总计划：`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/opening-city/docs/plans/2026-09-15-integrated-racing-experience/PLAN.md`
验收表：`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/opening-city/docs/plans/2026-09-15-integrated-racing-experience/ACCEPTANCE.md`
任务顺序与归属：`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/opening-city/docs/plans/2026-09-15-integrated-racing-experience/START_HERE.md`

共享协调目录（由 01 建立）：
`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/coordination/integrated-racing-experience`

先读取该目录的 `coordination.md`，从中取自己的真实 workspace、branch、baseline commit、状态文件路径和共享契约。不要猜工作区路径。若尚未发布 WORKSPACES_READY，先做只读分析并明确依赖，不在主仓库开始修改，也不自行创建另一套基线。用户在新任务里要求“执行此交接”即授权该任务的实现；本次编写交接本身未开始实现。

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

## 文件归属

下列源码路径以自己工作区的 `UnityProject/Assets/Scripts/` 为前缀。

拥有 Gameplay/ChaseCamera.cs，Presentation/IonPropulsion.cs、IonFlameParticles.cs、VehicleVFX.cs，新增相机/特效代码、Experience 特效 shader 代码及相应测试。不得改 Bootstrap、WorldBuilder、TrackPath、生产场景、ProjectSettings/Packages、PlayerPreferences 或 RaceHUD/RaceAudio。挂载点、全局 Volume 与持久化设置需求交 01；音画/UI 事件契约与 04 协调。

## 从已有行为出发

源码已有阻尼/FOV/抖动、连续倾斜路面、boost 和碰撞粒子。先查看原生表现，明确问题是幅度、时序、形态、空间关系还是输入错误。用户要的是速度、能量和接触感；不能把使用新技术或更多粒子当成目标。

01 完成工作区后，你可先做现有输入/事件、相机、生命周期和渲染顺序审计，并建立可测试接口。使用现有资源做诊断，不制作占位美术替代 Astra。向 02 早发 VFX 所需图集格式、帧数/方向/通道、材质参数和屏幕尺度约束；美术外观由 02 决定。

## 实现顺序

1. **速度与弯道。** 在真实近景/弯道中协调跟随阻尼、前视、有限 FOV、事件冲量与 boost 起落。区分道路 up、船身侧倾与镜头滚转，避免双重倾斜。不要提高真实速度来伪造进步，发现道路几何/倾斜不连续交 01。
2. **运动模糊。** 验证 URP 可用路径、深度/运动矢量及 UI/透明特效顺序。若改用外围方向性模糊，标明近似并保持中央路线清楚。仅实现自己的 shader/controller；01 挂载 renderer/Volume。按照现有 reduced-motion/舒适度语义处理关闭/降级，偏好修改由 01 统一实施。
3. **飞船 boost 状态。** 用 Astra 的外观方案驱动已有 boost 的起势、持续、退出，船身材质/轮廓/能量与推进同拍。保留 collision、质量、速度、消耗与现有状态权威，不增加新能力，也不猜视频 holographic phase 的玩法。
4. **饱满推进。** 使用 Astra 连贯图集和素材，把亮核、翻卷主体、外散亮粒/尾迹分层。控制相对速度、生命周期、近镜头/深度淡出、过绘与远处对手预算。消除片边、断喷口、低帧率串珠、静态水柱与大光斑。先看完整运动，再调局部参数。
5. **真实碰撞。** 由真实 contact point/normal、法向冲击和切向滑动驱动轻擦、持续擦墙和重撞；停止接触后停止持续发射。01/你核实墙面接触是否真的经过当前事件路径，不能只假设 OnCollisionEnter 有效。冷却/池化、重复接触合并、屏幕遮挡上限明确。发给 04 同一真实冲击事件供声音/UI 响应，不新增损伤系统。

## 协作与检查

把变化映射到 contracts 的单一事件来源。VFX 的随机流不得影响比赛/AI 随机流。暂停、返回标题、重开、自动恢复、boost 耗尽和快速重复接触都要稳定；资源不泄漏。不要复制两套输入采样或分别计算不一致的“碰撞强度”。

先把早期可运行代码和 Astra 样件交 01 原生整合，再完善。在 `03-motion-vfx.md` 写 commit、变更文件、契约、所需的 01 挂载/02 资产/04 消费方、原生片及未解决问题。让主片显示常速/加速/boost/弯道/释放，碰撞真实演示可以是单独受控片，必须标注输入方式。

完工条件是当前源码的行为/生命周期验证，以及真实镜头中速度、推进和碰撞的直接表现证据。性能由你测局部开销，最终完整场景测量归 01。编译/单元测试通过不等于视觉接受。不要开始城市模型、UI 美术或全赛道扩展。
