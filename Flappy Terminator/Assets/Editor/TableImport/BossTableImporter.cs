using System.Collections.Generic;
using UnityEditor;

namespace TableImport
{
    public static class BossTableImporter
    {
        public const string SourcePath = "Assets/Data/Tables/bosses.tsv";
        public const string AssetPath = "Assets/Data/Generated/Bosses.asset";

        private const string LineBreakMark = "\\n";

        private static readonly string[] Columns =
        {
            "id", "title", "kind", "prefab", "prototype", "health", "weapon", "barrels", "damage",
            "body_damage", "scale", "tint", "movement", "reward_money", "reward_xp", "intro"
        };

        public static BossTable Import(EnemyTable enemies, WeaponTable weapons, TableLog log)
        {
            TsvDocument document = TsvReader.Read(SourcePath, log);

            if (document == null)
                return null;

            if (document.HasColumns(Columns, log) == false)
                return null;

            List<BossRow> rows = new List<BossRow>();
            HashSet<string> ids = new HashSet<string>();

            foreach (TsvRow raw in document.Rows)
                ids.Add(new RowReader(document, raw, new TableLog()).ReadOptionalText("id"));

            Dictionary<string, BossPhaseRow[]> phases = BossPhaseTableImporter.Read(weapons, log);
            BossPhaseTableImporter.CheckBosses(phases, ids, log);

            HashSet<string> seen = new HashSet<string>();

            foreach (TsvRow raw in document.Rows)
            {
                RowReader reader = new RowReader(document, raw, log);
                string id = reader.ReadText("id");

                if (seen.Add(id) == false)
                    reader.Fail("повторяющийся id " + id);

                string prototype = reader.ReadText("prototype");
                string weapon = reader.ReadText("weapon");
                int barrels = reader.ReadInt("barrels", 1, 8);

                BossKind kind = reader.ReadEnum<BossKind>("kind");
                string intro = reader.ReadOptionalText("intro").Replace(LineBreakMark, "\n");

                CheckPrototype(enemies, ids, prototype, reader);
                CheckIntro(kind, intro, reader);
                CheckWeapon(weapons, weapon, barrels, reader);

                rows.Add(new BossRow(
                    id,
                    reader.ReadText("title"),
                    kind,
                    reader.ReadPrefab<EnemyShip>("prefab", true),
                    prototype,
                    reader.ReadInt("health", 1, 1000000),
                    weapon,
                    barrels,
                    reader.ReadInt("damage", 1, 100000),
                    reader.ReadInt("body_damage", 0, 100000),
                    reader.ReadInt("scale", 1, 10),
                    reader.ReadColor("tint"),
                    reader.ReadAsset<MovementProfile>("movement", true),
                    reader.ReadInt("reward_money", 0, 1000000),
                    reader.ReadInt("reward_xp", 0, 1000000),
                    intro,
                    phases.TryGetValue(id, out BossPhaseRow[] bossPhases) ? bossPhases : new BossPhaseRow[0]));
            }

            BossTable table = GeneratedAsset.LoadOrCreate<BossTable>(AssetPath);
            table.Fill(rows.ToArray());
            EditorUtility.SetDirty(table);

            return table;
        }

        public static void CheckWeapon(WeaponTable weapons, string weapon, int barrels, RowReader reader)
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

        private static void CheckIntro(BossKind kind, string intro, RowReader reader)
        {
            if (kind == BossKind.Boss && intro.Length == 0)
                reader.Fail("intro: боссу нужна надпись перед появлением");
            else if (kind == BossKind.MiniBoss && intro.Length > 0)
                reader.Fail("intro: у мини-босса надписи перед появлением нет, оставьте пустым");
        }

        private static void CheckPrototype(EnemyTable enemies, HashSet<string> ids, string prototype,
            RowReader reader)
        {
            if (enemies == null || prototype.Length == 0)
                return;

            if (enemies.GetRow(prototype) == null && ids.Contains(prototype) == false)
                reader.Fail("prototype: " + prototype + " — нет ни врага в "
                    + EnemyTableImporter.SourcePath + ", ни строки в этой таблице");
        }
    }
}
