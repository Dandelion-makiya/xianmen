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
        private Text _mapStatus;

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
            CreateButton("进入节点（待接入战斗）", _mapRoot.transform, OnEnterNodePressed);
            CreateButton("返回主界面", _mapRoot.transform, ShowMenu);
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
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
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
            if (_menuRoot != null) _menuRoot.SetActive(true);
            if (_mapRoot != null) _mapRoot.SetActive(false);
        }

        private void ShowMap()
        {
            if (_menuRoot != null) _menuRoot.SetActive(false);
            if (_mapRoot != null) _mapRoot.SetActive(true);
            if (_mapStatus != null)
            {
                _mapStatus.text = string.Format("当前节点：{0} / 20", GameState.CurrentNodeIndex + 1);
            }
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
            // TODO: 按节点类型进入战斗 / 事件 / 打坐。
            Debug.Log("节点进入逻辑待实现：" + (GameState.CurrentNode == null ? "null" : GameState.CurrentNode.type));
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
