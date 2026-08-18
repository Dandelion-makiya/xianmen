using System.Collections.Generic;

namespace Xianmen
{
    public struct RewardResult
    {
        public int stones;
        public int herbs;
        public int heal;
        public List<string> cardOffers;
        public string relic;
    }

    public static class RewardSystem
    {
        public static RewardResult RollRewards(string nodeType, int nodeIndex, List<string> relics, System.Random rng)
        {
            var result = new RewardResult { cardOffers = new List<string>() };

            if (nodeType == "battle")
            {
                result.stones = rng.Next(2, 4);
                result.herbs = rng.Next(1, 3);
            }
            else if (nodeType == "elite")
            {
                result.stones = 5;
                result.herbs = 3;
                result.relic = RollRelic(rng);
            }
            else
            {
                return result;
            }

            if (relics != null)
            {
                foreach (var relicId in relics)
                {
                    var relic = DataLoader.GetRelic(relicId);
                    if (relic == null || relic.effect == null) continue;
                    switch (relic.effect.type)
                    {
                        case "battle_reward":
                            if (relic.effect.resource == "spirit_stone")
                            {
                                result.stones += (int)relic.effect.value;
                            }
                            else if (relic.effect.resource == "herb")
                            {
                                result.herbs += (int)relic.effect.value;
                            }
                            break;
                        case "battle_reward_heal":
                            result.heal += (int)relic.effect.value;
                            break;
                    }
                }
            }

            result.cardOffers = RollCards(nodeIndex, 3, rng);
            return result;
        }

        private static List<string> RollCards(int nodeIndex, int count, System.Random rng)
        {
            var allowed = new HashSet<string> { "basic" };
            if (DataLoader.MapConfig != null && DataLoader.MapConfig.rarity_by_node != null)
            {
                foreach (var rule in DataLoader.MapConfig.rarity_by_node)
                {
                    if (nodeIndex >= rule.from && nodeIndex <= rule.to)
                    {
                        allowed = new HashSet<string>(rule.rarities);
                        break;
                    }
                }
            }

            var pool = new List<string>();
            foreach (var card in DataLoader.Cards.Values)
            {
                if (allowed.Contains(card.rarity)) pool.Add(card.id);
            }

            var offers = new List<string>();
            while (offers.Count < count && pool.Count > 0)
            {
                var idx = rng.Next(pool.Count);
                offers.Add(pool[idx]);
                pool.RemoveAt(idx);
            }
            return offers;
        }

        private static string RollRelic(System.Random rng)
        {
            var pool = new List<string>(DataLoader.Relics.Keys);
            if (pool.Count == 0) return null;
            return pool[rng.Next(pool.Count)];
        }
    }
}
