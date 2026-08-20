# Buff-TopGame-2D

一个基于 Unity 的俯视角动作射击原型。项目当前采用 **2D 战斗逻辑 + 3D/2.5D 表现**：角色、子弹、范围技能和 AI 主要在平面内计算，模型、粒子、旋转和阴影负责提供立体感。

> 当前定位：可运行的战斗系统 MVP/垂直切片。英雄、状态、敌人感知、天气和波次流程已经具备可扩展骨架；正式美术、完整关卡内容和商业化系统尚未进入本仓库范围。

## 环境

- Unity `2021.3.57f2`
- Windows 开发环境
- 使用 Unity 内置 Input Manager：`Horizontal`、`Vertical`、`Fire1`~`Fire5`、`Jump`
- 代码主要位于 `Assets/Scripts/Fight`，运行时 UI 位于 `Assets/Scripts/UI`

## 快速开始

1. 用 Unity `2021.3.57f2` 打开项目目录。
2. 优先打开 `Assets/Scenes/GameMain.unity` 运行战斗；需要技能调试面板时可使用 `Assets/Scenes/DebugTest.unity`。
3. 确认场景中存在 `GameManager`、`Main Camera`、`Canvas`、地图对象和玩家血条对象。旧场景缺少 `MobSpawnManager` 时，`GameManager` 会在运行时自动补齐。
4. 点击英雄选择面板中的英雄。选择会保存到 `PlayerPrefs`，并重载当前场景后生效。
5. 进入 Play Mode 后使用移动、鼠标瞄准和英雄技能按键开始测试。

如果本地场景经过二次制作，以场景里实际绑定的 `GameDesignConfig` 和对象名为准；没有配置资产时，`GameRuntime` 会创建兼容默认配置。

## 当前架构

项目将战斗能力拆成四层，避免隐身、钻地、魅惑、天气等能力互相覆盖：

1. **状态层**：`ChaState` 统一维护隐身、无敌、中毒、冰冻、眩晕、变羊、辐射、燃烧、伪装和移动形态等状态。
2. **运动层**：`MotionInfluence`、`MovePreorder` 和 `UnitMove` 合并基础移动、击退、风力、龙卷风等运动影响。
3. **控制权层**：`ControlAuthorityState` 记录玩家接管、AI 控制、傀儡控制和脚本强控，并按优先级与时长自动释放。
4. **环境层**：`WeatherController`、`EnvironmentVolume` 和天气视野服务处理风、天气体积、伤害、减速和视野变化。

## 已实现功能

### 英雄系统

| 英雄 | ID | 专长 | 当前技能与按键 |
| --- | --- | --- | --- |
| 暗影潜行者 | `shadow_stalker` | 隐身、回旋镖、翻滚 | `Fire1` 隐身，`Fire2` 回旋镖，`Jump` 翻滚 |
| 毒雾炼金师 | `toxic_alchemist` | 毒伤、减速、再生 | `Fire1` 中毒弹，`Fire2` 手雷，`Fire3` 再生 |
| 风暴术士 | `storm_sage` | 变羊、闪电、冰冻、加速 | `Fire1` 变羊，`Fire2` 闪电，`Fire3` 冰冻，`Fire4` 加速 |

英雄定义包含基础属性、初始技能、按键绑定和状态抗性。可在 `GameDesignConfig.heroDefinitions` 中按 ID 覆盖内置英雄，也可以追加新的有效英雄，不需要修改选择 UI。

### 状态与能力

- 隐身：普通敌人不能锁定；可通过 `TargetDetectionProfile.canDetectInvisible` 配置反隐敌人。
- 无敌：伤害仍经过统一伤害链路，由状态判定拦截。
- 中毒：状态、持续伤害和减速使用同一份抗性结算后的持续时间。
- 控制：变羊、冰冻、眩晕会锁定移动或施法；远程控制怪物会临时接管目标并在结束后还原 AI。
- 伪装：通过有效阵营参与敌我关系计算。
- 分身/诱饵：由 `AbilityEffectService` 创建，并由生命周期组件自动销毁。
- 闪电：锁定范围内最近敌人，走统一伤害与眩晕链路。
- 移动形态：已预留 `underground`、`fly`、`wallClimbing` 三种移动模式和对应状态入口。

