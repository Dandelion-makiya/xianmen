using System.Collections.Generic;
using UnityEngine;

namespace Xianmen
{
    public class BattleState
    {
        public const int HandLimit = 10;
        public const int StartEnergy = 3;
        public const int DrawPerTurn = 5;

        public Combatant Player;
        public Combatant Enemy;
        public List<string> DrawPile = new List<string>();
        public List<string> DiscardPile = new List<string>();
        public List<string> Hand = new List<string>();
        public int Energy;
        public int TurnCount;
        public bool PlayerTurn;
        public bool BattleOver;
        public bool PlayerWon;
        public bool FirstAttackPlayedThisTurn;
        public bool AttackPlayedThisTurn;
        public readonly List<string> Log = new List<string>();

        private readonly System.Random _rng = new System.Random();
        private bool _startRelicsApplied;

        public BattleState(List<string> deck, EnemyData enemyData)
        {
            Player = new Combatant("掌门", GameState.MaxHp, true);
            Player.CurrentHp = GameState.CurrentHp;
            Enemy = new Combatant(enemyData.name, enemyData.hp, false);
            Enemy.Data = enemyData;
            Enemy.IntentIndex = 0;
            DrawPile = new List<string>(deck);
            Shuffle(DrawPile);
            StartPlayerTurn();
        }

        public static EnemyData ScaleForNode(EnemyData data, int nodeIndex)
        {
            if (data == null || data.type == "boss") return data;
            var ratio = Mathf.Min(1.8f, 1f + 0.05f * (nodeIndex - 1));
            var clone = new EnemyData
            {
                id = data.id,
                name = data.name,
                type = data.type,
                hp = Mathf.FloorToInt(data.hp * ratio),
                mechanic = data.mechanic,
                lore = data.lore,
                intents = new List<EnemyIntent>()
            };
            foreach (var intent in data.intents)
            {
                clone.intents.Add(new EnemyIntent
                {
                    action = intent.action,
                    value = ScaleIntentValue(intent.action, intent.value, ratio),
                    times = intent.times,
                    buff = intent.buff,
                    stacks = intent.stacks,
                    action2 = intent.action2,
                    value2 = ScaleIntentValue(intent.action2, intent.value2, ratio),
                    times2 = intent.times2,
                    buff2 = intent.buff2,
                    stacks2 = intent.stacks2
                });
            }
            return clone;
        }

        private static int ScaleIntentValue(string action, int value, float ratio)
        {
            if (string.IsNullOrEmpty(action) || value <= 0) return value;
            if (action == "attack" || action == "heavy_attack" || action == "multi_attack")
            {
                return Mathf.FloorToInt(value * ratio);
            }
            return value;
        }

        public void StartPlayerTurn()
        {
            if (BattleOver) return;
            TurnCount++;
            PlayerTurn = true;
            AttackPlayedThisTurn = false;
            FirstAttackPlayedThisTurn = false;
            Player.ClearBlock();
            var hpBefore = Player.CurrentHp;
            Player.TickTurnStart();
            if (Player.CurrentHp < hpBefore)
            {
                AddLog(string.Format("毒煞发作，掌门受到 {0} 伤害", hpBefore - Player.CurrentHp));
            }
            if (CheckBattleEnd()) return;
            Energy = StartEnergy;
            if (!_startRelicsApplied)
            {
                _startRelicsApplied = true;
                ApplyStartRelics();
            }
            DrawCards(DrawPerTurn);
        }

        private void ApplyStartRelics()
        {
            if (GameState.Relics.Contains("shoushan_dazhen"))
            {
                Player.Block += 3;
                AddLog("守山大阵运转，获得 3 点罡气");
            }
            if (GameState.Relics.Contains("yanwu_chang"))
            {
                Energy += 1;
                AddLog("演武场士气高昂，额外获得 1 点灵力");
            }
        }

