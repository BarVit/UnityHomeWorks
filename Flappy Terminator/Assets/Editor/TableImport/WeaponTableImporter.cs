using System.Collections.Generic;
using UnityEditor;

namespace TableImport
{
    public static class WeaponTableImporter
    {
        public const string SourcePath = "Assets/Data/Tables/weapons.tsv";
        public const string AssetPath = "Assets/Data/Generated/Weapons.asset";

        private static readonly string[] Columns =
        {
            "id", "delivery", "damage", "rate", "projectile_speed", "barrels_max",
            "fire_pattern", "heat_per_shot", "cooldown_rate", "tick_interval", "turn_rate",
            "area", "price", "projectile", "hit_effect"
        };

        public static WeaponTable Import(TableLog log)
        {
            TsvDocument document = TsvReader.Read(SourcePath, log);

            if (document == null)
                return null;

            if (document.HasColumns(Columns, log) == false)
                return null;

            List<WeaponRow> rows = new List<WeaponRow>();
            HashSet<string> ids = new HashSet<string>();

            foreach (TsvRow raw in document.Rows)
            {
                RowReader reader = new RowReader(document, raw, log);
                string id = reader.ReadText("id");

                if (ids.Add(id) == false)
                    reader.Fail("повторяющийся id " + id);

                DeliveryKind delivery = reader.ReadEnum<DeliveryKind>("delivery");
                bool needsProjectile = delivery != DeliveryKind.Beam && delivery != DeliveryKind.ContinuousBeam;

                rows.Add(new WeaponRow(
                    id,
                    delivery,
                    reader.ReadInt("damage", 1, 10000),
                    reader.ReadFloat("rate", 0.01f, 60f),
                    reader.ReadFloat("projectile_speed", 0f, 100f),
                    reader.ReadInt("barrels_max", 1, 8),
                    reader.ReadEnum<FirePattern>("fire_pattern"),
                    reader.ReadFloat("heat_per_shot", 0f, 100f),
                    reader.ReadFloat("cooldown_rate", 0f, 1000f),
                    reader.ReadFloat("tick_interval", 0f, 10f),
                    reader.ReadFloat("turn_rate", 0f, 1000f),
                    reader.ReadFloat("area", 0f, 20f),
                    reader.ReadInt("price", 0, 1000000),
                    reader.ReadPrefab<Ammo>("projectile", needsProjectile),
                    reader.ReadPrefab<ExplosionAnimation>("hit_effect", needsProjectile == false)));
            }

            WeaponTable table = GeneratedAsset.LoadOrCreate<WeaponTable>(AssetPath);
            table.Fill(rows.ToArray());
            EditorUtility.SetDirty(table);

            return table;
        }
    }
}
