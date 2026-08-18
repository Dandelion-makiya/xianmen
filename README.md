# 《仙门问道》Xianmen: Path of Ascension

修仙题材的牌组构筑肉鸽，目标平台为 Steam（Windows 优先），技术栈为 Unity 2022 LTS + C#。

## 文档

- 玩法文档：`Xianmen-Path-of-Ascension-1.0.md`
- 故事文档：`Xianmen-Story-1.0.md`
- 执行计划：`docs/`
- 打开指南：`docs/项目打开指南.md`

## 当前状态

- [x] 项目框架初始化（Unity）
- [x] 数据加载：DataLoader / GameState
- [x] 地图生成器（20 节点 / 连续战斗不超过 3 校验）
- [x] 主界面与地图占位场景
- [x] 首批数据示例：卡牌、敌人、遗物、事件
- [x] 战斗引擎初版（回合流程 / 牌库手牌 / 伤害 Buff / 敌人意图）
- [x] 全量数据录入：60 卡 / 16 敌 / 15 遗物 / 3 事件（含文案）
- [x] 敌人按节点类型随机选择 + 成长倍率（上限 1.8）
- [x] 战斗掉落与奖励（灵石/药材/卡牌三选一/精英遗物）
- [x] 宗门 Hub（坊市/祭炼/打坐/领取遗物/继续前进）
- [x] 奇遇事件接入（3 个）
- [x] 完整循环 Demo（战斗→奖励→Hub→下一节点→Boss→通关/失败结算）
- [x] 地图节点可视化（类型配色 + 当前节点高亮）
- [x] 战斗界面：敌人意图 / 状态效果 / 牌库弃牌 / 手牌描述 / 已祭炼标记
- [x] Hub 牌组查看
- [x] 美术管线：SpriteLibrary（Resources/Art 自动加载，缺图回退占位）
- [ ] 剧情文案全量录入
- [ ] 美术替换
- [ ] Windows 导出

## 项目结构

```text
E:\xianmen
├─ Assets/
│  ├─ README.md             # 美术资源规划
│  ├─ Resources/Data/       # JSON 数据（卡牌/敌人/遗物/事件/地图配置）
│  ├─ Scenes/Main.unity     # 主场景
│  └─ Scripts/
│     ├─ Bootstrap.cs       # 启动入口
│     ├─ Battle/            # 战斗引擎
│     ├─ Data/              # 数据模型与加载
│     ├─ Game/              # 游戏状态
│     ├─ Map/               # 地图生成
│     └─ UI/                # 界面控制
├─ Packages/manifest.json
├─ ProjectSettings/
├─ docs/                    # 执行计划、策划建议、打开指南
├─ Xianmen-Path-of-Ascension-1.0.md
└─ Xianmen-Story-1.0.md
```

## 运行

用 Unity 2022 LTS（本项目锁定 2022.3.20f1）打开本项目文件夹，运行 `Assets/Scenes/Main.unity`。
详细步骤见 `docs/项目打开指南.md`。
