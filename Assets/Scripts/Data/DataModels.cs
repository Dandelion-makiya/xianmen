using System;
using System.Collections.Generic;

namespace Xianmen
{
    [Serializable]
    public class MapNode
    {
        public int index;
        public string type;
    }

    [Serializable]
    public class Conditional
    {
        public string when;
        public int value;
    }

    [Serializable]
    public class CardEffect
    {
        public string type;
        public int value;
        public int times;
        public string buff;
        public int stacks;
        public Conditional conditional;
    }

    [Serializable]
    public class CardData
    {
        public string id;
        public string name;
        public string name_en;
        public string rarity;
        public int cost;
        public string type;
        public string target;
        public List<CardEffect> effects;
        public List<CardEffect> upgrade_effects;
        public string desc;
        public string desc_up;
        public string flavor;
    }

    [Serializable]
    public class CardList
    {
        public List<CardData> cards;
    }

    [Serializable]
    public class EnemyIntent
    {
        public string action;
        public int value;
        public int times;
        public string buff;
        public int stacks;
    }

    [Serializable]
    public class EnemyData
    {
        public string id;
        public string name;
        public string type;
        public int hp;
        public List<EnemyIntent> intents;
        public string mechanic;
        public string lore;
    }

    [Serializable]
    public class EnemyList
    {
        public List<EnemyData> enemies;
    }

    [Serializable]
    public class RelicEffect
    {
        public string type;
        public string resource;
        public float value;
    }

    [Serializable]
    public class RelicData
    {
        public string id;
        public string name;
        public string desc;
        public RelicEffect effect;
        public string flavor;
    }

    [Serializable]
    public class RelicList
    {
        public List<RelicData> relics;
    }

    [Serializable]
    public class EventCost
    {
        public string resource;
        public int value;
    }

    [Serializable]
    public class EventEffect
    {
        public string type;
        public int value;
        public string resource;
        public string rarity;
    }

    [Serializable]
    public class EventOption
    {
        public string text;
        public EventEffect effect;
        public EventCost cost;
    }

    [Serializable]
    public class EventData
    {
        public string id;
        public string name;
        public string text;
        public List<EventOption> options;
    }

    [Serializable]
    public class EventList
    {
        public List<EventData> events;
    }

    [Serializable]
    public class RarityRule
    {
        public int from;
        public int to;
        public List<string> rarities;
    }

    [Serializable]
    public class MapConfig
    {
        public int total_nodes;
        public int first_battle_node;
        public int rest_node;
        public int boss_node;
        public List<IntRange> elite_windows;
        public int event_count;
        public int max_consecutive_battles;
        public List<RarityRule> rarity_by_node;
    }

    [Serializable]
    public class IntRange
    {
        public int from;
        public int to;
    }

    [Serializable]
    public class MapConfigWrapper
    {
        public MapConfig map_config;
    }
}
