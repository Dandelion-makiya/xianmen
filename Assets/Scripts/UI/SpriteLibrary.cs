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

        private static Sprite Get(string path)
        {
            if (Cache.TryGetValue(path, out var cached)) return cached;
            var texture = Resources.Load<Texture2D>("Art/" + path);
            if (texture == null) return null;
            var sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
            Cache[path] = sprite;
            return sprite;
        }
    }
}