### 天气和环境

`WeatherType` 已提供以下规则类型：

- `Cloudy` 云雾/降视野
- `Tornado` 龙卷风
- `MagneticStorm` 地磁吸力
- `Thunderstorm` 雷电
- `Blizzard` 冰墙/暴风雪
- `ToxicFog` 毒气
- `RadiationStorm` 辐射
- `FireRain` 火油/火雨

天气可以通过 `WeatherController.SetWeather` 或时间轴事件 `SetWeather` 设置持续时间、全局风向、运动强度和是否降低视野。多个天气控制器同时存在时，视野服务取最严格的视野倍率。

### 敌人 AI

- 统一目标筛选：距离、前方视野角、地图不可通行格遮挡、隐身和无敌规则均可配置。
- 感知状态：可见目标时追击；丢失目标后前往最后可见位置并搜索；超时后恢复巡逻。
- 声音警报：实际生成的开火、有效受击和爆炸会产生短时调查点；声音不会绕过隐身和遮挡。
- 主动攻击：仅在目标可见、处于攻击距离且敌人允许施法时推进攻击冷却，并使用开火瞬间的方向发射子弹。
- 特殊感知：`TargetDetectionProfile` 可配置反隐、穿墙和忽略天气视野；`SoundAlertProfile` 可配置听觉距离。

### 敌人职业与波次

当前内置敌人定义可复用 `Ghost` 视图，不要求立即制作新预制体：

| ID | 中文名 | 职业 | 特点 |
| --- | --- | --- | --- |
| `assault_grunt` | 突击怪 | `Assault` | 更快接近、近距离压迫 |
| `ranged_grunt` | 枪手怪 | `Ranged` | 默认远程攻击节奏 |
| `swift_runner` | 疾行怪 | `Swift` | 速度快、生命和攻击较低 |
| `armored_colossus` | 重甲巨兽 | `Armored` | 高生命，允许作为 Boss 候选 |

敌人定义支持 ID、中文名、预制体路径、波次范围、出现权重、精英/Boss 资格、生命/攻击/移动/行动倍率和攻击节奏。配置没有合法候选时会回退旧的 `enemyPrefabPath`、`enemyViewAnimInfo` 等字段，避免错误配置卡死生成流程。

默认波次流程为：普通波 → 清场 → 波间休息 → 精英波 → 清场 → 波间休息 → Boss 波 → 清场胜利。玩家死亡优先结算失败；失败和胜利都只结算一次，并提供重开当前场景的按钮。

敌人首次死亡会通过 `CharacterDeathService` 发布统一事件，事件携带受害者、攻击者、玩家身份、阵营、波次标记、序号和时间；波次击杀计数订阅该事件，清场仍以存活扫描作为安全兜底。

### UI 和调试工具

- 英雄选择面板：运行时生成，不依赖新的正式预制体。
- 状态 HUD：显示隐身、毒、冰冻、眩晕、变羊和无敌图标及剩余时间。
- 变羊视觉：优先使用绑定预制体，没有预制体时使用无碰撞程序化羊模型。
- 结算面板：显示胜利/失败并支持重新开始。
- `TestManager` 技能调试面板：可列出技能、学习、施放、生成敌人、清理敌人、切换无敌和停止移动。

## 操作要领

### 基础战斗

- `WASD` 或方向键：移动（对应 `Horizontal`/`Vertical`）。
- 鼠标位置：角色朝向和技能瞄准方向。
- 英雄技能：按当前英雄表中的 `Fire1`~`Fire4`、`Jump`。
- 英雄选择：点击卡片后会重载当前场景，下一局使用新英雄。

### 调试快捷键

场景存在 `TestManager` 时，数字键 `1`~`9` 会走“学习并施放”流程：

