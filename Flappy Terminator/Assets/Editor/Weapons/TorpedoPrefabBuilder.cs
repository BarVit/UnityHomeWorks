using System.IO;
using UnityEditor;
using UnityEngine;

namespace WeaponContent
{
    public static class TorpedoPrefabBuilder
    {
        private const string SourcePath = "Assets/Prefabs/Player Rocket.prefab";
        private const string TargetPath = "Assets/Prefabs/Torpedo.prefab";
        private const string BlastPath = "Assets/Prefabs/Blast Wave.prefab";
        private const string BlastSpritePath = "Assets/Sprites/Ammo/Blast/Blast Wave.png";
        private const string LampSpritePath = "Assets/Sprites/VFX/Smoke.png";
        private const string AtlasPath = "Assets/Sprites/Ammo/Torpedo/2DShipsMissilesTorpedoesAtlas.png";
        private const string BodySpriteName = "2DShipsMissilesTorpedoesAtlas_0";
        private const string MaterialPath = "Assets/VFX/Plasma Additive.mat";
        private const float SizeAgainstRocket = 2f;

        [MenuItem("Tools/Weapons/Build Torpedo")]
        public static void Build()
        {
            BlastWave blast = BuildBlast();

            if (File.Exists(TargetPath))
                AssetDatabase.DeleteAsset(TargetPath);

            AssetDatabase.CopyAsset(SourcePath, TargetPath);

            GameObject root = PrefabUtility.LoadPrefabContents(TargetPath);

            root.name = "Torpedo";

            float rocketLength = GetRocketLength();
            SpriteRenderer body = root.GetComponentInChildren<SpriteRenderer>(true);
            Sprite sprite = FindSprite();

            if (body != null && sprite != null)
            {
                body.sprite = sprite;

                float scale = rocketLength * SizeAgainstRocket / sprite.bounds.size.y;

                body.transform.localScale = new Vector3(scale, scale, 1f);

                SetHull(root, sprite.bounds.size.y * scale, sprite.bounds.size.x * scale);
            }

            SetLamp(root, body);

            TorpedoWarhead warhead = root.GetComponent<TorpedoWarhead>();

            if (warhead == null)
                warhead = root.AddComponent<TorpedoWarhead>();

            SerializedObject serialized = new SerializedObject(warhead);

            serialized.FindProperty("_blast").objectReferenceValue = blast;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, TargetPath);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Торпеда: собрана");
        }

        private static void SetHull(GameObject root, float length, float width)
        {
            BoxCollider2D hull = root.GetComponent<BoxCollider2D>();

            if (hull == null)
                return;

            hull.size = new Vector2(length, width);
            hull.offset = Vector2.zero;
        }

        private static float GetRocketLength()
        {
            GameObject rocket = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePath);

            if (rocket == null)
                return 2f;

            SpriteRenderer renderer = rocket.GetComponentInChildren<SpriteRenderer>(true);

            if (renderer == null || renderer.sprite == null)
                return 2f;

            return renderer.sprite.bounds.size.y * renderer.transform.localScale.y;
        }

        private static Sprite FindSprite()
        {
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(AtlasPath))
            {
                Sprite sprite = asset as Sprite;

                if (sprite != null && sprite.name == BodySpriteName)
                    return sprite;
            }

            return null;
        }

        private static void SetLamp(GameObject root, SpriteRenderer body)
        {
            Transform lamp = root.transform.Find("Warhead");

            if (lamp == null)
            {
                lamp = new GameObject("Warhead").transform;
                lamp.SetParent(root.transform, false);
            }

            float length = body == null ? 2f : body.sprite.bounds.size.y * body.transform.localScale.y;

            lamp.localPosition = new Vector3(length * 0.42f, 0f, 0f);
            lamp.localScale = Vector3.one * (length * 0.14f);

            SpriteRenderer renderer = lamp.GetComponent<SpriteRenderer>();

            if (renderer == null)
                renderer = lamp.gameObject.AddComponent<SpriteRenderer>();

            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(LampSpritePath);
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            renderer.color = new Color(1f, 0.15f, 0.1f, 1f);
            renderer.sortingOrder = 6;

            if (lamp.GetComponent<WarheadBlink>() == null)
                lamp.gameObject.AddComponent<WarheadBlink>();
        }

        private static BlastWave BuildBlast()
        {
            if (File.Exists(BlastPath))
                AssetDatabase.DeleteAsset(BlastPath);

            GameObject root = new GameObject("Blast Wave");
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();

            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BlastSpritePath);
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            renderer.sortingOrder = 7;

            root.AddComponent<BlastWave>();

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, BlastPath);

            Object.DestroyImmediate(root);

            return saved.GetComponent<BlastWave>();
        }
    }
}
