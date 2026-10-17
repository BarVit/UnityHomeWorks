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
            "id", "title", "kind", "location", "prefab", "prototype", "health", "weapon", "barrels",
            "body_damage", "scale", "tint", "movement", "reward_money", "reward_xp", "intro",
            "special_every", "special_weapon", "special_barrels"
        };

        public static BossTable Import(LocationTable locations, EnemyTable enemies, WeaponTable weapons, TableLog log)
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
                string location = reader.ReadOptionalText("location");

                CheckPrototype(enemies, ids, prototype, reader);
                CheckIntro(kind, intro, reader);
                CheckLocation(locations, location, reader);

                int specialEvery = reader.ReadInt("special_every", 0, 100);
                string specialWeapon = reader.ReadOptionalText("special_weapon");
                int specialBarrels = reader.ReadInt("special_barrels", 0, 8);

                CheckSpecial(weapons, specialEvery, specialWeapon, specialBarrels, reader);
                CheckWeapon(weapons, weapon, barrels, reader);

                rows.Add(new BossRow(
                    id,
                    reader.ReadText("title"),
                    kind,
                    location,
                    reader.ReadPrefab<EnemyShip>("prefab", true),
                    prototype,
                    reader.ReadInt("health", 1, 1000000000),
                    weapon,
                    barrels,
                    reader.ReadInt("body_damage", 0, 100000),
                    reader.ReadInt("scale", 1, 10),
                    reader.ReadColor("tint"),
                    reader.ReadAsset<MovementProfile>("movement", true),
                    reader.ReadInt("reward_money", 0, 1000000),
                    reader.ReadInt("reward_xp", 0, 1000000),
                    intro,
                    specialEvery,
                    specialWeapon,
                    specialBarrels,
                    phases.TryGetValue(id, out BossPhaseRow[] bossPhases) ? bossPhases : new BossPhaseRow[0]));
            }

            CheckPlacement(locations, rows, log);

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

        private static void CheckSpecial(WeaponTable weapons, int every, string weapon, int barrels, RowReader reader)
        {
            if (every == 0)
            {
                if (weapon.Length > 0 || barrels > 0)
                    reader.Fail("special_weapon, special_barrels: без special_every особая атака не сработает");

                return;
            }

            if (every < 2)
                reader.Fail("special_every: особая атака — каждый N-й выстрел, N от 2");

            if (weapon.Length == 0 || barrels < 1)
                reader.Fail("special_weapon, special_barrels: у особой атаки нужны оружие и стволы");
            else
                CheckWeapon(weapons, weapon, barrels, reader);
        }

        private static void CheckLocation(LocationTable locations, string location, RowReader reader)
        {
            if (locations == null || location.Length == 0)
                return;

            if (locations.GetRow(location) == null)
                reader.Fail("location: нет локации с id " + location + " в " + LocationTableImporter.SourcePath);
        }

        private static void CheckPlacement(LocationTable locations, List<BossRow> bosses, TableLog log)
        {
            if (locations == null)
                return;

            foreach (BossRow boss in bosses)
            {
                LocationRow location = locations.GetRow(boss.Location);

                if (location != null && location.Boss != boss.Id)
                    log.Add(SourcePath, 0, boss.Id + ": location " + boss.Location + ", а в "
                        + LocationTableImporter.SourcePath + " у этой локации boss «" + location.Boss + "»");
            }

            foreach (LocationRow location in locations.Rows)
            {
                if (string.IsNullOrEmpty(location.Boss))
                    continue;

                BossRow boss = bosses.Find(row => row.Id == location.Boss);

                if (boss == null)
                    log.Add(LocationTableImporter.SourcePath, 0,
                        location.Id + ": boss " + location.Boss + " — нет такого босса в " + SourcePath);
                else if (boss.Location != location.Id)
                    log.Add(LocationTableImporter.SourcePath, 0, location.Id + ": boss " + location.Boss + ", а в "
                        + SourcePath + " у него location «" + boss.Location + "»");
            }
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
