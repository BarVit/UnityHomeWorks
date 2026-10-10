using System.Collections.Generic;
using UnityEngine;

namespace TableImport
{
    public static class BossPhaseTableImporter
    {
        public const string SourcePath = "Assets/Data/Tables/boss_phases.tsv";

        private const int MaxPhases = 20;

        private static readonly string[] Columns =
        {
            "boss", "phase", "weapon", "barrels", "damage", "rate_scale", "exit", "exit_value", "next"
        };

        public static Dictionary<string, BossPhaseRow[]> Read(WeaponTable weapons, TableLog log)
        {
            Dictionary<string, BossPhaseRow[]> phases = new Dictionary<string, BossPhaseRow[]>();
            TsvDocument document = TsvReader.Read(SourcePath, log);

            if (document == null || document.HasColumns(Columns, log) == false)
                return phases;

            Dictionary<string, List<BossPhaseRow>> byBoss = new Dictionary<string, List<BossPhaseRow>>();
            Dictionary<BossPhaseRow, RowReader> readers = new Dictionary<BossPhaseRow, RowReader>();

            foreach (TsvRow raw in document.Rows)
            {
                RowReader reader = new RowReader(document, raw, log);
                string boss = reader.ReadText("boss");
                string weapon = reader.ReadText("weapon");
                int barrels = reader.ReadInt("barrels", 1, 8);
                PhaseExitKind exit = reader.ReadEnum<PhaseExitKind>("exit");
                float exitValue = reader.ReadFloat("exit_value", 0f, 100000f);

                BossTableImporter.CheckWeapon(weapons, weapon, barrels, reader);
                CheckExit(exit, exitValue, reader);

                BossPhaseRow row = new BossPhaseRow(
                    reader.ReadInt("phase", 1, MaxPhases),
                    weapon,
                    barrels,
                    reader.ReadInt("damage", 1, 100000),
                    reader.ReadFloat("rate_scale", 0.05f, 10f),
                    exit,
                    exitValue,
                    reader.ReadInt("next", BossPhaseRow.FollowingPhase, MaxPhases));

                if (byBoss.TryGetValue(boss, out List<BossPhaseRow> list) == false)
                {
                    list = new List<BossPhaseRow>();
                    byBoss.Add(boss, list);
                }

                list.Add(row);
                readers.Add(row, reader);
            }

            foreach (KeyValuePair<string, List<BossPhaseRow>> pair in byBoss)
            {
                pair.Value.Sort((left, right) => left.Phase.CompareTo(right.Phase));
                CheckOrder(pair.Value, readers);
                phases.Add(pair.Key, pair.Value.ToArray());
            }

            return phases;
        }

        public static void CheckBosses(Dictionary<string, BossPhaseRow[]> phases, HashSet<string> bosses, TableLog log)
        {
            foreach (string boss in phases.Keys)
            {
                if (bosses.Contains(boss) == false)
                    log.Add(SourcePath, 0, "boss: нет босса с id " + boss + " в " + BossTableImporter.SourcePath);
            }
        }

        private static void CheckExit(PhaseExitKind exit, float value, RowReader reader)
        {
            switch (exit)
            {
                case PhaseExitKind.Health:
                    if (value <= 0f || value >= 1f)
                        reader.Fail("exit_value: для Health нужна доля прочности между 0 и 1, например 0.67");

                    break;
                case PhaseExitKind.Time:
                    if (value <= 0f)
                        reader.Fail("exit_value: для Time нужно время фазы в секундах больше 0");

                    break;
                case PhaseExitKind.Shots:
                    if (value < 1f || Mathf.Approximately(value, Mathf.Round(value)) == false)
                        reader.Fail("exit_value: для Shots нужно целое число залпов от 1");

                    break;
            }
        }

        private static void CheckOrder(List<BossPhaseRow> rows, Dictionary<BossPhaseRow, RowReader> readers)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                RowReader reader = readers[rows[i]];

                if (rows[i].Phase != i + 1)
                    reader.Fail("phase: фазы босса нумеруются подряд с 1, а здесь " + rows[i].Phase);

                if (rows[i].Next > rows.Count)
                    reader.Fail("next: у босса нет фазы " + rows[i].Next);

                if (rows[i].Exit == PhaseExitKind.None && rows[i].Next != BossPhaseRow.FollowingPhase)
                    reader.Fail("next: у фазы без выхода (exit None) переход не сработает");
            }
        }
    }
}
