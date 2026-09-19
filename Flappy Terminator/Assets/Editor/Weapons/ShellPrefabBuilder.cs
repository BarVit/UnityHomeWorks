using System.IO;
using UnityEditor;
using UnityEngine;

namespace WeaponContent
{
    public static class ShellPrefabBuilder
    {
        private const string SourcePath = "Assets/Prefabs/PlayerBullet.prefab";
        private const string TargetPath = "Assets/Prefabs/Cannon Shell.prefab";
        private const string SpritePath = "Assets/Sprites/Ammo/Cannon/Cannon Shell.png";
        private const string MaterialPath = "Assets/Prefabs/Beams/Beam.mat";
        private const string SoundPath = "Assets/Sounds/Rocket 1.wav";
        private const float Scale = 0.246f;

        [MenuItem("Tools/Weapons/Build Cannon Shell")]
        public static void Build()
        {
            if (File.Exists(TargetPath) == false)
                AssetDatabase.CopyAsset(SourcePath, TargetPath);

            GameObject root = PrefabUtility.LoadPrefabContents(TargetPath);

            root.name = "Cannon Shell";
            root.transform.localScale = new Vector3(Scale, Scale, 1f);

            Strip<Animator>(root);
            SetSprite(root);
            SetCollider(root);
            SetTrail(root);
            SetSound(root);

            PrefabUtility.SaveAsPrefabAsset(root, TargetPath);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Пушка: снаряд собран");
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
                Debug.LogWarning("Пушка: не найден спрайт " + SpritePath);

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
            collider.size = new Vector2(1.9f, 0.4f);
            collider.offset = Vector2.zero;
        }

        private static void SetTrail(GameObject root)
        {
            TrailRenderer trail = root.GetComponent<TrailRenderer>();

            if (trail == null)
                trail = root.AddComponent<TrailRenderer>();

            trail.time = 0.035f;
            trail.minVertexDistance = 0.02f;
            trail.autodestruct = false;
            trail.emitting = true;
            trail.alignment = LineAlignment.View;
            trail.textureMode = LineTextureMode.Stretch;
            trail.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            trail.widthCurve = AnimationCurve.Linear(0f, 0.07f, 1f, 0f);
            trail.colorGradient = GetGradient();
            trail.sortingOrder = 3;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows = false;

            if (root.GetComponent<TrailReset>() == null)
                root.AddComponent<TrailReset>();
        }

        private static void SetSound(GameObject root)
        {
            Ammo ammo = root.GetComponent<Ammo>();

            if (ammo == null)
                return;

            SerializedObject serialized = new SerializedObject(ammo);

            serialized.FindProperty("_shotSound._clip").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<AudioClip>(SoundPath);
            serialized.FindProperty("_shotSound._volume").floatValue = 0.2f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Gradient GetGradient()
        {
            Gradient gradient = new Gradient();

            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(1f, 0.75f, 0.35f), 0f),
                    new GradientColorKey(new Color(1f, 0.45f, 0.1f), 1f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0.85f, 0f),
                    new GradientAlphaKey(0f, 1f)
                });

            return gradient;
        }
    }
}