        public bool PlayCard(CardData card, int handIndex)
        {
            if (!PlayerTurn || BattleOver || card == null) return false;
            if (handIndex < 0 || handIndex >= Hand.Count) return false;
            if (card.cost > Energy) return false;

            var entry = Hand[handIndex];
            var upgraded = GameState.IsUpgraded(entry);
            Energy -= card.cost;
            Hand.RemoveAt(handIndex);
            DiscardPile.Add(entry);

            var attackPlayedBefore = AttackPlayedThisTurn;
            var damageBonus = 0;
            if (card.type == "attack")
            {
                if (!FirstAttackPlayedThisTurn)
                {
                    FirstAttackPlayedThisTurn = true;
                    if (GameState.Relics.Contains("tianji_pan"))
                    {
                        damageBonus = 2;
                    }
                }
                AttackPlayedThisTurn = true;
            }

            ExecuteCardEffects(upgraded ? card.upgrade_effects : card.effects, card.target, damageBonus, attackPlayedBefore);
            CheckBattleEnd();
            return true;
        }

        public void EndPlayerTurn()
        {
            if (!PlayerTurn || BattleOver) return;
            PlayerTurn = false;
            foreach (var cardId in Hand)
            {
                DiscardPile.Add(cardId);
            }
            Hand.Clear();
            Energy = 0;
            Player.TickTurnEnd();
            if (CheckBattleEnd()) return;
            ExecuteEnemyTurn();
            if (!BattleOver)
            {
                StartPlayerTurn();
            }
        }

        private void ExecuteEnemyTurn()
        {
            var hpBefore = Enemy.CurrentHp;
            Enemy.TickTurnStart();
            if (Enemy.CurrentHp < hpBefore)
            {
                AddLog(string.Format("毒煞发作，{0} 受到 {1} 伤害", Enemy.Name, hpBefore - Enemy.CurrentHp));
            }
            if (CheckBattleEnd()) return;

            var intent = Enemy.CurrentIntent;
            if (intent == null) return;

            ExecuteIntent(intent.action, intent.value, intent.times, intent.buff, intent.stacks);
            if (!string.IsNullOrEmpty(intent.action2))
            {
                ExecuteIntent(intent.action2, intent.value2, intent.times2, intent.buff2, intent.stacks2);
            }

            Enemy.IntentIndex++;
            Enemy.TickTurnEnd();
            CheckBattleEnd();
        }

        private void ExecuteIntent(string action, int value, int times, string buff, int stacks)
        {
            switch (action)
            {
                case "attack":
                case "heavy_attack":
                    DealDamage(Enemy, Player, value);
                    ApplyEnemyAttackMechanic();
                    break;
                case "multi_attack":
                    var hitTimes = Mathf.Max(1, times);
                    for (var i = 0; i < hitTimes; i++)
                    {
                        DealDamage(Enemy, Player, value);
                    }
                    ApplyEnemyAttackMechanic();
                    break;
                case "block":
                    Enemy.Block += value;
                    AddLog(string.Format("{0} 防御，获得 {1} 点罡气", Enemy.Name, value));
                    break;
                case "buff":
                    Enemy.AddBuff(
                        string.IsNullOrEmpty(buff) ? "strength" : buff,
                        stacks > 0 ? stacks : 1
                    );
                    AddLog(string.Format("{0} 强化自身", Enemy.Name));
                    break;
                case "debuff":
                    Player.AddBuff(
                        string.IsNullOrEmpty(buff) ? "weak" : buff,
                        stacks > 0 ? stacks : 1
                    );
                    AddLog(string.Format("{0} 对掌门施加负面状态", Enemy.Name));
                    break;
            }
        }

        private void ApplyEnemyAttackMechanic()
        {
            if (Enemy.Data != null && Enemy.Data.id == "shi_kui")
            {
                Enemy.AddBuff("strength", 1);
                AddLog("尸傀越战越勇，剑意 +1");
            }
        }

