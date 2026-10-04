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
            "id", "ship", "tier", "health_scale", "projectile_speed_scale", "barrels", "movement"
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
                int barrels = reader.ReadInt("barrels", 1, 8);

                if (steps.Add(ship + " " + tier) == false)
                    reader.Fail("у корабля " + ship + " уже есть ступень " + tier);

                CheckShip(ships, weapons, ship, barrels, reader);

                rows.Add(new EnemyTierRow(
                    id,
                    ship,
                    tier,
                    reader.ReadFloat("health_scale", 0.1f, 100f),
                    reader.ReadFloat("projectile_speed_scale", 0.1f, 10f),
                    barrels,
                    reader.ReadAsset<MovementProfile>("movement", true)));
            }

            EnemyTierTable table = GeneratedAsset.LoadOrCreate<EnemyTierTable>(AssetPath);
            table.Fill(rows.ToArray());
            EditorUtility.SetDirty(table);

            return table;
        }

        private static void CheckShip(EnemyShipTable ships, WeaponTable weapons, string ship,
            int barrels, RowReader reader)
        {
            if (ships == null || ship.Length == 0)
                return;

            EnemyShipRow shipRow = ships.GetRow(ship);

            if (shipRow == null)
            {
                reader.Fail("ship: нет корабля с id " + ship + " в " + EnemyShipTableImporter.SourcePath);

                return;
            }

            if (weapons == null)
                return;

            WeaponRow weapon = weapons.GetRow(shipRow.Weapon);

            if (weapon != null && barrels > weapon.BarrelsMax)
                reader.Fail("barrels: " + barrels + " больше, чем barrels_max оружия "
                    + weapon.Id + " (" + weapon.BarrelsMax + ")");
        }
    }
}
