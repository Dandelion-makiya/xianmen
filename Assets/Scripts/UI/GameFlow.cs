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
        private Text _mapStatus;
        private Text _enemyStatus;
        private Text _playerStatus;
        private Text _battleLog;
        private Transform _handRoot;
        private Button _endTurnButton;
        private Button _battleContinueButton;
        private readonly List<Button> _handButtons = new List<Button>();

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
            _battleContinueButton = CreateButton("返回地图", _battleRoot.transform, OnBattleContinuePressed);
            _battleContinueButton.gameObject.SetActive(false);
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
                    GameState.AdvanceNode();
                    ShowMap();
                    break;
                case "event":
                    _mapStatus.text = "奇遇事件待接入";
                    break;
            }
        }

        private void StartBattle()
        {
            var node = GameState.CurrentNode;
            if (node == null) return;
            var enemyId = node.type == "boss" ? "mo_zun" : "ni_zhao_jing";
            var enemy = DataLoader.GetEnemy(enemyId);
            if (enemy == null)
            {
                Debug.LogError("Enemy data missing: " + enemyId);
                return;
            }
            _battleState = new BattleState(GameState.Deck, enemy);
            ShowBattle();
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
                    _mapStatus.text = "通关结算待实现";
                    ShowMenu();
                    return;
                }
                GameState.AdvanceNode();
                ShowMap();
            }
            else
            {
                GameState.StartNewRun();
                ShowMenu();
            }
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