        private void ExecuteCardEffects(List<CardEffect> effects, string targetType, int damageBonus, bool attackPlayedBefore)
        {
            if (effects == null) return;
            foreach (var effect in effects)
            {
                var conditionalMet = effect.conditional != null && IsConditionMet(effect.conditional.when, attackPlayedBefore);
                var value = conditionalMet ? effect.conditional.value : effect.value;

                switch (effect.type)
                {
                    case "damage":
                        DealDamage(Player, Enemy, value + damageBonus);
                        break;
                    case "multi_hit":
                        var times = Mathf.Max(1, effect.times);
                        for (var i = 0; i < times; i++)
                        {
                            DealDamage(Player, Enemy, value + (i == 0 ? damageBonus : 0));
                        }
                        break;
                    case "block":
                        Player.Block += value + Player.GetBuff("dexterity");
                        AddLog(string.Format("获得 {0} 点罡气", value + Player.GetBuff("dexterity")));
                        break;
                    case "draw":
                        DrawCards(value);
                        break;
                    case "energy":
                        Energy += value;
                        AddLog(string.Format("获得 {0} 点灵力", value));
                        break;
                    case "heal":
                        Player.CurrentHp = Mathf.Min(Player.MaxHp, Player.CurrentHp + value);
                        AddLog(string.Format("回复 {0} 点生命", value));
                        break;
                    case "discard":
                        DiscardCards(value);
                        break;
                    case "apply_buff":
                        var target = targetType == "self" || targetType == "player" ? Player : Enemy;
                        var stacks = conditionalMet && effect.value != 0 ? value : effect.stacks;
                        target.AddBuff(effect.buff, stacks);
                        AddLog(string.Format("对 {0} 施加 {1} 层{2}", target.Name, stacks, BuffLabel(effect.buff)));
                        break;
                    case "cleanse":
                        Player.RemoveBuff("poison");
                        Player.RemoveBuff("weak");
                        Player.RemoveBuff("vulnerable");
                        AddLog("清除全部负面状态");
                        break;
                }
            }
        }

        private bool IsConditionMet(string when, bool attackPlayedBefore)
        {
            if (when == "target_has_poison") return Enemy.GetBuff("poison") > 0;
            if (when == "played_attack_this_turn") return attackPlayedBefore;
            return false;
        }

        private void DealDamage(Combatant source, Combatant target, int baseDamage)
        {
            var damage = baseDamage + source.GetBuff("strength");
            if (source.GetBuff("weak") > 0)
            {
                damage = Mathf.FloorToInt(damage * 0.75f);
            }
            if (target.GetBuff("vulnerable") > 0)
            {
                damage = Mathf.FloorToInt(damage * 1.5f);
            }
            damage = Mathf.Max(0, damage);

            var absorbed = Mathf.Min(target.Block, damage);
            target.Block -= absorbed;
            target.CurrentHp -= damage - absorbed;
            AddLog(string.Format(
                "{0} 对 {1} 造成 {2} 伤害（罡气抵消 {3}）",
                source.Name,
                target.Name,
                damage - absorbed,
                absorbed
            ));

            var thorns = target.GetBuff("thorns");
            if (thorns > 0)
            {
                source.CurrentHp -= thorns;
                AddLog(string.Format("{0} 受反噬 {1} 伤害", source.Name, thorns));
            }
        }

        private static string BuffLabel(string buff)
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

        private void AddLog(string message)
        {
            Log.Add(message);
            if (Log.Count > 40) Log.RemoveAt(0);
        }

        private void DrawCards(int count)
        {
            for (var i = 0; i < count; i++)
            {
                if (Hand.Count >= HandLimit) break;
                if (DrawPile.Count == 0)
                {
                    if (DiscardPile.Count == 0) break;
                    DrawPile.AddRange(DiscardPile);
                    DiscardPile.Clear();
                    Shuffle(DrawPile);
                }
                Hand.Add(DrawPile[0]);
                DrawPile.RemoveAt(0);
            }
        }

        private void DiscardCards(int count)
        {
            var amount = Mathf.Min(count, Hand.Count);
            for (var i = 0; i < amount; i++)
            {
                var index = _rng.Next(Hand.Count);
                DiscardPile.Add(Hand[index]);
                Hand.RemoveAt(index);
            }
        }

        private void Shuffle(List<string> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = _rng.Next(i + 1);
                var tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }

        private bool CheckBattleEnd()
        {
            if (Enemy.IsDead)
            {
                FinishBattle(true);
                return true;
            }
            if (Player.IsDead)
            {
                FinishBattle(false);
                return true;
            }
            return false;
        }

        private void FinishBattle(bool playerWon)
        {
            BattleOver = true;
            PlayerTurn = false;
            PlayerWon = playerWon;
            if (playerWon)
            {
                GameState.CurrentHp = Player.CurrentHp;
            }
        }
    }
}
