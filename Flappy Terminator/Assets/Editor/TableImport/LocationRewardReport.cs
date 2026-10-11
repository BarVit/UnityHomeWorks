using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TableImport
{
    public static class LocationRewardReport
    {
        public const string ReportPath = "Assets/Data/Reports/location_rewards.tsv";

        private const string Separator = "\t";
        private const string LineEnd = "\r\n";

        private static readonly string[] Columns =
        {
            "location", "label", "levels", "enemies", "money", "xp", "money_min", "money_max", "xp_min", "xp_max",
            "route_money", "route_xp", "note"
        };

        public static void Write(SystemTable systems, LocationTable locations, WaveTable waves, EnemyTable enemies,
            BossTable bosses)
        {
            StringBuilder text = new StringBuilder(string.Join(Separator, Columns)).Append(LineEnd);
            List<SystemRow> systemRoute = new List<SystemRow>(systems.Rows);
            float routeMoney = 0f;
            float routeXp = 0f;

            systemRoute.Sort((left, right) => left.Order.CompareTo(right.Order));

            foreach (SystemRow system in systemRoute)
            {
                foreach (LocationRow location in GetRoute(locations, system.Id))
                {
                    if (waves.HasLocation(location.Id))
                    {
                        LocationReward reward = Count(location, waves, enemies, bosses);

                        routeMoney += reward.Money;
                        routeXp += reward.Xp;
                        Append(text, location, system.Order + "-" + location.Order, reward, Format(routeMoney),
                            Format(routeXp));
                    }

                    foreach (LocationRow branch in locations.GetBranches(location.Id))
                    {
                        if (waves.HasLocation(branch.Id))
                            Append(text, branch, branch.Title, Count(branch, waves, enemies, bosses), "", "");
                    }
                }
            }

            Save(text.ToString());
        }

        private static List<LocationRow> GetRoute(LocationTable locations, string systemId)
        {
            List<LocationRow> route = new List<LocationRow>();

            foreach (LocationRow location in locations.Rows)
            {
                if (location.System == systemId && location.IsBranch == false)
                    route.Add(location);
            }

            route.Sort((left, right) => left.Order.CompareTo(right.Order));

            return route;
        }

        private static LocationReward Count(LocationRow location, WaveTable waves, EnemyTable enemies, BossTable bosses)
        {
            LocationReward reward = new LocationReward();

            for (int level = 1; level <= location.Levels; level++)
            {
                List<WaveRow> rows = waves.GetLevel(location.Id, level);

                if (rows.Count == 0)
                    reward.RandomLevels.Add(level);

                foreach (WaveRow row in rows)
                    CountGroup(reward, row, enemies, bosses);
            }

            return reward;
        }

        private static void CountGroup(LocationReward reward, WaveRow row, EnemyTable enemies, BossTable bosses)
        {
            if (row.Mode == WaveMode.Boss)
            {
                BossRow boss = bosses.GetRow(row.Enemies[0]);

                reward.Add(boss.RewardMoney, boss.RewardMoney, boss.RewardMoney, boss.RewardXp, boss.RewardXp,
                    boss.RewardXp);

                return;
            }

            if (row.Mode == WaveMode.Sequence)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    EnemyRow enemy = enemies.GetRow(row.Enemies[i % row.Enemies.Length]);

                    reward.Add(enemy.RewardMoney, enemy.RewardMoney, enemy.RewardMoney, enemy.RewardXp, enemy.RewardXp,
                        enemy.RewardXp);
                }

                return;
            }

            float money = 0f;
            float xp = 0f;
            int moneyMin = int.MaxValue;
            int moneyMax = 0;
            int xpMin = int.MaxValue;
            int xpMax = 0;

            foreach (string id in row.Enemies)
            {
                EnemyRow enemy = enemies.GetRow(id);

                money += enemy.RewardMoney;
                xp += enemy.RewardXp;
                moneyMin = Mathf.Min(moneyMin, enemy.RewardMoney);
                moneyMax = Mathf.Max(moneyMax, enemy.RewardMoney);
                xpMin = Mathf.Min(xpMin, enemy.RewardXp);
                xpMax = Mathf.Max(xpMax, enemy.RewardXp);
            }

            for (int i = 0; i < row.Count; i++)
                reward.Add(money / row.Enemies.Length, moneyMin, moneyMax, xp / row.Enemies.Length, xpMin, xpMax);
        }

        private static void Append(StringBuilder text, LocationRow location, string label, LocationReward reward,
            string routeMoney, string routeXp)
        {
            string[] cells =
            {
                location.Id, label, location.Levels.ToString(CultureInfo.InvariantCulture),
                reward.Enemies.ToString(CultureInfo.InvariantCulture), Format(reward.Money), Format(reward.Xp),
                reward.MoneyMin.ToString(CultureInfo.InvariantCulture), reward.MoneyMax.ToString(CultureInfo.InvariantCulture),
                reward.XpMin.ToString(CultureInfo.InvariantCulture), reward.XpMax.ToString(CultureInfo.InvariantCulture),
                routeMoney, routeXp, GetNote(reward)
            };

            text.Append(string.Join(Separator, cells)).Append(LineEnd);
        }

        private static string GetNote(LocationReward reward)
        {
            if (reward.RandomLevels.Count == 0)
                return "";

            return "уровни " + string.Join(", ", reward.RandomLevels) + " без строк в waves.tsv — случайные враги, не посчитаны";
        }

        private static string Format(float value)
        {
            return Mathf.RoundToInt(value).ToString(CultureInfo.InvariantCulture);
        }

        private static void Save(string text)
        {
            string folder = Path.GetDirectoryName(ReportPath);

            if (Directory.Exists(folder) == false)
                Directory.CreateDirectory(folder);

            if (File.Exists(ReportPath) && File.ReadAllText(ReportPath, Encoding.Unicode) == text)
                return;

            File.WriteAllText(ReportPath, text, new UnicodeEncoding(false, true));
            AssetDatabase.ImportAsset(ReportPath);
        }

        private class LocationReward
        {
            public readonly List<int> RandomLevels = new List<int>();

            public int Enemies { get; private set; }

            public float Money { get; private set; }

            public float Xp { get; private set; }

            public int MoneyMin { get; private set; }

            public int MoneyMax { get; private set; }

            public int XpMin { get; private set; }

            public int XpMax { get; private set; }

            public void Add(float money, int moneyMin, int moneyMax, float xp, int xpMin, int xpMax)
            {
                Enemies++;
                Money += money;
                MoneyMin += moneyMin;
                MoneyMax += moneyMax;
                Xp += xp;
                XpMin += xpMin;
                XpMax += xpMax;
            }
        }
    }
}
