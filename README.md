# 《仙门问道》Xianmen: Path of Ascension

修仙题材的牌组构筑肉鸽，目标平台为 Steam（Windows 优先），技术栈为 Unity 2022 LTS + C#。

## 文档

- 玩法文档：`Xianmen-Path-of-Ascension-1.0.md`
- 故事文档：`Xianmen-Story-1.0.md`
- 执行计划：`docs/`

## 当前状态

- [x] 项目框架初始化（Unity）
- [x] 数据加载：DataLoader / GameState
- [x] 地图生成器骨架
- [x] 主界面与地图占位场景
- [x] 首批数据示例：卡牌、敌人、遗物、事件
- [ ] 战斗引擎
- [ ] 宗门 Hub
- [ ] 完整地图流程
- [ ] 剧情文案全量录入
- [ ] 美术替换
- [ ] Windows 导出

## 项目结构

```text
E:\xianmen
├─ Assets/
│  ├─ Resources/Data/       # JSON 数据（卡牌/敌人/遗物/事件/地图配置）
│  ├─ Scenes/Main.unity     # 主场景
│  └─ Scripts/
│     ├─ Bootstrap.cs       # 启动入口
│     ├─ Data/              # 数据模型与加载
│     ├─ Game/              # 游戏状态
│     ├─ Map/               # 地图生成
│     └─ UI/                # 界面控制
├─ Packages/manifest.json
├─ ProjectSettings/
├─ docs/                    # 执行计划与策划建议
├─ Xianmen-Path-of-Ascension-1.0.md
└─ Xianmen-Story-1.0.md
```

## 运行

用 Unity 2022 LTS 打开本项目文件夹，运行 `Assets/Scenes/Main.unity`。
