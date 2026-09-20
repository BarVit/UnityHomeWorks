using System.IO;
using UnityEditor;
using UnityEngine;

namespace WeaponContent
{
    public static class ImpulsePrefabBuilder
    {
        private const string SourcePath = "Assets/Prefabs/PlayerBullet.prefab";
        private const string TargetPath = "Assets/Prefabs/Impulse Bolt.prefab";
        private const string SpritePath = "Assets/Sprites/Ammo/Impulse/Impulse Bolt.png";
        private const string MaterialPath = "Assets/VFX/Plasma Additive.mat";

        [MenuItem("Tools/Weapons/Build Impulse Bolt")]
        public static void Build()
        {
            if (File.Exists(TargetPath))
                AssetDatabase.DeleteAsset(TargetPath);

            AssetDatabase.CopyAsset(SourcePath, TargetPath);

            GameObject root = PrefabUtility.LoadPrefabContents(TargetPath);

            root.name = "Impulse Bolt";
            root.transform.localScale = Vector3.one;

            Animator animator = root.GetComponent<Animator>();

            if (animator != null)
                Object.DestroyImmediate(animator, true);

            SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();

            if (renderer != null)
            {
                renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
                renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
                renderer.color = Color.white;
            }

            CapsuleCollider2D collider = root.GetComponent<CapsuleCollider2D>();

            if (collider != null)
            {
                collider.direction = CapsuleDirection2D.Horizontal;
                collider.size = new Vector2(1.4f, 0.28f);
                collider.offset = Vector2.zero;
            }

            Ammo ammo = root.GetComponent<Ammo>();

            if (ammo != null)
            {
                SerializedObject serialized = new SerializedObject(ammo);

                serialized.FindProperty("_hitEffectPrefab").objectReferenceValue = null;
                serialized.FindProperty("_shotSound._clip").objectReferenceValue = null;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(root, TargetPath);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Импульсный лазер: снаряд собран");
        }
    }
}
