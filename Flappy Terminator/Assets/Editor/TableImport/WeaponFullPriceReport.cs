using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TableImport
{
    public static class WeaponFullPriceReport
    {
        public const string ReportPath = "Assets/Data/Reports/weapons_full_price.tsv";

        private const int MaxBarrels = 4;
        private const int ComfortLevel = 4;
        private const int RouteStep = 10;
        private const int SystemStep = 1000;
        private const int BranchShift = 5;

        public static void Write(SystemTable systems, LocationTable locations, WeaponTable weapons, SlotTable slots,
            UpgradeTable upgrades)
        {
            List<WeaponRow> rows = GetPlayerWeapons(weapons);
            List<UpgradeStat> stats = GetUpgradedStats(rows, upgrades);
            StringBuilder text = new StringBuilder();

            rows.Sort((left, right) =>
                GetRouteKey(systems, locations, left).CompareTo(GetRouteKey(systems, locations, right)));
            AppendLine(text, GetColumns(stats));

            foreach (WeaponRow row in rows)
                AppendLine(text, GetCells(row, stats, slots, upgrades));

            ReportFile.Save(ReportPath, text.ToString());
        }

        private static List<WeaponRow> GetPlayerWeapons(WeaponTable weapons)
        {
            List<WeaponRow> rows = new List<WeaponRow>();

            foreach (WeaponRow row in weapons.Rows)
            {
                if (row.Owner == WeaponOwner.Player)
                    rows.Add(row);
            }

            return rows;
        }

        private static List<UpgradeStat> GetUpgradedStats(List<WeaponRow> rows, UpgradeTable upgrades)
        {
            List<UpgradeStat> stats = new List<UpgradeStat>();

            foreach (UpgradeStat stat in (UpgradeStat[])Enum.GetValues(typeof(UpgradeStat)))
            {
                if (rows.Exists(row => upgrades.GetMaxLevel(row.Id, stat) > 0))
                    stats.Add(stat);
            }

            return stats;
        }

        private static List<string> GetColumns(List<UpgradeStat> stats)
        {
            List<string> columns = new List<string> { "weapon", "title", "unlock", "barrels", "weapon_price" };

            for (int slot = Hangar.FirstSlot + 1; slot <= MaxBarrels; slot++)
                columns.Add("slot_" + slot);

            foreach (UpgradeStat stat in stats)
                columns.Add(ToSnakeCase(stat.ToString()));

            columns.AddRange(new[] { "per_barrel", "all_barrels", "all_barrels_lvl" + ComfortLevel, "total", "note" });

            return columns;
        }

        private static List<string> GetCells(WeaponRow row, List<UpgradeStat> stats, SlotTable slots,
            UpgradeTable upgrades)
        {
            int barrels = Math.Min(MaxBarrels, row.BarrelsMax);
            int total = row.Price;
            int perBarrel = 0;
            int perBarrelToComfort = 0;
            List<string> cells = new List<string>
            {
                row.Id, row.Title, GetUnlock(row), Format(barrels), Format(row.Price)
            };

            for (int slot = Hangar.FirstSlot + 1; slot <= MaxBarrels; slot++)
            {
                if (slot > barrels)
                {
                    cells.Add("");
                    continue;
                }

                int price = slots.GetRow(row.Id, slot).Price;

                total += price;
                cells.Add(Format(price));
            }

            foreach (UpgradeStat stat in stats)
            {
                int price = GetPrice(upgrades, row.Id, stat, upgrades.GetMaxLevel(row.Id, stat));

                perBarrel += price;
                perBarrelToComfort += GetPrice(upgrades, row.Id, stat, ComfortLevel);
                cells.Add(price > 0 ? Format(price) : "");
            }

            total += perBarrel * barrels;
            cells.AddRange(new[]
            {
                Format(perBarrel), Format(perBarrel * barrels), Format(perBarrelToComfort * barrels), Format(total),
                GetNote(row, barrels, upgrades)
            });

            return cells;
        }

        private static int GetPrice(UpgradeTable upgrades, string weaponId, UpgradeStat stat, int toLevel)
        {
            int price = 0;
            int lastLevel = Math.Min(toLevel, upgrades.GetMaxLevel(weaponId, stat));

            for (int level = 1; level <= lastLevel; level++)
                price += upgrades.GetRow(weaponId, stat, level).Price;

            return price;
        }

        private static string GetUnlock(WeaponRow row)
        {
            if (string.IsNullOrEmpty(row.GrantedAfter) == false)
                return row.GrantedAfter;

            return string.IsNullOrEmpty(row.UnlockAfter) ? "start" : row.UnlockAfter;
        }

        private static string GetNote(WeaponRow row, int barrels, UpgradeTable upgrades)
        {
            List<string> notes = new List<string>();

            if (barrels < MaxBarrels)
                notes.Add("максимум стволов: " + barrels);

            if (string.IsNullOrEmpty(row.GrantedAfter) == false)
                notes.Add("выдаётся бесплатно");

            if (HasOwnUpgrades(row.Id, upgrades) == false)
                notes.Add("своих апгрейдов нет — посчитаны общие");

            return string.Join(", ", notes);
        }

        private static bool HasOwnUpgrades(string weaponId, UpgradeTable upgrades)
        {
            foreach (LevelRow<UpgradeStat> row in upgrades.Rows)
            {
                if (row.Owner == weaponId && row.Level > 0)
                    return true;
            }

            return false;
        }

        private static int GetRouteKey(SystemTable systems, LocationTable locations, WeaponRow row)
        {
            string unlock = GetUnlock(row);

            if (unlock == "start")
                return 0;

            List<LocationRow> voidLocations = locations.GetVoid(unlock);
            LocationRow location = voidLocations.Count > 0 ? voidLocations[0] : locations.GetRow(unlock);

            if (location == null)
                return int.MaxValue;

            int shift = 0;

            if (location.IsBranch)
            {
                location = locations.GetRow(location.BranchOf);
                shift = BranchShift;
            }

            SystemRow system = systems.GetRow(location.System);

            return system.Order * SystemStep + location.Order * RouteStep + shift;
        }

        private static string ToSnakeCase(string name)
        {
            StringBuilder text = new StringBuilder();

            for (int i = 0; i < name.Length; i++)
            {
                if (i > 0 && char.IsUpper(name[i]))
                    text.Append('_');

                text.Append(char.ToLowerInvariant(name[i]));
            }

            return text.ToString();
        }

        private static string Format(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static void AppendLine(StringBuilder text, List<string> cells)
        {
            text.Append(string.Join(ReportFile.Separator, cells)).Append(ReportFile.LineEnd);
        }
    }
}
