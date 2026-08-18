using System;
using System.Collections.Generic;

namespace Xianmen
{
    public static class MapGenerator
    {
        public const int TotalNodes = 20;
        private const int RestNodeIndex = 9;
        private const int BossNodeIndex = 19;
        private const int EventCount = 3;
        private const int MaxConsecutiveBattles = 3;

        private static readonly int[][] EliteWindows =
        {
            new[] { 6, 8 },
            new[] { 11, 13 },
            new[] { 16, 18 }
        };

        public static List<MapNode> Generate(int seed = -1)
        {
            var rng = seed >= 0 ? new Random(seed) : new Random();
            for (var attempt = 0; attempt < 50; attempt++)
            {
                var nodes = BuildLayout(rng);
                if (IsValidLayout(nodes)) return nodes;
            }
            return FixLayout(BuildLayout(rng), rng);
        }

        private static List<MapNode> BuildLayout(Random rng)
        {
            var nodes = new List<MapNode>();
            for (var i = 1; i <= TotalNodes; i++)
            {
                nodes.Add(new MapNode { index = i, type = "battle" });
            }

            nodes[RestNodeIndex].type = "rest";
            nodes[BossNodeIndex].type = "boss";

            foreach (var window in EliteWindows)
            {
                var index = rng.Next(window[0], window[1] + 1) - 1;
                while (nodes[index].type != "battle")
                {
                    index = rng.Next(window[0], window[1] + 1) - 1;
                }
                nodes[index].type = "elite";
            }

            var battleIndices = new List<int>();
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].type == "battle")
                {
                    battleIndices.Add(i);
                }
            }

            for (var i = battleIndices.Count - 1; i > 0; i--)
            {
                var j = rng.Next(i + 1);
                var tmp = battleIndices[i];
                battleIndices[i] = battleIndices[j];
                battleIndices[j] = tmp;
            }

            for (var k = 0; k < Math.Min(EventCount, battleIndices.Count); k++)
            {
                nodes[battleIndices[k]].type = "event";
            }

            return nodes;
        }

        private static bool IsValidLayout(List<MapNode> nodes)
        {
            var run = 0;
            foreach (var node in nodes)
            {
                if (node.type == "battle")
                {
                    run++;
                    if (run > MaxConsecutiveBattles) return false;
                }
                else
                {
                    run = 0;
                }
            }
            return true;
        }

        private static List<MapNode> FixLayout(List<MapNode> nodes, Random rng)
        {
            var battleSlots = new List<int>();
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].type == "battle") battleSlots.Add(i);
            }

            var validCombos = new List<int[]>();
            for (var a = 0; a < battleSlots.Count; a++)
            {
                for (var b = a + 1; b < battleSlots.Count; b++)
                {
                    for (var c = b + 1; c < battleSlots.Count; c++)
                    {
                        var candidate = new List<MapNode>();
                        foreach (var node in nodes)
                        {
                            candidate.Add(new MapNode { index = node.index, type = node.type });
                        }
                        candidate[battleSlots[a]].type = "event";
                        candidate[battleSlots[b]].type = "event";
                        candidate[battleSlots[c]].type = "event";
                        if (IsValidLayout(candidate))
                        {
                            validCombos.Add(new[] { battleSlots[a], battleSlots[b], battleSlots[c] });
                        }
                    }
                }
            }

            if (validCombos.Count == 0) return nodes;

            var chosen = validCombos[rng.Next(validCombos.Count)];
            foreach (var slot in battleSlots)
            {
                nodes[slot].type = "battle";
            }
            nodes[chosen[0]].type = "event";
            nodes[chosen[1]].type = "event";
            nodes[chosen[2]].type = "event";
            return nodes;
        }
    }
}
