using System.Collections.Generic;
using UnityEditor;

namespace TableImport
{
    public static class EnemyTierTableImporter
    {
        public const string SourcePath = "Assets/Data/Tables/enemy_tiers.tsv";
        public const string AssetPath = "Assets/Data/Generated/EnemyTiers.asset";

        private static readonly string[] Columns =
        {
            "id", "ship", "tier", "health", "weapon", "barrels", "damage", "movement"
        };

        public static EnemyTierTable Import(EnemyShipTable ships, WeaponTable weapons, TableLog log)
        {
            TsvDocument document = TsvReader.Read(SourcePath, log);

            if (document == null)
                return null;

            if (document.HasColumns(Columns, log) == false)
                return null;

            List<EnemyTierRow> rows = new List<EnemyTierRow>();
            HashSet<string> ids = new HashSet<string>();
            HashSet<string> steps = new HashSet<string>();

            foreach (TsvRow raw in document.Rows)
            {
                RowReader reader = new RowReader(document, raw, log);
                string id = reader.ReadText("id");

                if (ids.Add(id) == false)
                    reader.Fail("повторяющийся id " + id);

                string ship = reader.ReadText("ship");
                int tier = reader.ReadInt("tier", 1, 5);
                string weapon = reader.ReadText("weapon");
                int barrels = reader.ReadInt("barrels", 1, 8);

                if (steps.Add(ship + " " + tier) == false)
                    reader.Fail("у корабля " + ship + " уже есть ступень " + tier);

                CheckShip(ships, ship, reader);
                CheckWeapon(weapons, weapon, barrels, reader);

                rows.Add(new EnemyTierRow(
                    id,
                    ship,
                    tier,
                    reader.ReadInt("health", 1, 100000),
                    weapon,
                    barrels,
                    reader.ReadInt("damage", 1, 10000),
                    reader.ReadAsset<MovementProfile>("movement", true)));
            }

            EnemyTierTable table = GeneratedAsset.LoadOrCreate<EnemyTierTable>(AssetPath);
            table.Fill(rows.ToArray());
            EditorUtility.SetDirty(table);

            return table;
        }

        private static void CheckShip(EnemyShipTable ships, string ship, RowReader reader)
        {
            if (ships == null || ship.Length == 0)
                return;

            if (ships.GetRow(ship) == null)
                reader.Fail("ship: нет корабля с id " + ship + " в " + EnemyShipTableImporter.SourcePath);
        }

        private static void CheckWeapon(WeaponTable weapons, string weapon, int barrels, RowReader reader)
        {
            if (weapons == null || weapon.Length == 0)
                return;

            WeaponRow row = weapons.GetRow(weapon);

            if (row == null)
            {
                reader.Fail("weapon: нет оружия с id " + weapon + " в " + WeaponTableImporter.SourcePath);

                return;
            }

            if (row.Owner != WeaponOwner.Enemy)
                reader.Fail("weapon: " + weapon + " принадлежит игроку, врагу нужна строка с owner Enemy");

            if (barrels > row.BarrelsMax)
                reader.Fail("barrels: " + barrels + " больше, чем barrels_max оружия "
                    + row.Id + " (" + row.BarrelsMax + ")");
        }
    }
}
