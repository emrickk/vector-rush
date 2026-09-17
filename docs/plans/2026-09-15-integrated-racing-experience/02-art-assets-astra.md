# 02：统一美术方向、全部资产与 Blender 制作

指定模型：GPT-6 Astra。用户明确要求资产与建模全部由 Astra 完成。你不只做建筑，还负责本示范段用到的飞船视觉、材质/贴图、VFX 图集、字体和 UI 图形；Sol 实现程序并集成。

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

## 归属与第一项工作

你拥有 SourceAssets/Experience、ExperienceArt payload、Blender 文件与生成/导出脚本、美术材质、设计 tokens 和构图清单。遵守 START_HERE 的具体边界。不要覆盖生产 Unity 场景、写 gameplay/C# 或修改相机/全局渲染配置。

WORKSPACES_READY 后先看实际原生示范路线与参考视频，审计可用资产。把每类关键现有资产标为保留、改造、替换或远景化。交一张完整风格板、三个路线构图、简短资产清单和材质/字体规范，立即进入第一批制作，不再交一份等待别人的泛泛计划。

## 分批交付，最早一批就能进入 Unity

**A：共同视觉语言与最小可见资产包。** 给城市、赛道、飞船、特效与 UI 定统一轮廓、材质、颜色和明暗规则。优先交付一组能看出明显差异的建筑/裙楼/近景护栏指引，加上尾焰及碰撞图集样件、字体选择与 HUD 数字/标题样式。03/04 可以用这些开始验证，不能让技术任务等到全部城市资产结束。

**B：城市资产与布局。** 按总计划建议的建筑家族制作真正不同的体量/顶部/立面分区，加地标、交通结构、连桥、屋顶设备、广告与导向模块。变体不能只是换高度或换窗颜色。提供沿实际路线的实例放置清单，带稳定 ID、位置/旋转/缩放、材质槽与 LOD；01 把它装入生产场景。避免等距摆楼。建筑近中远景交叠，但赛道视线、净空和路线可读性始终优先。

**C：飞船、推进与碰撞的视觉资产。** 保留反重力与蓝色推进方向，设计更饱满的主喷流和粒子层次，做有时间连贯性的火焰图集、细长火花/碎片状亮点素材与必要网格。明确 UV 朝向、帧数/FPS、通道、透明度/亮度的约定，避免任意独立生成帧造成闪烁。定义 boost 船身材质/能量/轮廓变化和轻擦、持续擦墙、重撞的视觉节奏。若机械开合或船体调整有价值，先用原生样例证明再投入，不新增 gameplay。

**D：UI 与声音素材。** 提供有赛车个性的显示字体与易读小字搭配、许可/字符覆盖、图标/仪表/面板/选择态图形、尺寸/安全区/颜色 tokens，以及已有 boost/排名/碰撞提示的动效分镜和时间建议。先覆盖 HUD 与代表菜单。没有新玩法的数据不得画进设计。新增声音素材如确需制作或选取，由你负责来源、许可与原始素材；04 实现播放、既有运行时合成链路及混音代码，按你提供的音色规范工作，不自行制作新的源音频资产。

## 资产生产要求

- `.blend`、可重跑的生成/导出脚本与实际导出文件一并交付。记录单位、轴、pivot、应用变换、法线/切线、UV、材质槽、贴图颜色空间和通道。
- 有意设计粗糙度/金属/玻璃/发光分区；Blender 材质观感不等于 URP 结果。给 Sol 可复现的材质数据和兼容贴图，shader 行为需求写清楚。
- 提供必要 LOD 和简单碰撞代理；实际赛道碰撞面由 01 维护，不能用高面数建筑细节当运动碰撞。
- 记录 provenance、许可、纹理尺寸、内存/面数预算。已有可用资产优先；未经明确预算不采购。用户给的参考只用于观察视觉，不当作可挪用的资产包。
- 字体、纹理、图形和预览的 GUID/文件名稳定。技术导入失败与美术不合格分开报告。只在自己的预览场景验证，不覆盖 01 的生产场景。

## 原生反馈与结束条件

每批交付带 commit、manifest、样图/片和 `02-art.md` 状态，请求 01/03/04 在真实镜头验证并回传问题。根据这些原生结果修正，尤其检查近景连接、重复感、远景雾中轮廓、霓虹泛白、尾焰片感和字体实际尺寸。

你的资产包不能仅凭 Blender studio render 宣布完成。必须有原生导入与运动场景中的可用证据；若依赖整合任务，报告“资产可供集成 / 原生验证待回传”，不要伪称已过。用户并未要求你派生新的资产工作任务，自己完成这份范围并分批交付。
