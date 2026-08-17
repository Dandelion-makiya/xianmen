using UnityEngine;

namespace Xianmen
{
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (GameFlow.Instance == null)
            {
                var go = new GameObject("GameFlow");
                go.AddComponent<GameFlow>();
            }
        }
    }
}
