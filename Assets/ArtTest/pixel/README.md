# 《仙门问道》像素美术素材清单

> 版本：v1.0 · 全部素材为代码绘制的透明底 PNG，无第三方授权负担，可直接商用。

## 目录结构

```text
pixel/
├─ enemies/      怪物立绘 16 张（288×288）
├─ icons/        卡面图标 60 张（128×128，文件名 = cards.json 的 id）
├─ relics/       遗物图标 15 张（128×128）
├─ frames/       卡框 3 张（200×280，按稀有度）
├─ nodes/        地图节点图标 5 张（96×96）
├─ intents/      敌人意图图标 5 张（128×128）
├─ ui/           UI 组件（按钮/面板/血条/资源图标/卡背/徽记）
└─ backgrounds/  背景 4 张（1600×900）
```

## 怪物（16）

- `ni_zhao_jing` 泥沼精
- `yao_lang` 妖狼
- `shanzei_lou_luo` 山贼喽啰
- `shanzei_tou_mu` 山贼头目
- `shi_kui` 尸傀
- `du_zhu_yao` 毒蛛妖
- `hei_xiong_jing` 黑熊精
- `shi_kui_jiang_jun` 尸傀将军
- `huo_sha_mo` 火煞魔
- `shuang_shou_jiao` 双首蛟
- `shu_yao` 树妖
- `shui_gui` 水鬼
- `du_yan_shan_yao` 独眼山妖
- `shi_xiang_kui_lei` 石像傀儡
- `ying_mo` 影魔
- `mo_zun` 魔尊

## 卡面图标（60）

- `yujian_shu` 御剑术
- `lianhuan_jian` 连环剑
- `tuci` 突刺
- `zhanfeng` 斩风
- `pojia_jian` 破甲剑
- `dusha_zhang` 毒煞掌
- `jianqi` 剑气
- `pishan_jian` 劈山剑
- `zhendi_quan` 震地拳
- `lieyan_zhang` 烈焰掌
- `zhuihun_ci` 追魂刺
- `hengsao` 横扫
- `suigu_ji` 碎骨击
- `yufeng_jian` 御风剑
- `gangqi_huti` 罡气护体
- `tiebushan` 铁布衫
- `panshi_gong` 磐石功
- `jinzhong_zhao` 金钟罩
- `xieli` 卸力
- `huxin_jing` 护心镜
- `guixi_gong` 龟息功
- `tiangang_zhao` 天罡罩
- `dangjian_jue` 挡剑诀
- `tuna_shu` 吐纳术
- `ningshen` 凝神
- `juqi` 聚气
- `jingxin_jue` 静心诀
- `jixing_bu` 疾行步
- `liaoshang_shu` 疗伤术
- `yinqi_ru_ti` 引气入体
- `wanjian_jue` 万剑诀
- `leiting_jian` 雷霆剑
- `youming_guizhao` 幽冥鬼爪
- `xuesha_zhan` 血煞斩
- `pojun` 破军
- `yulei_shu` 御雷术
- `fentian_zhang` 焚天掌
- `jianxin_tongming` 剑心通明
- `yushou_jue` 御兽诀
- `tianlei_zhengfa` 天雷正法
- `taiji_huti` 太极护体
- `jingang_buhuai` 金刚不坏
- `luohan_gong` 罗汉功
- `fanshi_gangqi` 反噬罡气
- `tongpi_tiegu` 铜皮铁骨
- `linggui_ke` 灵龟壳
- `tiangang_bu` 天罡步
- `dahuandan` 大还丹
- `hunyuan_yiqi` 混元一气
- `yuqi_feixing` 御气飞行
- `yujian_feixing` 御剑飞行
- `mieshi_jianyi` 灭世剑意
- `wandu_shixin` 万毒噬心
- `tiangang_shengti` 天罡圣体
- `qiankun_yizhi` 乾坤一掷
- `yiqi_hua_sanqing` 一气化三清
- `xiantian_gangqi` 先天罡气
- `xisui_fagu` 洗髓伐骨
- `jianpo_xukong` 剑破虚空
- `lingxi_yizhi` 灵犀一指

## 遗物图标（15）

- `zongmen_zhangce` 宗门账册
- `lingshi_kuangmai` 灵石矿脉
- `lingyao_yuan` 灵药园
- `lianqi_ge` 炼器阁
- `shoushan_dazhen` 守山大阵
- `fangshi_ling` 坊市令
- `huichun_cao` 回春草
- `xianghuo_qian` 香火钱
- `cangbao_tu` 藏宝图
- `lingshi_che` 灵石车
- `lingquan` 灵泉
- `yanwu_chang` 演武场
- `juling_zhen` 聚灵阵
- `tianji_pan` 天机盘
- `zongmen_jinku` 宗门金库

## 卡框（3）
- `frame_basic.png` 凡阶（钢蓝）
- `frame_advanced.png` 灵阶（紫）
- `frame_rare.png` 仙阶（金）

## 地图节点（5）
- `node_battle.png` 战斗 · `node_elite.png` 精英 · `node_event.png` 奇遇 · `node_rest.png` 打坐 · `node_boss.png` Boss

## 意图图标（5）
- `intent_attack.png` 攻击 · `intent_heavy.png` 重击 · `intent_block.png` 防御 · `intent_buff.png` 增益 · `intent_debuff.png` 减益

## UI 组件
- `btn_normal.png / btn_hover.png / btn_pressed.png / btn_disabled.png` 按钮四态（120×40）
- `panel_frame.png` 面板（9-slice，边框 8px，四角金色装饰）
- `bar_track.png` 血条轨道 + `bar_fill_hp/energy/block/enemy_hp.png` 各色填充（按比例截取宽度）
- `res_spirit_stone.png` 灵石 · `res_herb.png` 药材（128×128）
- `card_back.png` 卡背（200×280）
- `emblem_title.png` 标题徽记（192×192）

## 背景（4）
- `bg_menu.png` 主菜单（夜月山门）· `bg_map.png` 地图（星空远山）· `bg_battle.png` 战斗（暮色仙山）· `bg_hub.png` 宗门 Hub（夕照宗门）

## 接入建议
- 图标/立绘均为透明底 PNG，Unity 中 `Sprite` 直接导入，`Filter Mode = Point`（否则会模糊）。
- 面板用 9-slice：`Border` 设为 8px。
- 血条：轨道底图固定，填充图用 `Image.Type = Filled`（Horizontal）按百分比显示。
- 卡名/描述文字不用烧进图里，用字体渲染（推荐霞鹜文楷，OFL 免费商用）。
