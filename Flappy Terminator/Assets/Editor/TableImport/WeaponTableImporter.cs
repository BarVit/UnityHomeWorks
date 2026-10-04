using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TableImport
{
    public static class WeaponTableImporter
    {
        public const string SourcePath = "Assets/Data/Tables/weapons.tsv";
        public const string AssetPath = "Assets/Data/Generated/Weapons.asset";

        private static readonly string[] Columns =
        {
            "id", "title", "owner", "delivery", "damage", "rate", "projectile_speed", "barrels_max",
            "fire_pattern", "side_angle", "heat_per_shot", "cooldown_rate", "tick_interval", "turn_rate",
            "area", "price", "projectile", "beam",
            "icon", "projectile_sprite", "projectile_animation", "shot_effect",
            "hit_target_effect", "hit_world_effect",
            "shot_sound", "flight_sound", "hit_target_sound", "hit_world_sound", "overheat_sound"
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
                    reader.ReadText("title"),
                    reader.ReadEnum<WeaponOwner>("owner"),
                    delivery,
                    reader.ReadInt("damage", 1, 10000),
                    reader.ReadFloat("rate", 0.01f, 60f),
                    reader.ReadFloat("projectile_speed", 0f, 100f),
                    reader.ReadInt("barrels_max", 1, 8),
                    reader.ReadEnum<FirePattern>("fire_pattern"),
                    reader.ReadFloat("side_angle", 0f, 90f),
                    reader.ReadFloat("heat_per_shot", 0f, 100f),
                    reader.ReadFloat("cooldown_rate", 0f, 1000f),
                    reader.ReadFloat("tick_interval", 0f, 10f),
                    reader.ReadFloat("turn_rate", 0f, 1000f),
                    reader.ReadFloat("area", 0f, 20f),
                    reader.ReadInt("price", 0, 1000000),
                    reader.ReadPrefab<Ammo>("projectile", needsProjectile),
                    reader.ReadPrefab<BeamFlash>("beam", needsProjectile == false),
                    new WeaponAssets(
                        reader.ReadAsset<Sprite>("icon", false),
                        reader.ReadAsset<Sprite>("projectile_sprite", false),
                        reader.ReadAsset<AnimationClip>("projectile_animation", false),
                        reader.ReadPrefab<ExplosionAnimation>("shot_effect", false),
                        reader.ReadPrefab<ExplosionAnimation>("hit_target_effect", false),
                        reader.ReadPrefab<ExplosionAnimation>("hit_world_effect", false),
                        reader.ReadAsset<AudioClip>("shot_sound", false),
                        reader.ReadAsset<AudioClip>("flight_sound", false),
                        reader.ReadAsset<AudioClip>("hit_target_sound", false),
                        reader.ReadAsset<AudioClip>("hit_world_sound", false),
                        reader.ReadAsset<AudioClip>("overheat_sound", false))));
            }

            WeaponTable table = GeneratedAsset.LoadOrCreate<WeaponTable>(AssetPath);
            table.Fill(rows.ToArray());
            EditorUtility.SetDirty(table);

            return table;
        }
    }
}
