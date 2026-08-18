# 美术资源放置说明

游戏运行时从 `Assets/Resources/Art/` 下按固定命名加载图片，**缺图时界面自动回退为占位色块**，不会报错。

命名规则：

- `cards/card_{卡牌id}.png`：卡面图（如 `card_yujian_shu.png`）
- `enemies/enemy_{敌人id}.png`：敌人立绘（如 `enemy_nizhao_jing.png`）
- `intents/intent_{意图动作}.png`：意图图标（`attack / heavy_attack / multi_attack / block / buff / debuff`）
- `nodes/node_{节点类型}.png`：地图节点（`battle / elite / event / rest / boss`）
- `relics/relic_{遗物id}.png`：遗物图标（如 `relic_lingquan.png`）
- `frames/frame_{稀有度}.png`：卡框（`basic / advanced / rare`）

id 与 JSON 数据中的 `id` 字段一致（如敌人 `shi_kui_jiang_jun`，卡牌 `youming_guizhao`）。
