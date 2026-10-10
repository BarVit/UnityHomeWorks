using TMPro;
using UnityEditor;
using UnityEngine;

namespace WeaponContent
{
    public static class CritMarkBuilder
    {
        private const string PrefabPath = "Assets/Prefabs/Crit Mark.prefab";
        private const string BurstPath = "Assets/Sprites/VFX/Crit Burst.png";
        private const string FontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        private const string OutlinePath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Outline.mat";
        private const string PlayerPath = "Assets/Prefabs/Player Ship.prefab";
        private const float BurstPixelsPerUnit = 256f;
        private const float BurstScale = 1.4f;
        private const float FontSize = 6.4f;
        private const int BurstSortingOrder = 20;
        private const int LabelSortingOrder = 21;
        private static readonly Vector3 LabelOffset = new Vector3(0f, 0.3f, 0f);
        private static readonly Color LabelColor = new Color(0.82f, 0.94f, 1f);

        [MenuItem("Tools/Weapons/Build Crit Mark")]
        public static void Build()
        {
            PrepareBurst();

            GameObject root = new GameObject("Crit Mark");
            CritMark mark = root.AddComponent<CritMark>();

            GameObject burst = new GameObject("Burst");
            burst.transform.SetParent(root.transform, false);
            burst.transform.localScale = Vector3.one * BurstScale;
            SpriteRenderer burstRenderer = burst.AddComponent<SpriteRenderer>();
            burstRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BurstPath);
            burstRenderer.sortingOrder = BurstSortingOrder;

            GameObject label = new GameObject("Label");
            label.transform.SetParent(root.transform, false);
            label.transform.localPosition = LabelOffset;
            TextMeshPro text = label.AddComponent<TextMeshPro>();
            text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            text.fontSharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(OutlinePath);
            text.fontSize = FontSize;
            text.fontStyle = FontStyles.Bold;
            text.color = LabelColor;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.rectTransform.sizeDelta = new Vector2(4f, 1.5f);
            text.sortingOrder = LabelSortingOrder;
            text.text = "24";

            SerializedObject so = new SerializedObject(mark);
            so.FindProperty("_burst").objectReferenceValue = burstRenderer;
            so.FindProperty("_label").objectReferenceValue = text;
            so.ApplyModifiedPropertiesWithoutUndo();

            CritMark prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath).GetComponent<CritMark>();
            Object.DestroyImmediate(root);

            AttachToPlayer(prefab);
            Debug.Log("Крит: метка собрана в " + PrefabPath);
        }

        private static void PrepareBurst()
        {
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(BurstPath);

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = BurstPixelsPerUnit;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static void AttachToPlayer(CritMark prefab)
        {
            GameObject player = PrefabUtility.LoadPrefabContents(PlayerPath);

            try
            {
                SerializedObject so = new SerializedObject(player.GetComponent<PlayerShooter>());
                so.FindProperty("_critMark").objectReferenceValue = prefab;
                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(player, PlayerPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(player);
            }
        }
    }
}