| 按键 | 技能 ID | 说明 |
| --- | --- | --- |
| `1` | `shoot1` | 单发直射 |
| `2` | `shoot2` | 两发扇形直射 |
| `3` | `shoot3` | 三发扇形直射 |
| `4` | `poisonShot` | 中毒、减速和持续伤害 |
| `5` | `stunShot` | 伤害并眩晕 |
| `6` | `laser` | 高速直线高伤害 |
| `7` | `lightning` | 追踪最近敌人并造成伤害 |
| `8` | `haste` | 移动和行动加速 |
| `9` | `regen` | 持续回复 |

调试面板还提供技能列表、添加敌人、清理敌人、切换无敌和停止移动按钮。技能调试目标不存在时会自动生成一个敌人。

## 配置说明

### `GameDesignConfig`

通常使用 `Resources/Design/GameDesignConfig`，主要配置项如下：

- `selectedHeroId`：本局默认英雄 ID。
- `showHeroSelectionPanel`：是否显示英雄选择面板。
- `heroDefinitions`：覆盖或追加英雄定义。
- `enemyDefinitions`：覆盖内置敌人目录；数组为空时使用内置定义。
- `enableWaveProgression`：开启波次流程；关闭后使用旧的持续补怪模式。
- `waveCount`、`enemiesPerWave`、`waveSpawnInterval`、`waveIntermission`：波次节奏。
- `eliteWave`、`bossWave` 和对应倍率：精英/Boss 规则。
- `maxMob`、`spawnPeriod`：兼容旧刷怪模式。

### 新增英雄的最小条件

1. 填写唯一 `id` 和有效 `prefabPath`。
2. 填写初始 `skillIds`，并确认每个技能存在于 `GameDatabase`。
3. 如需独立操作，填写 `skillBindings`；为空时回退全局按键。
4. 用 `roleDisplayName` 和 `specialtySummary` 提供中文职业展示。
5. 进入 Play Mode 检查启动校验 Warning，并执行英雄选择和技能测试。

### 新增敌人的最小条件

1. 填写唯一 `id`、`displayName` 和 `prefabPath`，例如复用 `Ghost`。
2. 设置 `spawnWeight`、`minWave`、`maxWave` 及精英/Boss 资格。
3. 设置属性倍率和攻击距离/冷却区间。
4. 先用 EditMode 的目录测试验证权重、波次筛选和旧配置回退，再进入 Play Mode 观察生成日志。

## 示例测试流程

### 三英雄手工冒烟

1. 开启 `showHeroSelectionPanel`，依次选择三个英雄并重载场景。
2. 验证基础属性、初始技能、按键和卡片中文专长随英雄切换。
3. 暗影潜行者：释放隐身，确认普通敌人失去目标；挂载反隐配置后确认特殊敌人可重新锁定。
4. 毒雾炼金师：用中毒弹命中敌人，确认中毒、减速和 DOT 同时结束；验证毒抗性缩短持续时间。
5. 风暴术士：依次验证变羊、闪电、冰冻和加速的控制、伤害、眩晕和属性恢复。
6. 多个状态同时存在时观察 `PlayerHP` 下方状态 HUD，确认图标和倒计时不互相覆盖。

### 敌人感知与波次

1. 在 `DebugTest` 中生成敌人，确认敌人约在首次攻击延迟后才开火。
2. 让英雄隐身、走到攻击距离外或躲到地图阻挡后，确认敌人停止攻击并进入搜索/巡逻。
3. 触发开火、受击和手雷爆炸，确认敌人调查声音位置但不绕过隐身。
4. 开启波次后，确认生成配额完成前不会清场推进，清场后才进入下一波。
5. 击杀最终 Boss，确认只出现一次胜利面板；让玩家死亡，确认只出现一次失败面板。
6. 点击重新开始，确认当前场景重载且没有残留状态、羊模型或选择面板对象。

### 自动化测试

`Assets/Scripts/Fight/Architecture/Editor/HeroAbilityStateTests.cs` 当前包含 24 个 EditMode 回归用例，覆盖：

- 波次清场顺序与最终完成。
- 胜利/失败终局互斥。
- 敌人攻击时机和敌人职业目录权重。
- 三英雄覆盖/追加、毒性时长与毒免疫。
- 隐身、反隐、视野角、距离和地图遮挡。
- 搜索巡逻、天气降视野和特殊感知例外。
- 声音警报阵营、听觉距离和视觉优先级。
- 敌人首次死亡事件只发布一次，以及伤害事件携带实际攻击者。
- 局内经验溢出升级、货币奖励，以及只给主角归属击杀发放奖励。

