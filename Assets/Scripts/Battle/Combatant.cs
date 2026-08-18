using System.Collections.Generic;
using UnityEngine;

namespace Xianmen
{
    public class Combatant
    {
        public string Name;
        public int MaxHp;
        public int CurrentHp;
        public int Block;
        public bool IsPlayer;
        public EnemyData Data;
        public int IntentIndex;

        private readonly Dictionary<string, int> _buffs = new Dictionary<string, int>();

        public Combatant(string name, int maxHp, bool isPlayer)
        {
            Name = name;
            MaxHp = maxHp;
            CurrentHp = maxHp;
            IsPlayer = isPlayer;
        }

        public EnemyIntent CurrentIntent
        {
            get
            {
                if (Data == null || Data.intents == null || Data.intents.Count == 0) return null;
                return Data.intents[IntentIndex % Data.intents.Count];
            }
        }

        public int GetBuff(string buff)
        {
            return _buffs.TryGetValue(buff, out var value) ? value : 0;
        }

        public List<KeyValuePair<string, int>> GetBuffs()
        {
            var list = new List<KeyValuePair<string, int>>();
            foreach (var kv in _buffs)
            {
                list.Add(kv);
            }
            return list;
        }

        public void AddBuff(string buff, int stacks)
        {
            if (stacks <= 0) return;
            _buffs[buff] = GetBuff(buff) + stacks;
        }

        public void RemoveBuff(string buff)
        {
            _buffs.Remove(buff);
        }

        public void TickTurnStart()
        {
            var poison = GetBuff("poison");
            if (poison > 0)
            {
                CurrentHp -= poison;
                _buffs["poison"] = poison - 1;
                if (_buffs["poison"] <= 0) _buffs.Remove("poison");
            }
        }

        public void TickTurnEnd()
        {
            var regen = GetBuff("regen");
            if (regen > 0)
            {
                CurrentHp = Mathf.Min(MaxHp, CurrentHp + regen);
                _buffs["regen"] = regen - 1;
                if (_buffs["regen"] <= 0) _buffs.Remove("regen");
            }

            foreach (var durationBuff in new[] { "weak", "vulnerable", "thorns" })
            {
                var value = GetBuff(durationBuff);
                if (value > 0)
                {
                    _buffs[durationBuff] = value - 1;
                    if (_buffs[durationBuff] <= 0) _buffs.Remove(durationBuff);
                }
            }
        }

        public void ClearBlock()
        {
            Block = 0;
        }

        public bool IsDead
        {
            get { return CurrentHp <= 0; }
        }
    }
}
