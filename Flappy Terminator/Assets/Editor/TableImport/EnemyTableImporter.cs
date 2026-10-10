using System.Collections.Generic;
using UnityEditor;

namespace TableImport
{
    public static class EnemyTableImporter
    {
        public const string SourcePath = "Assets/Data/Tables/enemies.tsv";
        public const string AssetPath = "Assets/Data/Generated/Enemies.asset";

        private static readonly string[] Columns =
        {
            "id", "type", "title", "tier", "prefab", "body_damage", "from_location", "health", "weapon",
            "barrels", "damage", "movement", "skin", "reward_money"
        };

        public static EnemyTable Import(LocationTable locations, WeaponTable weapons, TableLog log)
        {
            TsvDocument document = TsvReader.Read(SourcePath, log);

            if (document == null)
                return null;

            if (document.HasColumns(Columns, log) == false)
                return null;

            List<EnemyRow> rows = new List<EnemyRow>();
            HashSet<string> ids = new HashSet<string>();
            HashSet<string> steps = new HashSet<string>();
            Dictionary<string, EnemyShip> prefabs = new Dictionary<string, EnemyShip>();

            foreach (TsvRow raw in document.Rows)
            {
                RowReader reader = new RowReader(document, raw, log);
                string id = reader.ReadText("id");

                if (ids.Add(id) == false)
                    reader.Fail("повторяющийся id " + id);

                string type = reader.ReadText("type");
                int tier = reader.ReadInt("tier", 1, 5);
                EnemyShip prefab = reader.ReadPrefab<EnemyShip>("prefab", true);
                string fromLocation = reader.ReadText("from_location");
                string weapon = reader.ReadText("weapon");
                int barrels = reader.ReadInt("barrels", 1, 8);

                if (steps.Add(type + " " + tier) == false)
                    reader.Fail("у типа " + type + " уже есть ступень " + tier);

                CheckPrefab(prefabs, type, prefab, reader);
                CheckLocation(locations, fromLocation, reader);
                CheckWeapon(weapons, weapon, barrels, reader);

                rows.Add(new EnemyRow(
                    id,
                    type,
                    reader.ReadText("title"),
                    tier,
                    prefab,
                    reader.ReadInt("body_damage", 0, 1000),
                    fromLocation,
                    reader.ReadInt("health", 1, 100000),
                    weapon,
                    barrels,
                    reader.ReadInt("damage", 1, 10000),
                    reader.ReadAsset<MovementProfile>("movement", true),
                    reader.ReadAsset<SpriteSkin>("skin", false),
                    reader.ReadInt("reward_money", 0, 1000000)));
            }

            EnemyTable table = GeneratedAsset.LoadOrCreate<EnemyTable>(AssetPath);
            table.Fill(rows.ToArray());
            EditorUtility.SetDirty(table);

            return table;
        }

        private static void CheckPrefab(Dictionary<string, EnemyShip> prefabs, string type, EnemyShip prefab,
            RowReader reader)
        {
            if (prefab == null || type.Length == 0)
                return;

            if (prefabs.TryGetValue(type, out EnemyShip first) == false)
                prefabs.Add(type, prefab);
            else if (first != prefab)
                reader.Fail("prefab: у ступеней типа " + type + " разные префабы — " + first.name + " и "
                    + prefab.name);
        }

        private static void CheckLocation(LocationTable locations, string location, RowReader reader)
        {
            if (locations == null || location.Length == 0)
                return;

            if (locations.GetRow(location) == null)
                reader.Fail("from_location: нет локации с id " + location
                    + " в " + LocationTableImporter.SourcePath);
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
