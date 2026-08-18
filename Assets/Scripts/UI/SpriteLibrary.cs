using System.Collections.Generic;
using UnityEngine;

namespace Xianmen
{
    /// <summary>
    /// 从 Resources/Art/ 加载美术贴图，缺图返回 null，调用方回退到占位样式。
    /// 命名约定：cards/card_{id}.png, enemies/enemy_{id}.png, intents/intent_{action}.png,
    /// nodes/node_{type}.png, relics/relic_{id}.png, frames/frame_{rarity}.png
    /// </summary>
    public static class SpriteLibrary
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Card(string cardId)
        {
            return Get("cards/card_" + cardId);
        }

        public static Sprite Enemy(string enemyId)
        {
            return Get("enemies/enemy_" + enemyId);
        }

        public static Sprite Intent(string action)
        {
            return Get("intents/intent_" + action);
        }

        public static Sprite Node(string type)
        {
            return Get("nodes/node_" + type);
        }

        public static Sprite Relic(string relicId)
        {
            return Get("relics/relic_" + relicId);
        }

        public static Sprite Frame(string rarity)
        {
            return Get("frames/frame_" + rarity);
        }

        public static Sprite Background(string name)
        {
            return Get("backgrounds/bg_" + name);
        }

        public static Sprite Ui(string name, int border = 0)
        {
            return Get("ui/" + name, border);
        }

        private static Sprite Get(string path, int border = 0)
        {
            if (Cache.TryGetValue(path, out var cached)) return cached;
            var texture = Resources.Load<Texture2D>("Art/" + path);
            if (texture == null) return null;
            var rect = new Rect(0, 0, texture.width, texture.height);
            var pivot = new Vector2(0.5f, 0.5f);
            var sprite = border > 0
                ? Sprite.Create(texture, rect, pivot, 100f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border))
                : Sprite.Create(texture, rect, pivot);
            Cache[path] = sprite;
            return sprite;
        }
    }
}
