using System.Collections.Generic;
using UnityEngine;

namespace Xianmen
{
    public static class DataLoader
    {
        public static Dictionary<string, CardData> Cards { get; private set; } = new Dictionary<string, CardData>();
        public static Dictionary<string, EnemyData> Enemies { get; private set; } = new Dictionary<string, EnemyData>();
        public static Dictionary<string, RelicData> Relics { get; private set; } = new Dictionary<string, RelicData>();
        public static Dictionary<string, EventData> Events { get; private set; } = new Dictionary<string, EventData>();
        public static MapConfig MapConfig { get; private set; }

        public static CardData GetCard(string id)
        {
            return Cards.TryGetValue(id, out var card) ? card : null;
        }

        public static EnemyData GetEnemy(string id)
        {
            return Enemies.TryGetValue(id, out var enemy) ? enemy : null;
        }

        public static RelicData GetRelic(string id)
        {
            return Relics.TryGetValue(id, out var relic) ? relic : null;
        }

        public static void LoadAll()
        {
            LoadCards();
            LoadEnemies();
            LoadRelics();
            LoadEvents();
            LoadMapConfig();
        }

        private static void LoadCards()
        {
            var asset = Resources.Load<TextAsset>("Data/cards");
            if (asset == null)
            {
                Debug.LogError("Data/cards.json missing");
                return;
            }
            var wrapper = JsonUtility.FromJson<CardList>(asset.text);
            if (wrapper == null || wrapper.cards == null) return;
            foreach (var card in wrapper.cards)
            {
                if (card != null && !string.IsNullOrEmpty(card.id))
                {
                    Cards[card.id] = card;
                }
            }
        }

        private static void LoadEnemies()
        {
            var asset = Resources.Load<TextAsset>("Data/enemies");
            if (asset == null)
            {
                Debug.LogError("Data/enemies.json missing");
                return;
            }
            var wrapper = JsonUtility.FromJson<EnemyList>(asset.text);
            if (wrapper == null || wrapper.enemies == null) return;
            foreach (var enemy in wrapper.enemies)
            {
                if (enemy != null && !string.IsNullOrEmpty(enemy.id))
                {
                    Enemies[enemy.id] = enemy;
                }
            }
        }

        private static void LoadRelics()
        {
            var asset = Resources.Load<TextAsset>("Data/relics");
            if (asset == null)
            {
                Debug.LogError("Data/relics.json missing");
                return;
            }
            var wrapper = JsonUtility.FromJson<RelicList>(asset.text);
            if (wrapper == null || wrapper.relics == null) return;
            foreach (var relic in wrapper.relics)
            {
                if (relic != null && !string.IsNullOrEmpty(relic.id))
                {
                    Relics[relic.id] = relic;
                }
            }
        }

        private static void LoadEvents()
        {
            var asset = Resources.Load<TextAsset>("Data/events");
            if (asset == null)
            {
                Debug.LogError("Data/events.json missing");
                return;
            }
            var wrapper = JsonUtility.FromJson<EventList>(asset.text);
            if (wrapper == null || wrapper.events == null) return;
            foreach (var eventData in wrapper.events)
            {
                if (eventData != null && !string.IsNullOrEmpty(eventData.id))
                {
                    Events[eventData.id] = eventData;
                }
            }
        }

        private static void LoadMapConfig()
        {
            var asset = Resources.Load<TextAsset>("Data/map_config");
            if (asset == null)
            {
                Debug.LogError("Data/map_config.json missing");
                return;
            }
            var wrapper = JsonUtility.FromJson<MapConfigWrapper>(asset.text);
            MapConfig = wrapper == null ? null : wrapper.map_config;
        }
    }
}
