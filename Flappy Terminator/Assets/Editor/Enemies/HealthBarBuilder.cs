using UnityEditor;
using UnityEngine;

namespace EnemyContent
{
    public static class HealthBarBuilder
    {
        private const string PixelPath = "Assets/Sprites/VFX/Bar Pixel.png";
        private const string BarName = "Health Bar";
        private const int TrailSortingOrder = 8;
        private const int FillSortingOrder = 9;
        private const float PixelsPerUnit = 4f;
        private static readonly Color FillColor = new Color(1f, 0.16f, 0.16f);
        private static readonly Color TrailColor = new Color(1f, 0.86f, 0.2f);

        private static readonly string[] Prefabs =
        {
            "Assets/Prefabs/Enemies/Enemy 1.prefab", "Assets/Prefabs/Enemies/Enemy 2.prefab",
            "Assets/Prefabs/Enemies/Enemy 3.prefab", "Assets/Prefabs/Enemies/Enemy 4.prefab",
            "Assets/Prefabs/Enemies/Enemy 5.prefab", "Assets/Prefabs/Enemies/Enemy 6.prefab",
            "Assets/Prefabs/Bosses/Boss 1.prefab", "Assets/Prefabs/Bosses/Boss 2.prefab",
            "Assets/Prefabs/Bosses/Boss 3.prefab", "Assets/Prefabs/Bosses/Boss 4.prefab",
            "Assets/Prefabs/Bosses/Boss 5.prefab"
        };

        [MenuItem("Tools/Enemies/Build Health Bars")]
        public static void Build()
        {
            Sprite pixel = PreparePixel();

            foreach (string path in Prefabs)
                Attach(path, pixel);

            Debug.Log("Полоски прочности: добавлены на " + Prefabs.Length + " префабов");
        }

        private static Sprite PreparePixel()
        {
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(PixelPath);
            TextureImporterSettings settings = new TextureImporterSettings();

            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.LeftCenter;
            importer.SetTextureSettings(settings);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(PixelPath);
        }

        private static void Attach(string path, Sprite pixel)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);

            try
            {
                Transform old = root.transform.Find(BarName);

                if (old != null)
                    Object.DestroyImmediate(old.gameObject);

                GameObject bar = new GameObject(BarName);
                bar.transform.SetParent(root.transform, false);

                Transform trail = AddPart(bar.transform, "Trail", pixel, TrailColor, TrailSortingOrder);
                Transform fill = AddPart(bar.transform, "Fill", pixel, FillColor, FillSortingOrder);

                EnemyHealthBar view = root.GetComponent<EnemyHealthBar>();

                if (view == null)
                    view = root.AddComponent<EnemyHealthBar>();

                SerializedObject so = new SerializedObject(view);
                so.FindProperty("_health").objectReferenceValue = root.GetComponent<Health>();
                so.FindProperty("_body").objectReferenceValue = FindBody(root);
                so.FindProperty("_bar").objectReferenceValue = bar.transform;
                so.FindProperty("_fill").objectReferenceValue = fill;
                so.FindProperty("_trail").objectReferenceValue = trail;
                so.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Transform AddPart(Transform bar, string name, Sprite pixel, Color color, int order)
        {
            GameObject part = new GameObject(name);
            part.transform.SetParent(bar, false);

            SpriteRenderer renderer = part.AddComponent<SpriteRenderer>();
            renderer.sprite = pixel;
            renderer.color = color;
            renderer.sortingOrder = order;

            return part.transform;
        }

        private static SpriteRenderer FindBody(GameObject root)
        {
            foreach (SpriteRenderer renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (renderer.transform.parent == null || renderer.transform.parent.name != BarName)
                    return renderer;
            }

            return null;
        }
    }
}
