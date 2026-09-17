# 04：赛车 UI 动效与声音实现

指定模型：GPT-5.6 Sol。你实现现有界面的样式绑定、动画和音频播放/混音；字体选择、图形、贴图、音频素材与美术方案全部来自 02 / Astra。

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

拥有 Presentation/RaceHUD.cs、RaceAudio.cs，新增 UI 动效/音频控制代码与专属测试。不得改相机/VFX、World/TrackPath、Bootstrap、生产场景、ProjectSettings/Packages、PlayerPreferences 或原始字体/图形/音频素材。共享设置和事件桥接交 01，碰撞强度与 boost 权威事件来自 03/现有 gameplay。

## 第一项工作

核对现有 HUD、主菜单、暂停、设置、结果、键鼠/手柄导航和 reduced-motion 行为。已有 Barlow 字体、boost 与排名提示、分层音频，不能宣称一切都没有。整理当前信息/动作清单和实际声音路径，发送给 02 作为视觉设计输入。先整理动画/绑定接口和既有事件，不新增玩法、页面或数据项。

## 第一轮呈现范围

将 Astra 的显示字体/小字搭配、大小/字距/颜色 tokens 与图形包接入实际像素尺寸。重点完成示范段 HUD（速度、位置、boost、已有提示）的整体赛车身份，以及一张代表菜单的选中/进入/退出动效；其他既有界面维持功能与基础一致性。全部界面的完整美术扩展属于用户认可示范段后的阶段 3，不提前扩大交付。

已有事件提示要有一致的起势、强调、保持、退出和中断规则。超车提示、boost 变化、碰撞警示的频率和优先级合理，避免同屏所有元素闪动。碰撞 UI 只表达已有事件，不新增护盾/伤害数值。Astra 负责视觉节奏稿，你根据真实交互调整并回传差异。

现有 IMGUI 若能实现就沿用；不能借机做一整套 UI 框架迁移。动画更新放在正确的 Update/时间源，不用重复 OnGUI 回调推进时间。遵守暂停/reduced-motion、字号与安全区，保持中央驾驶区域清楚。字体缺字符、图形不合尺寸等直接回 02，不擅自换另一套视觉资源。

## 声音

沿用现有引擎、风声、boost、撞击与提示的播放体系，围绕真实 throttle、速度、boost 起落、接触强度与 UI 事件重混音。音色规范、新素材选型与源音频资产制作归 Astra；你实现播放、既有运行时合成链路及混音代码，绑定约定参数，不自行制作新的源音频资产。

轻擦、持续擦墙、重撞有不同持续/瞬态行为，不能每帧叠一个 one-shot。限制并发与峰值，检查重复碰撞不爆音；引擎、风声和事件声有空间，音乐不能掩盖反馈。用与 03 同源的事件与时间戳同步，暂停、恢复、重开不残留循环声。没有听过的结果不得仅凭波形或代码断言听感合格。

## 分批交付与验证

02 的第一批字体/图形方案到达后，先交 HUD 与代表菜单可运行样例给 01；不要等全部图形制作结束才联调。借助 03 的真实 boost/碰撞事件验证 UI/声音共同响应，给 Astra 回传原生录像中的外观问题。

在 `04-ui-audio.md` 写 commit、文件清单、资产/契约版本、需要 01 的设置连接、实际画面/声音验证和未解决项。检查常用分辨率与缩放、现有键鼠/手柄导航、快速重复事件、暂停、重开、reduced-motion 和音频资源生命周期。

交付有声音的原生事件短片与必要行为检查，供 01 合入约 30 秒主片。最终全画面风格与音量平衡由 01 会同 Astra 调整，你配合修正。不要把 source-only 或静态 UI 截图当作动效/声音通过，不代替用户宣布整体艺术接受。