在 Unity 中打开 `Window > General > Test Runner`，选择 `EditMode` 后运行全部测试。

## 验证命令

在项目根目录执行：

```powershell
dotnet build .\Assembly-CSharp-Editor.csproj -nologo
git diff --check
```

代码编译通过只代表 C# 编译和静态测试代码可加载，不能替代 Unity Play Mode、正式资源、动画和目标设备验证。逐项手工验收请参考 [Docs/ThreeHeroManualTestChecklist.md](Docs/ThreeHeroManualTestChecklist.md)。

## 尚未实现或仍需验收

| 项目 | 当前状态 | 后续工作 |
| --- | --- | --- |
| 正式英雄/敌人预制体 | 未制作 | 导入正式模型、碰撞体、动画表和挂点 |
| 正式技能特效与声音 | 未制作 | 为隐身、毒雾、闪电、冰墙、龙卷风等绑定正式 VFX/SFX |
| 天气专属视觉 | 规则入口已具备 | 为每种天气制作体积、材质、粒子和屏幕后处理 |
| 完整 3D 高度战斗 | 未实现 | 地下、飞行、墙面和龙卷风目前仍需明确真实高度规则 |
| 坐标平面统一 | 待处理 | `UnitMove` 主要使用 `x/y`，部分 AoE/墙体逻辑仍使用 `x/z`；扩展高度玩法前必须统一 |
| 敌人职业差异化攻击 | 部分实现 | 当前主要是属性、攻击距离和节奏差异；飞行、施法、分身等专属 AI 尚未落地 |
| 关卡内容 | 原型地图 | 房间生成、地形机关、宝箱、Boss 行为和关卡节奏尚未制作 |
| 成长与局外系统 | 局内奖励已实现 | 经验、升级和货币奖励已接入；随机强化、实体掉落、解锁、存档和局外经济仍未制作 |
| 联机与网络同步 | 未实现 | 暂无多人协议、状态同步和权威服务器 |
| 正式移动端输入 | 未实现 | 目前以键鼠和 Unity Input Manager 为主，尚未接入虚拟摇杆/手柄布局 |

## 下一阶段建议

1. 先统一战斗坐标约定，并补充坐标转换工具和回归测试。
2. 把敌人职业从属性差异扩展为可配置行为策略：近战、远程、飞行、施法、分身和魅惑。
3. 制作一套可复用的英雄/敌人预制体、动画和技能特效，替换程序化占位表现。
4. 以“普通房间 → 精英房间 → Boss 房间”为最小关卡闭环，加入掉落和随机强化。
5. 完成真实高度玩法评审；只有地下/飞行/墙面影响目标选择时，才升级完整 3D 碰撞和寻路。

## 目录速查

- `Assets/Scripts/Fight/Architecture`：英雄、敌人、状态、控制、目标筛选、波次和终局状态机。
- `Assets/Scripts/Fight/Character`：角色状态、玩家控制器和敌人 AI。
- `Assets/Scripts/Fight/AoE`：范围效果、天气和环境体积。
- `Assets/Scripts/Fight/Manager`：游戏启动、刷怪、镜头、子弹和 AoE 管理。
- `Assets/Scripts/UI`：英雄选择、状态 HUD、技能调试和结算面板。
- `Docs/CHANGELOG.md`：按批次记录实现和验证结果。
- `Docs/ThreeHeroManualTestChecklist.md`：三英雄及敌人感知的 Play Mode 手工清单。

## 开发约定

- 新能力先接入状态、运动、控制权或环境层，再由技能时间轴提交请求。
- 新英雄和敌人优先使用配置扩展，不把职业数据硬编码到 UI 或刷怪器。
- 每完成一个功能批次：更新 `Docs/CHANGELOG.md`、运行针对性验证、`git diff --check`、提交 Git，并在远端可用时推送。
- 预制体、动画和正式特效暂不作为运行时逻辑的硬依赖；缺失资源必须有安全回退和明确 Warning。
