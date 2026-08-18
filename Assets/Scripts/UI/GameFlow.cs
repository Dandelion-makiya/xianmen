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
        private GameObject _deckRoot;
        private GameObject _cardDetailRoot;
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
        private Text _battleTitle;
        private Text _cardDetailTitle;
        private Text _cardDetailDesc;
        private Transform _handRoot;
        private Transform _rewardCardRoot;
        private Transform _shopCardRoot;
        private Transform _upgradeCardRoot;
        private Transform _eventOptionRoot;
        private Transform _deckCardRoot;
        private Button _endTurnButton;
        private Button _battleContinueButton;
        private Button _claimRelicButton;
        private Button _continueButton;
        private Image _enemyImage;
        private readonly List<Button> _handButtons = new List<Button>();
        private readonly List<Button> _rewardButtons = new List<Button>();
        private readonly List<Button> _shopButtons = new List<Button>();
        private readonly List<Button> _upgradeButtons = new List<Button>();
        private readonly List<Button> _eventButtons = new List<Button>();
        private readonly List<Text> _deckRows = new List<Text>();
        private readonly List<Image> _mapNodeImages = new List<Image>();
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
            BuildDeckPanel();
            BuildCardDetailPanel();
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
            SetPanelBackground(_menuRoot, SpriteLibrary.Background("menu"));
            var title = CreateText("仙门问道", 72, _menuRoot.transform);
            title.rectTransform.sizeDelta = new Vector2(500, 120);
            var subtitle = CreateText("一人一宗，对抗漫天魔气 · Demo", 22, _menuRoot.transform);
            subtitle.rectTransform.sizeDelta = new Vector2(500, 40);
            CreateButton("开始游戏", _menuRoot.transform, OnStartPressed);
            _continueButton = CreateButton("继续", _menuRoot.transform, OnContinuePressed);
            CreateButton("退出", _menuRoot.transform, OnQuitPressed);
        }

        private void BuildMap()
        {
            _mapRoot = CreateVerticalPanel("MapRoot", _canvas.transform);
            SetPanelBackground(_mapRoot, SpriteLibrary.Background("map"));
            var title = CreateText("仙门地图", 40, _mapRoot.transform);
            title.rectTransform.sizeDelta = new Vector2(700, 70);

            var nodeRow = new GameObject("NodeRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            nodeRow.transform.SetParent(_mapRoot.transform, false);
            var rowRect = nodeRow.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(700, 62);
            var rowLayout = nodeRow.GetComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = false;
            rowLayout.spacing = 1;

            for (var i = 1; i <= 20; i++)
            {
                var nodeGo = new GameObject("Node" + i, typeof(RectTransform), typeof(Image));
                nodeGo.transform.SetParent(nodeRow.transform, false);
                var nodeRect = nodeGo.GetComponent<RectTransform>();
                nodeRect.sizeDelta = new Vector2(24, 54);
                var nodeImage = nodeGo.GetComponent<Image>();
                nodeImage.color = new Color(0.4f, 0.4f, 0.45f, 1f);
                var label = CreateText(i.ToString(), 14, nodeGo.transform);
                var labelRect = label.rectTransform;
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;
                _mapNodeImages.Add(nodeImage);
            }

            _mapStatus = CreateText("当前节点：1 / 20", 24, _mapRoot.transform);
            _mapStatus.rectTransform.sizeDelta = new Vector2(700, 50);
            CreateButton("进入当前节点", _mapRoot.transform, OnEnterNodePressed);
            CreateButton("返回主界面", _mapRoot.transform, ShowMenu);
        }

        private void BuildBattlePanel()
        {
            _battleRoot = CreateVerticalPanel("BattleRoot", _canvas.transform, 1320, 920);
            SetPanelBackground(_battleRoot, SpriteLibrary.Background("battle"));
            _battleTitle = CreateText("战斗", 48, _battleRoot.transform);
            _battleTitle.rectTransform.sizeDelta = new Vector2(500, 80);
            var enemyGo = new GameObject("EnemyImage", typeof(RectTransform), typeof(Image));
            enemyGo.transform.SetParent(_battleRoot.transform, false);
            _enemyImage = enemyGo.GetComponent<Image>();
            _enemyImage.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            _enemyImage.rectTransform.sizeDelta = new Vector2(220, 170);
            _enemyStatus = CreateText("敌人", 28, _battleRoot.transform);
            _enemyStatus.rectTransform.sizeDelta = new Vector2(700, 70);
            _playerStatus = CreateText("玩家", 28, _battleRoot.transform);
            _playerStatus.rectTransform.sizeDelta = new Vector2(700, 70);

            var handGo = new GameObject("HandRoot", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            handGo.transform.SetParent(_battleRoot.transform, false);
            _handRoot = handGo.transform;
            var handRect = _handRoot.GetComponent<RectTransform>();
            handRect.sizeDelta = new Vector2(1200, 96);
            var handLayout = _handRoot.GetComponent<HorizontalLayoutGroup>();
            handLayout.childAlignment = TextAnchor.MiddleCenter;
            handLayout.childControlWidth = false;
            handLayout.childControlHeight = false;
            handLayout.spacing = 8;

            _battleLog = CreateText("", 24, _battleRoot.transform);
            _battleLog.rectTransform.sizeDelta = new Vector2(1100, 120);
            _battleLog.alignment = TextAnchor.LowerLeft;
            _battleLog.fontSize = 20;
            _endTurnButton = CreateButton("结束回合", _battleRoot.transform, OnEndTurnPressed);
            _battleContinueButton = CreateButton("继续", _battleRoot.transform, OnBattleContinuePressed);
            _battleContinueButton.gameObject.SetActive(false);
        }

        private void BuildRewardPanel()
        {
            _rewardRoot = CreateVerticalPanel("RewardRoot", _canvas.transform, 1000, 560);
            SetPanelBackground(_rewardRoot, SpriteLibrary.Ui("panel_frame", 8));
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
            SetPanelBackground(_hubRoot, SpriteLibrary.Background("hub"));
            var title = CreateText("宗门", 48, _hubRoot.transform);
            title.rectTransform.sizeDelta = new Vector2(500, 80);
            _hubStatus = CreateText("", 26, _hubRoot.transform);
            _hubStatus.rectTransform.sizeDelta = new Vector2(760, 90);
            CreateButton("坊市（买卡）", _hubRoot.transform, ShowShop);
            CreateButton("祭炼（升级卡牌）", _hubRoot.transform, ShowUpgrade);
            CreateButton("打坐", _hubRoot.transform, OnHubRestPressed);
            _claimRelicButton = CreateButton("领取遗物", _hubRoot.transform, OnClaimRelicPressed);
            CreateButton("牌组查看", _hubRoot.transform, ShowDeck);
            CreateButton("继续前进", _hubRoot.transform, OnHubContinuePressed);
            _hubRoot.SetActive(false);
        }

        private void BuildShopPanel()
        {
            _shopRoot = CreateVerticalPanel("ShopRoot", _canvas.transform, 1000, 560);
            SetPanelBackground(_shopRoot, SpriteLibrary.Ui("panel_frame", 8));
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
            _upgradeRoot = CreateVerticalPanel("UpgradeRoot", _canvas.transform, 1000, 640);
            SetPanelBackground(_upgradeRoot, SpriteLibrary.Ui("panel_frame", 8));
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
            _eventRoot = CreateVerticalPanel("EventRoot", _canvas.transform, 900, 620);
            SetPanelBackground(_eventRoot, SpriteLibrary.Ui("panel_frame", 8));
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
            _victoryRoot = CreateVerticalPanel("VictoryRoot", _canvas.transform, 900, 560);
            SetPanelBackground(_victoryRoot, SpriteLibrary.Ui("panel_frame", 8));
            var vTitle = CreateText("通关！", 48, _victoryRoot.transform);
            vTitle.rectTransform.sizeDelta = new Vector2(500, 80);
            _victoryText = CreateText("", 24, _victoryRoot.transform);
            _victoryText.rectTransform.sizeDelta = new Vector2(820, 220);
            CreateButton("返回主界面", _victoryRoot.transform, OnVictoryBackPressed);

            _defeatRoot = CreateVerticalPanel("DefeatRoot", _canvas.transform, 900, 560);
            SetPanelBackground(_defeatRoot, SpriteLibrary.Ui("panel_frame", 8));
            var dTitle = CreateText("道途断绝", 48, _defeatRoot.transform);
            dTitle.rectTransform.sizeDelta = new Vector2(500, 80);
            _defeatText = CreateText("", 24, _defeatRoot.transform);
            _defeatText.rectTransform.sizeDelta = new Vector2(820, 220);
            CreateButton("重新开始", _defeatRoot.transform, OnDefeatRestartPressed);
            _victoryRoot.SetActive(false);
            _defeatRoot.SetActive(false);
        }

        private void BuildDeckPanel()
        {
            _deckRoot = CreateVerticalPanel("DeckRoot", _canvas.transform, 700, 640);
            SetPanelBackground(_deckRoot, SpriteLibrary.Ui("panel_frame", 8));
            var title = CreateText("牌组", 40, _deckRoot.transform);
            title.rectTransform.sizeDelta = new Vector2(500, 70);
            var listGo = new GameObject("DeckCardRoot", typeof(RectTransform), typeof(VerticalLayoutGroup));
            listGo.transform.SetParent(_deckRoot.transform, false);
            _deckCardRoot = listGo.transform;
            var listLayout = _deckCardRoot.GetComponent<VerticalLayoutGroup>();
            listLayout.childAlignment = TextAnchor.MiddleCenter;
            listLayout.childControlWidth = false;
            listLayout.childControlHeight = false;
            listLayout.spacing = 4;
            CreateButton("关闭", _deckRoot.transform, ShowHub);
            _deckRoot.SetActive(false);
        }

        private void BuildCardDetailPanel()
        {
            _cardDetailRoot = new GameObject("CardDetail", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            _cardDetailRoot.transform.SetParent(_canvas.transform, false);
            var rect = _cardDetailRoot.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-30, 0);
            rect.sizeDelta = new Vector2(380, 320);
            var image = _cardDetailRoot.GetComponent<Image>();
            image.color = new Color(0.08f, 0.1f, 0.14f, 0.95f);
            SetPanelBackground(_cardDetailRoot, SpriteLibrary.Ui("panel_frame", 8));
            var layout = _cardDetailRoot.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.spacing = 10;
            layout.padding = new RectOffset(16, 16, 16, 16);
            _cardDetailTitle = CreateText("", 28, _cardDetailRoot.transform);
            _cardDetailTitle.alignment = TextAnchor.MiddleCenter;
            _cardDetailDesc = CreateText("", 19, _cardDetailRoot.transform);
            _cardDetailDesc.alignment = TextAnchor.UpperLeft;
            _cardDetailRoot.SetActive(false);
        }

        private GameObject CreateVerticalPanel(string name, Transform parent, float width = 520, float height = 480)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, height);
            var bg = go.GetComponent<Image>();
            bg.color = new Color(0.1f, 0.12f, 0.16f, 0.96f);
            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.spacing = 16;
            return go;
        }

        private void SetPanelBackground(GameObject panel, Sprite sprite)
        {
            if (panel == null || sprite == null) return;
            var image = panel.GetComponent<Image>();
            if (image == null) return;
            image.sprite = sprite;
            image.color = Color.white;
            if (sprite.border != Vector4.zero)
            {
                image.type = Image.Type.Sliced;
            }
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
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);
            var normalSprite = SpriteLibrary.Ui("btn_normal");
            if (normalSprite != null)
            {
                image.sprite = normalSprite;
                image.color = Color.white;
                button.transition = Selectable.Transition.SpriteSwap;
                var state = new SpriteState();
                state.highlightedSprite = SpriteLibrary.Ui("btn_hover");
                state.pressedSprite = SpriteLibrary.Ui("btn_pressed");
                state.disabledSprite = SpriteLibrary.Ui("btn_disabled");
                button.spriteState = state;
            }
            else
            {
                image.color = new Color(0.18f, 0.28f, 0.38f, 1f);
            }
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
            if (_continueButton != null)
            {
                _continueButton.interactable = GameState.HasSave();
            }
        }

        private void ShowMap()
        {
            HideAllPanels();
            if (_mapRoot != null) _mapRoot.SetActive(true);
            RefreshMap();
        }

        private void RefreshMap()
        {
            for (var i = 0; i < _mapNodeImages.Count; i++)
            {
                if (i >= GameState.MapNodes.Count) break;
                var node = GameState.MapNodes[i];
                var image = _mapNodeImages[i];
                var sprite = SpriteLibrary.Node(node.type);
                var passed = i < GameState.CurrentNodeIndex;
                if (sprite != null)
                {
                    image.sprite = sprite;
                    image.color = i == GameState.CurrentNodeIndex
                        ? Color.white
                        : new Color(0.85f, 0.85f, 0.85f, 1f);
                }
                else
                {
                    image.sprite = null;
                    image.color = i == GameState.CurrentNodeIndex
                        ? new Color(1f, 0.85f, 0.3f, 1f)
                        : NodeColor(node.type);
                }
                if (passed)
                {
                    var dimmed = image.color;
                    dimmed.a = 0.45f;
                    image.color = dimmed;
                }
            }
            var current = GameState.CurrentNode;
            if (_mapStatus != null)
            {
                _mapStatus.text = current != null
                    ? string.Format("当前节点 {0}：{1}", current.index, NodeTypeName(current.type))
                    : "地图已走完";
            }
        }

        private static Color NodeColor(string type)
        {
            switch (type)
            {
                case "elite": return new Color(0.78f, 0.48f, 0.18f, 1f);
                case "event": return new Color(0.22f, 0.58f, 0.3f, 1f);
                case "rest": return new Color(0.22f, 0.45f, 0.78f, 1f);
                case "boss": return new Color(0.75f, 0.2f, 0.2f, 1f);
                default: return new Color(0.38f, 0.38f, 0.44f, 1f);
            }
        }

        private static string NodeTypeName(string type)
        {
            switch (type)
            {
                case "elite": return "精英";
                case "event": return "奇遇";
                case "rest": return "打坐";
                case "boss": return "Boss";
                default: return "战斗";
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
            if (_cardDetailRoot != null) _cardDetailRoot.SetActive(false);
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
            if (_battleTitle != null)
            {
                _battleTitle.text = node.type == "elite" ? "精英战" : (node.type == "boss" ? "Boss 战" : "战斗");
            }
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
            var enemySprite = _battleState.Enemy.Data != null
                ? SpriteLibrary.Enemy(_battleState.Enemy.Data.id)
                : null;
            if (enemySprite != null)
            {
                _enemyImage.sprite = enemySprite;
                _enemyImage.color = Color.white;
            }
            else
            {
                _enemyImage.sprite = null;
                _enemyImage.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            }
            var intentText = _battleState.Enemy.CurrentIntent != null
                ? DescribeIntent(_battleState.Enemy.CurrentIntent)
                : "未知";
            _enemyStatus.text = string.Format(
                "{0}\nHP {1}/{2}  罡气 {3}\n意图：{4}{5}",
                _battleState.Enemy.Name,
                Mathf.Max(0, _battleState.Enemy.CurrentHp),
                _battleState.Enemy.MaxHp,
                _battleState.Enemy.Block,
                intentText,
                FormatBuffs(_battleState.Enemy)
            );
            _playerStatus.text = string.Format(
                "掌门 · 回合 {6}\nHP {0}/{1}  罡气 {2}  灵力 {3}\n牌库 {4}  弃牌 {5}{7}",
                Mathf.Max(0, _battleState.Player.CurrentHp),
                _battleState.Player.MaxHp,
                _battleState.Player.Block,
                _battleState.Energy,
                _battleState.DrawPile.Count,
                _battleState.DiscardPile.Count,
                _battleState.TurnCount,
                FormatBuffs(_battleState.Player)
            );
            RebuildHand();
            _endTurnButton.interactable = _battleState.PlayerTurn && !_battleState.BattleOver;
            _battleContinueButton.gameObject.SetActive(_battleState.BattleOver);
            var logLines = new List<string>();
            var start = Mathf.Max(0, _battleState.Log.Count - 5);
            for (var i = start; i < _battleState.Log.Count; i++)
            {
                logLines.Add(_battleState.Log[i]);
            }
            _battleLog.text = _battleState.BattleOver
                ? (_battleState.PlayerWon ? "战斗胜利！\n" : "战斗失败...\n") + string.Join("\n", logLines)
                : string.Join("\n", logLines);
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
                var entry = _battleState.Hand[i];
                var card = DataLoader.GetCard(GameState.BaseCardId(entry));
                var upgraded = GameState.IsUpgraded(entry);
                var index = i;
                var desc = card != null ? (upgraded ? card.desc_up : card.desc) : "";
                var label = card != null
                    ? string.Format("{0}（{1}费）{2}\n{3}", card.name, card.cost, upgraded ? "✦" : "", desc)
                    : "?";
                var button = CreateButton(label, _handRoot, () => OnCardPressed(index));
                button.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 86);
                button.interactable = _battleState.PlayerTurn && !_battleState.BattleOver && card.cost <= _battleState.Energy;
                var text = button.GetComponentInChildren<Text>();
                text.fontSize = 17;
                text.alignment = TextAnchor.MiddleCenter;
                ApplyCardIcon(button, GameState.BaseCardId(entry));
                WireHover(button, card, upgraded);
                _handButtons.Add(button);
            }
        }

        private void WireHover(Button button, CardData card, bool upgraded, string extra = "")
        {
            var trigger = button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = button.gameObject.AddComponent<EventTrigger>();
            }
            var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener(_ => ShowCardDetail(card, upgraded, extra));
            var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener(_ => HideCardDetail());
            trigger.triggers.Add(enter);
            trigger.triggers.Add(exit);
        }

        private void ShowCardDetail(CardData card, bool upgraded, string extra)
        {
            if (card == null || _cardDetailRoot == null) return;
            _cardDetailTitle.text = string.Format(
                "{0}（{1}费 · {2}）{3}",
                card.name,
                card.cost,
                RarityName(card.rarity),
                upgraded ? " ✦已祭炼" : ""
            );
            _cardDetailDesc.text = (extra.Length > 0 ? extra + "\n\n" : "")
                + (upgraded ? card.desc_up : card.desc)
                + "\n\n" + card.flavor;
            _cardDetailRoot.SetActive(true);
        }

        private void HideCardDetail()
        {
            if (_cardDetailRoot != null) _cardDetailRoot.SetActive(false);
        }

        private void ApplyCardIcon(Button button, string cardId)
        {
            var icon = SpriteLibrary.Card(cardId);
            if (icon == null) return;
            var bg = button.GetComponent<Image>();
            bg.sprite = icon;
            bg.color = Color.white;
        }

        private void OnCardPressed(int handIndex)
        {
            if (_battleState == null) return;
            var entry = _battleState.Hand[handIndex];
            var card = DataLoader.GetCard(GameState.BaseCardId(entry));
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
                ApplyCardIcon(button, _pendingCardOffers[index]);
                WireHover(button, card, false);
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

        private static string BuffName(string buff)
        {
            switch (buff)
            {
                case "poison": return "毒煞";
                case "weak": return "气滞";
                case "vulnerable": return "破绽";
                case "strength": return "剑意";
                case "dexterity": return "身法";
                case "thorns": return "反噬";
                case "regen": return "生生不息";
                default: return buff;
            }
        }

        private string FormatBuffs(Combatant combatant)
        {
            var buffs = combatant.GetBuffs();
            if (buffs.Count == 0) return "";
            var parts = new List<string>();
            foreach (var kv in buffs)
            {
                parts.Add(BuffName(kv.Key) + kv.Value);
            }
            return "\n状态：" + string.Join(" ", parts);
        }

        private string DescribeIntent(EnemyIntent intent)
        {
            if (intent == null) return "未知";
            var parts = new List<string>
            {
                DescribeIntentAction(intent.action, intent.value, intent.times, intent.buff, intent.stacks)
            };
            if (!string.IsNullOrEmpty(intent.action2))
            {
                parts.Add(DescribeIntentAction(intent.action2, intent.value2, intent.times2, intent.buff2, intent.stacks2));
            }
            return string.Join(" + ", parts);
        }

        private static string DescribeIntentAction(string action, int value, int times, string buff, int stacks)
        {
            switch (action)
            {
                case "attack": return "攻击 " + value;
                case "heavy_attack": return "重击 " + value;
                case "multi_attack": return "连击 " + Mathf.Max(1, times) + "×" + value;
                case "block": return "防御 " + value;
                case "buff":
                    return "强化 " + (stacks > 0 ? stacks : 1) + " " + BuffName(string.IsNullOrEmpty(buff) ? "strength" : buff);
                case "debuff":
                    return "削弱 " + (stacks > 0 ? stacks : 1) + " " + BuffName(string.IsNullOrEmpty(buff) ? "weak" : buff);
                default: return action;
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
                "灵石 {0}  药材 {1}  生命 {2}/{3}\n已走完节点 {4} / 20",
                GameState.SpiritStone,
                GameState.Herb,
                GameState.CurrentHp,
                GameState.MaxHp,
                GameState.CurrentNodeIndex + 1
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

        private void ShowDeck()
        {
            RebuildDeckCards();
            HideAllPanels();
            _deckRoot.SetActive(true);
        }

        private void RebuildDeckCards()
        {
            foreach (var row in _deckRows)
            {
                if (row != null) Destroy(row.gameObject);
            }
            _deckRows.Clear();

            foreach (var entry in GameState.Deck)
            {
                var card = DataLoader.GetCard(GameState.BaseCardId(entry));
                if (card == null) continue;
                var label = string.Format(
                    "{0}（{1}费 · {2}）{3}",
                    card.name,
                    card.cost,
                    RarityName(card.rarity),
                    GameState.IsUpgraded(entry) ? " ✦已祭炼" : ""
                );
                var row = CreateText(label, 20, _deckCardRoot);
                row.rectTransform.sizeDelta = new Vector2(600, 34);
                _deckRows.Add(row);
            }
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
                ApplyCardIcon(button, _shopOffers[index]);
                WireHover(button, card, false, "价格 " + price + " 灵石");
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
