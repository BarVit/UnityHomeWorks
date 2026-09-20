using System.IO;
using UnityEditor;
using UnityEngine;

namespace WeaponContent
{
    public static class OrbPrefabBuilder
    {
        private const string SourcePath = "Assets/Prefabs/PlayerBullet.prefab";
        private const string TargetPath = "Assets/Prefabs/Plasma Orb.prefab";
        private const string SpritePath = "Assets/Sprites/Ammo/Orb/Plasma Orb.png";

        [MenuItem("Tools/Weapons/Build Plasma Orb")]
        public static void Build()
        {
            if (File.Exists(TargetPath) == false)
                AssetDatabase.CopyAsset(SourcePath, TargetPath);

            GameObject root = PrefabUtility.LoadPrefabContents(TargetPath);

            root.name = "Plasma Orb";
            Strip<Animator>(root);
            SetSprite(root);
            SetCollider(root);

            if (root.GetComponent<OrbGrowth>() == null)
                root.AddComponent<OrbGrowth>();

            PrefabUtility.SaveAsPrefabAsset(root, TargetPath);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Плазменный шар: снаряд собран");
        }

        private static void Strip<TComponent>(GameObject root) where TComponent : Component
        {
            TComponent found = root.GetComponent<TComponent>();

            if (found != null)
                Object.DestroyImmediate(found, true);
        }

        private static void SetSprite(GameObject root)
        {
            SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);

            if (renderer == null || sprite == null)
            {
                Debug.LogWarning("Плазменный шар: не найден спрайт " + SpritePath);

                return;
            }

            renderer.sprite = sprite;
        }

        private static void SetCollider(GameObject root)
        {
            CapsuleCollider2D collider = root.GetComponent<CapsuleCollider2D>();

            if (collider == null)
                return;

            collider.direction = CapsuleDirection2D.Horizontal;
            collider.size = new Vector2(1.2f, 1.2f);
            collider.offset = Vector2.zero;
        }
    }
}
