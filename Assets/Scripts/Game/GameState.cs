using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Xianmen
{
    public static class GameState
    {
        public const int StartMaxHp = 70;
        public const int StartMaxEnergy = 3;
        public const int StartHandSize = 5;

        public static readonly List<string> StartDeck = new List<string>
        {
            "yujian_shu", "yujian_shu", "yujian_shu", "yujian_shu", "yujian_shu",
            "gangqi_huti", "gangqi_huti", "gangqi_huti", "gangqi_huti",
            "pojia_jian"
        };

        public static int MaxHp = StartMaxHp;
        public static int CurrentHp = StartMaxHp;
        public static int SpiritStone;
        public static int Herb;
        public static List<string> Deck = new List<string>();
        public static List<string> Relics = new List<string>();
        public static List<string> PendingRelics = new List<string>();
        public static int CurrentNodeIndex;
        public static List<MapNode> MapNodes = new List<MapNode>();
        public static bool InRun;

        private static string SavePath
        {
            get { return Path.Combine(Application.persistentDataPath, "save_run.json"); }
        }

        public static MapNode CurrentNode
        {
            get
            {
                if (CurrentNodeIndex < 0 || CurrentNodeIndex >= MapNodes.Count)
                {
                    return null;
                }
                return MapNodes[CurrentNodeIndex];
            }
        }

        public static void StartNewRun()
        {
            MaxHp = StartMaxHp;
            CurrentHp = StartMaxHp;
            SpiritStone = 0;
            Herb = 0;
            Deck = new List<string>(StartDeck);
            Relics = new List<string>();
            PendingRelics = new List<string>();
            CurrentNodeIndex = 0;
            MapNodes = MapGenerator.Generate();
            InRun = true;
            Save();
        }

        public static void AdvanceNode()
        {
            CurrentNodeIndex++;
            Save();
        }

        public static void AddResources(int stones, int herbs)
        {
            SpiritStone = Math.Max(0, SpiritStone + stones);
            Herb = Math.Max(0, Herb + herbs);
            Save();
        }

        public static void AddCardToDeck(string cardId)
        {
            Deck.Add(cardId);
            Save();
        }

        public static void AddRelic(string relicId)
        {
            if (Relics.Contains(relicId)) return;
            Relics.Add(relicId);
            Save();
        }

        public static string BaseCardId(string entry)
        {
            return string.IsNullOrEmpty(entry) ? entry : (entry.EndsWith("+") ? entry.Substring(0, entry.Length - 1) : entry);
        }

        public static bool IsUpgraded(string entry)
        {
            return !string.IsNullOrEmpty(entry) && entry.EndsWith("+");
        }

        public static bool UpgradeCardAt(int deckIndex)
        {
            if (deckIndex < 0 || deckIndex >= Deck.Count) return false;
            var entry = Deck[deckIndex];
            if (IsUpgraded(entry)) return false;
            Deck[deckIndex] = entry + "+";
            Save();
            return true;
        }

        public static void EndRun()
        {
            InRun = false;
            try
            {
                if (File.Exists(SavePath)) File.Delete(SavePath);
            }
            catch (Exception e)
            {
                Debug.LogError("EndRun cleanup failed: " + e.Message);
            }
        }

        public static bool HasSave()
        {
            return File.Exists(SavePath);
        }

        public static void Save()
        {
            if (!InRun) return;
            var data = new SaveData
            {
                max_hp = MaxHp,
                current_hp = CurrentHp,
                spirit_stone = SpiritStone,
                herb = Herb,
                deck = Deck,
                relics = Relics,
                pending_relics = PendingRelics,
                current_node_index = CurrentNodeIndex,
                map_nodes = MapNodes
            };
            File.WriteAllText(SavePath, JsonUtility.ToJson(data));
        }

        public static bool Load()
        {
            if (!HasSave()) return false;
            try
            {
                var json = File.ReadAllText(SavePath);
                var data = JsonUtility.FromJson<SaveData>(json);
                if (data == null) return false;
                MaxHp = data.max_hp;
                CurrentHp = data.current_hp;
                SpiritStone = data.spirit_stone;
                Herb = data.herb;
                Deck = data.deck ?? new List<string>();
                Relics = data.relics ?? new List<string>();
                PendingRelics = data.pending_relics ?? new List<string>();
                CurrentNodeIndex = data.current_node_index;
                MapNodes = data.map_nodes ?? new List<MapNode>();
                InRun = true;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Load run failed: " + e.Message);
                return false;
            }
        }
    }

    [Serializable]
    public class SaveData
    {
        public int max_hp;
        public int current_hp;
        public int spirit_stone;
        public int herb;
        public List<string> deck;
        public List<string> relics;
        public List<string> pending_relics;
        public int current_node_index;
        public List<MapNode> map_nodes;
    }
}
