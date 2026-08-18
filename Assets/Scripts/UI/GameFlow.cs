using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Xianmen
{
    public class GameFlow : MonoBehaviour
    {
        public static GameFlow Instance { get; private set; }

        private Canvas _canvas;
        private GameObject _menuRoot;
        private GameObject _mapRoot;
        private GameObject _battleRoot;
        private GameObject _rewardRoot;
        private GameObject _hubRoot;
        private GameObject _shopRoot;
        private GameObject _upgradeRoot;
        private GameObject _eventRoot;
        private GameObject _victoryRoot;
        private GameObject _defeatRoot;
        private Text _mapStatus;
        private Text _enemyStatus;
        private Text _playerStatus;
        private Text _battleLog;
        private Text _rewardInfo;
        private Text _hubStatus;
        private Text _shopInfo;
        private Text _upgradeInfo;
        private Text _eventTitle;
        private Text _eventText;
        private Text _victoryText;
        private Text _defeatText;
        private Transform _handRoot;
        private Transform _rewardCardRoot;
        private Transform _shopCardRoot;
        private Transform _upgradeCardRoot;
        private Transform _eventOptionRoot;
        private Button _endTurnButton;
        private Button _battleContinueButton;
        private Button _claimRelicButton;
        private readonly List<Button> _handButtons = new List<Button>();
        private readonly List<Button> _rewardButtons = new List<Button>();
        private readonly List<Button> _shopButtons = new List<Button>();
        private readonly List<Button> _upgradeButtons = new List<Button>();
        private readonly List<Button> _eventButtons = new List<Button>();
        private List<string> _pendingCardOffers = new List<string>();
        private List<string> _shopOffers = new List<string>();
        private EventData _currentEvent;

        private static readonly string[] NormalEnemies =
        {
            "ni_zhao_jing", "yao_lang", "shanzei_lou_luo", "shanzei_tou_mu", "shi_kui",
            "du_zhu_yao", "hei_xiong_jing", "shi_kui_jiang_jun", "huo_sha_mo", "shuang_shou_jiao",
            "shu_yao", "shui_gui"
        };

        private static readonly string[] EliteEnemies =
        {
            "du_yan_shan_yao", "shi_xiang_kui_lei", "ying_mo"
        };

        private readonly System.Random _rng = new System.Random();
        private BattleState _battleState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DataLoader.LoadAll();
            EnsureEventSystem();
            BuildCanvas();
            BuildMenu();
            BuildMap();
            BuildBattlePanel();
            BuildRewardPanel();
            BuildHubPanel();
            BuildShopPanel();
            BuildUpgradePanel();
            BuildEventPanel();
            BuildEndPanels();
            ShowMenu();
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private void BuildCanvas()
        {
            var go = new GameObject(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );
            go.transform.SetParent(transform, false);
            _canvas = go.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = _canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void BuildMenu()
        {
            _menuRoot = CreateVerticalPanel("MenuRoot", _canvas.transform);
            var title = CreateText("仙门问道", 72, _menuRoot.transform);
            title.rectTransform.sizeDelta = new Vector2(500, 120);
            CreateButton("开始游戏", _menuRoot.transform, OnStartPressed);
            CreateButton("继续", _menuRoot.transform, OnContinuePressed);
            CreateButton("退出", _menuRoot.transform, OnQuitPressed);
        }

        private void BuildMap()
        {
            _mapRoot = CreateVerticalPanel("MapRoot", _canvas.transform);
            var title = CreateText("地图场景（框架占位）", 40, _mapRoot.transform);
            title.rectTransform.sizeDelta = new Vector2(700, 80);
            _mapStatus = CreateText("当前节点：1 / 20", 28, _mapRoot.transform);
            _mapStatus.rectTransform.sizeDelta = new Vector2(700, 60);
            CreateButton("进入当前节点", _mapRoot.transform, OnEnterNodePressed);
            CreateButton("返回主界面", _mapRoot.transform, ShowMenu);
        }

        private void BuildBattlePanel()
        {
            _battleRoot = CreateVerticalPanel("BattleRoot", _canvas.transform);
            var title = CreateText("战斗", 48, _battleRoot.transform);
            title.rectTransform.sizeDelta = new Vector2(500, 80);
            _enemyStatus = CreateText("敌人", 28, _battleRoot.transform);
            _enemyStatus.rectTransform.sizeDelta = new Vector2(700, 70);
            _playerStatus = CreateText("玩家", 28, _battleRoot.transform);
            _playerStatus.rectTransform.sizeDelta = new Vector2(700, 70);

            var handGo = new GameObject("HandRoot", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            handGo.transform.SetParent(_battleRoot.transform, false);
            _handRoot = handGo.transform;
            var handRect = _handRoot.GetComponent<RectTransform>();
            handRect.sizeDelta = new Vector2(1200, 80);
            var handLayout = _handRoot.GetComponent<HorizontalLayoutGroup>();
            handLayout.childAlignment = TextAnchor.MiddleCenter;
            handLayout.childControlWidth = false;
            handLayout.childControlHeight = false;
            handLayout.spacing = 8;

            _battleLog = CreateText("", 24, _battleRoot.transform);
            _battleLog.rectTransform.sizeDelta = new Vector2(700, 50);
            _endTurnButton = CreateButton("结束回合", _battleRoot.transform, OnEndTurnPressed);
            _battleContinueButton = CreateButton("继续", _battleRoot.transform, OnBattleContinuePressed);
            _battleContinueButton.gameObject.SetActive(false);
        }

        private void BuildRewardPanel()
        {
            _rewardRoot = CreateVerticalPanel("RewardRoot", _canvas.transform);
            var title = CreateText("战斗奖励", 40, _rewardRoot.transform);
            title.rectTransform.sizeDelta = new Vector2(500, 70);
            _rewardInfo = CreateText("", 26, _rewardRoot.transform);
            _rewardInfo.rectTransform.sizeDelta = new Vector2(760, 90);

            var cardGo = new GameObject("RewardCardRoot", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            cardGo.transform.SetParent(_rewardRoot.transform, false);
            _rewardCardRoot = cardGo.transform;
            var cardRect = _rewardCardRoot.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(900, 90);
            var cardLayout = _rewardCardRoot.GetComponent<HorizontalLayoutGroup>();
            cardLayout.childAlignment = TextAnchor.MiddleCenter;
            cardLayout.childControlWidth = false;
            cardLayout.childControlHeight = false;
            cardLayout.spacing = 12;

            CreateButton("跳过奖励", _rewardRoot.transform, OnSkipRewardPressed);
            _rewardRoot.SetActive(false);
        }

        private void BuildHubPanel()
        {
            _hubRoot = CreateVerticalPanel("HubRoot", _canvas.transform);
            var title = CreateText("宗门", 48, _hubRoot.transform);
            title.rectTransform.sizeDelta = new Vector2(500, 80);
            _hubStatus = CreateText("", 26, _hubRoot.transform);
            _hubStatus.rectTransform.sizeDelta = new Vector2(760, 90);
            CreateButton("坊市（买卡）", _hubRoot.transform, ShowShop);
            CreateButton("祭炼（升级卡牌）", _hubRoot.transform, ShowUpgrade);
            CreateButton("打坐", _hubRoot.transform, OnHubRestPressed);
            _claimRelicButton = CreateButton("领取遗物", _hubRoot.transform, OnClaimRelicPressed);
            CreateButton("继续前进", _hubRoot.transform, OnHubContinuePressed);
            _hubRoot.SetActive(false);
        }

        private void BuildShopPanel()
        {
            _shopRoot = CreateVerticalPanel("ShopRoot", _canvas.transform);
            var title = CreateText("坊市", 40, _shopRoot.transform);
            title.rectTransform.sizeDelta = new Vector2(500, 70);
            _shopInfo = CreateText("当前货架", 24, _shopRoot.transform);
            _shopInfo.rectTransform.sizeDelta = new Vector2(760, 60);
            var cardGo = new GameObject("ShopCardRoot", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            cardGo.transform.SetParent(_shopRoot.transform, false);
            _shopCardRoot = cardGo.transform;
            var cardRect = _shopCardRoot.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(900, 90);
            var cardLayout = _shopCardRoot.GetComponent<HorizontalLayoutGroup>();
            cardLayout.childAlignment = TextAnchor.MiddleCenter;
            cardLayout.childControlWidth = false;
            cardLayout.childControlHeight = false;
            cardLayout.spacing = 12;
            CreateButton("离开坊市", _shopRoot.transform, ShowHub);
            _shopRoot.SetActive(false);
        }

        private void BuildUpgradePanel()
        {
            _upgradeRoot = CreateVerticalPanel("UpgradeRoot", _canvas.transform);
            var title = CreateText("祭炼", 40, _upgradeRoot.transform);
            title.rectTransform.sizeDelta = new Vector2(500, 70);
            _upgradeInfo = CreateText("选择要升级的卡牌（凡阶 10灵石+2药材 / 灵阶 15+3 / 仙阶 20+5）", 22, _upgradeRoot.transform);
            _upgradeInfo.rectTransform.sizeDelta = new Vector2(900, 70);
            var cardGo = new GameObject("UpgradeCardRoot", typeof(RectTransform), typeof(VerticalLayoutGroup));
            cardGo.transform.SetParent(_upgradeRoot.transform, false);
            _upgradeCardRoot = cardGo.transform;
            var cardLayout = _upgradeCardRoot.GetComponent<VerticalLayoutGroup>();
            cardLayout.childAlignment = TextAnchor.MiddleCenter;
            cardLayout.childControlWidth = false;
            cardLayout.childControlHeight = false;
            cardLayout.spacing = 8;
            CreateButton("离开祭炼", _upgradeRoot.transform, ShowHub);
            _upgradeRoot.SetActive(false);
        }

        private void BuildEventPanel()
        {
            _eventRoot = CreateVerticalPanel("EventRoot", _canvas.transform);
            _eventTitle = CreateText("奇遇", 36, _eventRoot.transform);
            _eventTitle.rectTransform.sizeDelta = new Vector2(600, 60);
            _eventText = CreateText("", 24, _eventRoot.transform);
            _eventText.rectTransform.sizeDelta = new Vector2(820, 170);
            var optionGo = new GameObject("EventOptionRoot", typeof(RectTransform), typeof(VerticalLayoutGroup));
            optionGo.transform.SetParent(_eventRoot.transform, false);
            _eventOptionRoot = optionGo.transform;
            var optionLayout = _eventOptionRoot.GetComponent<VerticalLayoutGroup>();
            optionLayout.childAlignment = TextAnchor.MiddleCenter;
            optionLayout.spacing = 10;
            _eventRoot.SetActive(false);
        }

        private void BuildEndPanels()
        {
            _victoryRoot = CreateVerticalPanel("VictoryRoot", _canvas.transform);
            var vTitle = CreateText("通关！", 48, _victoryRoot.transform);
            vTitle.rectTransform.sizeDelta = new Vector2(500, 80);
            _victoryText = CreateText("", 24, _victoryRoot.transform);
            _victoryText.rectTransform.sizeDelta = new Vector2(820, 220);
            CreateButton("返回主界面", _victoryRoot.transform, OnVictoryBackPressed);

            _defeatRoot = CreateVerticalPanel("DefeatRoot", _canvas.transform);
            var dTitle = CreateText("道途断绝", 48, _defeatRoot.transform);
            dTitle.rectTransform.sizeDelta = new Vector2(500, 80);
            _defeatText = CreateText("", 24, _defeatRoot.transform);
            _defeatText.rectTransform.sizeDelta = new Vector2(820, 220);
            CreateButton("重新开始", _defeatRoot.transform, OnDefeatRestartPressed);
            _victoryRoot.SetActive(false);
            _defeatRoot.SetActive(false);
        }

        private GameObject CreateVerticalPanel(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(520, 480);
            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.spacing = 16;
            return go;
        }

        private Text CreateText(string content, int size, Transform parent)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = content;
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.rectTransform.sizeDelta = new Vector2(400, 60);
            return text;
        }

        private Button CreateButton(string label, Transform parent, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(260, 58);
            var image = go.GetComponent<Image>();
            image.color = new Color(0.18f, 0.28f, 0.38f, 1f);
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);
            var text = CreateText(label, 28, go.transform);
            var textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            return button;
        }

        private void ShowMenu()
        {
            HideAllPanels();
            if (_menuRoot != null) _menuRoot.SetActive(true);
        }

        private void ShowMap()
        {
            HideAllPanels();
            if (_mapRoot != null) _mapRoot.SetActive(true);
            if (_mapStatus != null)
            {
                _mapStatus.text = string.Format("当前节点：{0} / 20", GameState.CurrentNodeIndex + 1);
            }
        }

        private void ShowBattle()
        {
            HideAllPanels();
            if (_battleRoot != null) _battleRoot.SetActive(true);
            RenderBattle();
        }

        private void HideAllPanels()
        {
            if (_menuRoot != null) _menuRoot.SetActive(false);
            if (_mapRoot != null) _mapRoot.SetActive(false);
            if (_battleRoot != null) _battleRoot.SetActive(false);
            if (_rewardRoot != null) _rewardRoot.SetActive(false);
            if (_hubRoot != null) _hubRoot.SetActive(false);
            if (_shopRoot != null) _shopRoot.SetActive(false);
            if (_upgradeRoot != null) _upgradeRoot.SetActive(false);
            if (_eventRoot != null) _eventRoot.SetActive(false);
            if (_victoryRoot != null) _victoryRoot.SetActive(false);
            if (_defeatRoot != null) _defeatRoot.SetActive(false);
        }

        private void OnStartPressed()
        {
            GameState.StartNewRun();
            ShowMap();
        }

        private void OnContinuePressed()
        {
            if (GameState.Load())
            {
                ShowMap();
            }
        }

        private void OnEnterNodePressed()
        {
            var node = GameState.CurrentNode;
            if (node == null) return;

            switch (node.type)
            {
                case "battle":
                case "elite":
                case "boss":
                    StartBattle();
                    break;
                case "rest":
                    var healRatio = GameState.Relics.Contains("lingquan") ? 0.5f : 0.3f;
                    var heal = Mathf.CeilToInt(GameState.MaxHp * healRatio);
                    GameState.CurrentHp = Mathf.Min(GameState.MaxHp, GameState.CurrentHp + heal);
                    GameState.Save();
                    ShowHub();
                    break;
                case "event":
                    ShowEvent();
                    break;
            }
        }

        private void StartBattle()
        {
            var node = GameState.CurrentNode;
            if (node == null) return;
            var enemyId = PickEnemyId(node.type);
            var enemy = DataLoader.GetEnemy(enemyId);
            if (enemy == null)
            {
                Debug.LogError("Enemy data missing: " + enemyId);
                return;
            }
            _battleState = new BattleState(GameState.Deck, BattleState.ScaleForNode(enemy, node.index));
            ShowBattle();
        }

        private string PickEnemyId(string nodeType)
        {
            if (nodeType == "elite") return EliteEnemies[_rng.Next(EliteEnemies.Length)];
            if (nodeType == "boss") return "mo_zun";
            return NormalEnemies[_rng.Next(NormalEnemies.Length)];
        }

        private void RenderBattle()
        {
            if (_battleState == null) return;
            _enemyStatus.text = string.Format(
                "{0}\nHP {1}/{2}  罡气 {3}",
                _battleState.Enemy.Name,
                Mathf.Max(0, _battleState.Enemy.CurrentHp),
                _battleState.Enemy.MaxHp,
                _battleState.Enemy.Block
            );
            _playerStatus.text = string.Format(
                "掌门\nHP {0}/{1}  罡气 {2}  灵力 {3}",
                Mathf.Max(0, _battleState.Player.CurrentHp),
                _battleState.Player.MaxHp,
                _battleState.Player.Block,
                _battleState.Energy
            );
            RebuildHand();
            _endTurnButton.interactable = _battleState.PlayerTurn && !_battleState.BattleOver;
            _battleContinueButton.gameObject.SetActive(_battleState.BattleOver);
            _battleLog.text = _battleState.BattleOver
                ? (_battleState.PlayerWon ? "战斗胜利！" : "战斗失败...")
                : "";
        }

        private void RebuildHand()
        {
            foreach (var button in _handButtons)
            {
                if (button != null) Destroy(button.gameObject);
            }
            _handButtons.Clear();
            if (_battleState == null) return;

            for (var i = 0; i < _battleState.Hand.Count; i++)
            {
                var card = DataLoader.GetCard(_battleState.Hand[i]);
                var index = i;
                var label = card != null ? string.Format("{0} ({1})", card.name, card.cost) : "?";
                var button = CreateButton(label, _handRoot, () => OnCardPressed(index));
                _handButtons.Add(button);
            }
        }

        private void OnCardPressed(int handIndex)
        {
            if (_battleState == null) return;
            var card = DataLoader.GetCard(_battleState.Hand[handIndex]);
            if (card == null) return;
            if (_battleState.PlayCard(card, handIndex))
            {
                RenderBattle();
            }
        }

        private void OnEndTurnPressed()
        {
            if (_battleState == null) return;
            _battleState.EndPlayerTurn();
            RenderBattle();
        }

        private void OnBattleContinuePressed()
        {
            if (_battleState == null) return;
            if (_battleState.PlayerWon)
            {
                if (GameState.CurrentNodeIndex >= GameState.MapNodes.Count - 1)
                {
                    ShowVictory();
                    return;
                }
                ShowRewards();
            }
            else
            {
                ShowDefeat();
            }
        }

        private void ShowRewards()
        {
            var node = GameState.CurrentNode;
            if (node == null)
            {
                ShowHub();
                return;
            }

            var reward = RewardSystem.RollRewards(node.type, node.index, GameState.Relics, _rng);
            GameState.AddResources(reward.stones, reward.herbs);
            if (reward.heal > 0)
            {
                GameState.CurrentHp = Mathf.Min(GameState.MaxHp, GameState.CurrentHp + reward.heal);
                GameState.Save();
            }
            if (!string.IsNullOrEmpty(reward.relic))
            {
                GameState.PendingRelics.Add(reward.relic);
                GameState.Save();
            }

            var relicName = "";
            if (!string.IsNullOrEmpty(reward.relic))
            {
                var relic = DataLoader.GetRelic(reward.relic);
                relicName = relic != null ? "，获得遗物「" + relic.name + "」（回宗门领取）" : "";
            }
            _rewardInfo.text = string.Format(
                "灵石 +{0}  药材 +{1}{2}{3}",
                reward.stones,
                reward.herbs,
                reward.heal > 0 ? "  HP +" + reward.heal : "",
                relicName
            );

            _pendingCardOffers = reward.cardOffers;
            RebuildRewardCards();

            HideAllPanels();
            _rewardRoot.SetActive(true);
        }

        private void RebuildRewardCards()
        {
            foreach (var button in _rewardButtons)
            {
                if (button != null) Destroy(button.gameObject);
            }
            _rewardButtons.Clear();

            for (var i = 0; i < _pendingCardOffers.Count; i++)
            {
                var card = DataLoader.GetCard(_pendingCardOffers[i]);
                var index = i;
                var label = card != null ? string.Format("{0}（{1}）", card.name, RarityName(card.rarity)) : "?";
                var button = CreateButton(label, _rewardCardRoot, () => OnRewardCardPressed(index));
                _rewardButtons.Add(button);
            }
        }

        private string RarityName(string rarity)
        {
            switch (rarity)
            {
                case "advanced": return "灵阶";
                case "rare": return "仙阶";
                default: return "凡阶";
            }
        }

        private void OnRewardCardPressed(int index)
        {
            if (index < 0 || index >= _pendingCardOffers.Count) return;
            GameState.AddCardToDeck(_pendingCardOffers[index]);
            ShowHub();
        }

        private void OnSkipRewardPressed()
        {
            ShowHub();
        }

        private void ShowHub()
        {
            RefreshHubStatus();
            HideAllPanels();
            if (_hubRoot != null) _hubRoot.SetActive(true);
        }

        private void RefreshHubStatus()
        {
            _hubStatus.text = string.Format(
                "灵石 {0}  药材 {1}  生命 {2}/{3}",
                GameState.SpiritStone,
                GameState.Herb,
                GameState.CurrentHp,
                GameState.MaxHp
            );
            _claimRelicButton.gameObject.SetActive(GameState.PendingRelics.Count > 0);
            if (GameState.PendingRelics.Count > 0)
            {
                var label = _claimRelicButton.GetComponentInChildren<Text>();
                label.text = "领取遗物（" + GameState.PendingRelics.Count + "）";
            }
        }

        private void OnHubRestPressed()
        {
            var healRatio = GameState.Relics.Contains("lingquan") ? 0.5f : 0.3f;
            var heal = Mathf.CeilToInt(GameState.MaxHp * healRatio);
            GameState.CurrentHp = Mathf.Min(GameState.MaxHp, GameState.CurrentHp + heal);
            GameState.Save();
            RefreshHubStatus();
        }

        private void OnClaimRelicPressed()
        {
            foreach (var relicId in GameState.PendingRelics)
            {
                GameState.AddRelic(relicId);
            }
            GameState.PendingRelics.Clear();
            GameState.Save();
            RefreshHubStatus();
        }

        private void OnHubContinuePressed()
        {
            GameState.AdvanceNode();
            ShowMap();
        }

        private void ShowShop()
        {
            var nodeIndex = GameState.CurrentNodeIndex + 2;
            _shopOffers = RewardSystem.RollCards(nodeIndex, 3, _rng);
            RebuildShopCards();
            HideAllPanels();
            _shopRoot.SetActive(true);
        }

        private void RebuildShopCards()
        {
            foreach (var button in _shopButtons)
            {
                if (button != null) Destroy(button.gameObject);
            }
            _shopButtons.Clear();

            for (var i = 0; i < _shopOffers.Count; i++)
            {
                var card = DataLoader.GetCard(_shopOffers[i]);
                var index = i;
                var price = ShopPrice(card);
                var label = card != null
                    ? string.Format("{0}（{1}）- {2}灵石", card.name, RarityName(card.rarity), price)
                    : "?";
                var button = CreateButton(label, _shopCardRoot, () => OnShopCardPressed(index));
                _shopButtons.Add(button);
            }
        }

        private int ShopPrice(CardData card)
        {
            if (card == null) return 0;
            var price = card.rarity == "advanced" ? 25 : (card.rarity == "rare" ? 40 : 15);
            if (GameState.Relics.Contains("lianqi_ge"))
            {
                price = Mathf.FloorToInt(price * 0.85f);
            }
            return price;
        }

        private void OnShopCardPressed(int index)
        {
            if (index < 0 || index >= _shopOffers.Count) return;
            var card = DataLoader.GetCard(_shopOffers[index]);
            if (card == null) return;
            var price = ShopPrice(card);
            if (GameState.SpiritStone < price) return;
            GameState.AddResources(-price, 0);
            GameState.AddCardToDeck(card.id);
            ShowHub();
        }

        private void ShowUpgrade()
        {
            RebuildUpgradeCards();
            HideAllPanels();
            _upgradeRoot.SetActive(true);
        }

        private void RebuildUpgradeCards()
        {
            foreach (var button in _upgradeButtons)
            {
                if (button != null) Destroy(button.gameObject);
            }
            _upgradeButtons.Clear();

            for (var i = 0; i < GameState.Deck.Count; i++)
            {
                var entry = GameState.Deck[i];
                var card = DataLoader.GetCard(GameState.BaseCardId(entry));
                if (card == null || GameState.IsUpgraded(entry)) continue;
                var index = i;
                var stones = card.rarity == "advanced" ? 15 : (card.rarity == "rare" ? 20 : 10);
                var herbs = card.rarity == "advanced" ? 3 : (card.rarity == "rare" ? 5 : 2);
                var label = string.Format(
                    "{0}（{1}费）- {2}灵石+{3}药材",
                    card.name,
                    card.cost,
                    stones,
                    herbs
                );
                var button = CreateButton(label, _upgradeCardRoot, () => OnUpgradeCardPressed(index));
                _upgradeButtons.Add(button);
            }
        }

        private void OnUpgradeCardPressed(int deckIndex)
        {
            if (deckIndex < 0 || deckIndex >= GameState.Deck.Count) return;
            var entry = GameState.Deck[deckIndex];
            if (GameState.IsUpgraded(entry)) return;
            var card = DataLoader.GetCard(GameState.BaseCardId(entry));
            if (card == null) return;
            var stones = card.rarity == "advanced" ? 15 : (card.rarity == "rare" ? 20 : 10);
            var herbs = card.rarity == "advanced" ? 3 : (card.rarity == "rare" ? 5 : 2);
            if (GameState.SpiritStone < stones || GameState.Herb < herbs) return;
            GameState.AddResources(-stones, -herbs);
            GameState.UpgradeCardAt(deckIndex);
            RebuildUpgradeCards();
        }

        private void ShowEvent()
        {
            var pool = new List<EventData>(DataLoader.Events.Values);
            if (pool.Count == 0)
            {
                _mapStatus.text = "没有奇遇数据";
                return;
            }
            _currentEvent = pool[_rng.Next(pool.Count)];
            _eventTitle.text = _currentEvent.name;
            _eventText.text = _currentEvent.text;
            RebuildEventOptions();
            HideAllPanels();
            _eventRoot.SetActive(true);
        }

        private void RebuildEventOptions()
        {
            foreach (var button in _eventButtons)
            {
                if (button != null) Destroy(button.gameObject);
            }
            _eventButtons.Clear();
            if (_currentEvent == null || _currentEvent.options == null) return;

            for (var i = 0; i < _currentEvent.options.Count; i++)
            {
                var option = _currentEvent.options[i];
                var index = i;
                var costText = "";
                if (option.cost != null)
                {
                    costText = option.cost.resource == "spirit_stone"
                        ? "（消耗 " + option.cost.value + " 灵石）"
                        : "（消耗 " + option.cost.value + " 药材）";
                }
                var label = option.text + costText;
                var button = CreateButton(label, _eventOptionRoot, () => ResolveEventOption(index));
                button.interactable = CanAffordEventCost(option);
                _eventButtons.Add(button);
            }
        }

        private bool CanAffordEventCost(EventOption option)
        {
            if (option.cost == null) return true;
            if (option.cost.resource == "spirit_stone") return GameState.SpiritStone >= option.cost.value;
            if (option.cost.resource == "herb") return GameState.Herb >= option.cost.value;
            return true;
        }

        private void ResolveEventOption(int index)
        {
            if (_currentEvent == null || _currentEvent.options == null) return;
            if (index < 0 || index >= _currentEvent.options.Count) return;
            var option = _currentEvent.options[index];
            if (!CanAffordEventCost(option)) return;

            if (option.cost != null)
            {
                if (option.cost.resource == "spirit_stone") GameState.AddResources(-option.cost.value, 0);
                else if (option.cost.resource == "herb") GameState.AddResources(0, -option.cost.value);
            }

            var effect = option.effect;
            if (effect != null)
            {
                switch (effect.type)
                {
                    case "heal":
                        GameState.CurrentHp = Mathf.Min(GameState.MaxHp, GameState.CurrentHp + effect.value);
                        break;
                    case "resource":
                        if (effect.resource == "spirit_stone") GameState.AddResources(effect.value, 0);
                        else if (effect.resource == "herb") GameState.AddResources(0, effect.value);
                        break;
                    case "reward_card":
                        var rarity = string.IsNullOrEmpty(effect.rarity) ? "advanced" : effect.rarity;
                        var cardId = RewardSystem.RollCardOfRarity(rarity, _rng);
                        if (cardId != null) GameState.AddCardToDeck(cardId);
                        break;
                    case "reward_relic":
                        var relicId = RewardSystem.RollRelic(_rng);
                        if (relicId != null)
                        {
                            GameState.PendingRelics.Add(relicId);
                            GameState.Save();
                        }
                        break;
                }
            }
            GameState.Save();
            ShowHub();
        }

        private void ShowVictory()
        {
            _victoryText.text = "剑落，魔气散尽。\n你站在魔尊陨落之地，遥望青云山的方向。\n百年浩劫，终在你手中画上句点。\n从此，青云门的大名，将重新响彻玄天大陆。";
            HideAllPanels();
            _victoryRoot.SetActive(true);
        }

        private void OnVictoryBackPressed()
        {
            GameState.EndRun();
            ShowMenu();
        }

        private void ShowDefeat()
        {
            _defeatText.text = "眼前一黑，你倒在了半途。\n青云门的火种，似乎就要熄灭了……\n（重新开始，宗门复兴之路，尚可再来。）";
            HideAllPanels();
            _defeatRoot.SetActive(true);
        }

        private void OnDefeatRestartPressed()
        {
            GameState.EndRun();
            ShowMenu();
        }

        private void OnQuitPressed()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
