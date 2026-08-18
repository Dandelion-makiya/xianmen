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

            // TODO: 校验连续普通战斗不超过 MaxConsecutiveBattles，违规时重排。
            return nodes;
        }
    }
}
