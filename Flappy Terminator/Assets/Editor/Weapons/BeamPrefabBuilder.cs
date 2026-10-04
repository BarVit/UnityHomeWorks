using System.IO;
using UnityEditor;
using UnityEngine;

namespace WeaponContent
{
    public static class BeamPrefabBuilder
    {
        private const string Folder = "Assets/Prefabs/Beams";
        private const string MaterialPath = "Assets/Prefabs/Beams/Beam.mat";
        private const string RailgunShotPath = "Assets/Sounds/Railgun [394672].wav";
        private const float RailgunShotVolume = 0.5f;
        private const string RailgunHitPath = "Assets/Sounds/Railgun enemy hit [724428].wav";
        private const float RailgunHitVolume = 0.15f;
        private const string RailgunWorldHitPath = "Assets/Sounds/Railgun hit [871710].wav";
        private const float RailgunWorldHitVolume = 0.2f;
        private const string RailgunPiercePath = "Assets/VFX/Sparks/Pierce Sparks.prefab";
        private const string LaserLoopPath = "Assets/Sounds/Beam laser trill.wav";
        private const float LaserLoopVolume = 0.11f;
        private const string LaserBurnPath = "Assets/Sounds/Beam burn [843455].wav";
        private const float LaserBurnVolume = 0.74f;
        private const string SightName = "Railgun Sight";
        private const float SightWidth = 0.02f;
        private const float SightNearAlpha = 0f;
        private const int SightSortingOrder = 4;
        private static readonly Color SightColor = new Color(1f, 0.1f, 0.1f);
        private const float SpiralWidth = 0.05f;
        private const float SpiralLifetimeShare = 2f;
        private const int CoreSortingOrder = 6;
        private const int SpiralSortingOrder = 5;
        private static readonly Color SpiralNear = new Color(0.9f, 0.97f, 1f);
        private static readonly Color SpiralFar = new Color(0.45f, 0.75f, 1f);

        [MenuItem("Tools/Weapons/Build Beams")]
        public static void Build()
        {
            if (Directory.Exists(Folder) == false)
                AssetDatabase.CreateFolder("Assets/Prefabs", "Beams");

            Material material = GetMaterial();
            AimLine sight = CreateSight(material);

            Create("Railgun Beam", material, Color.white, 0.03f, 1f, 0.14f,
                new BeamSounds(RailgunShotPath, RailgunShotVolume, RailgunHitPath, RailgunHitVolume,
                    RailgunWorldHitPath, RailgunWorldHitVolume, null, 0f, null, 0f), RailgunPiercePath, sight, true);
            Create("Laser Beam", material, new Color(1f, 0.35f, 0.25f), 0.09f, 0.55f, 0.05f,
                new BeamSounds(null, 0f, null, 0f, null, 0f, LaserLoopPath, LaserLoopVolume,
                    LaserBurnPath, LaserBurnVolume), null, null, false);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Лучи: префабы собраны");
        }

        private static LineRenderer AddSpiral(GameObject root, Material material)
        {
            GameObject holder = new GameObject("Spiral");

            holder.transform.SetParent(root.transform, false);

            LineRenderer spiral = holder.AddComponent<LineRenderer>();
            Gradient gradient = new Gradient();

            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(SpiralNear, 0f), new GradientColorKey(SpiralFar, 0.2f), new GradientColorKey(SpiralFar, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });

            spiral.useWorldSpace = true;
            spiral.positionCount = 0;
            spiral.numCapVertices = 2;
            spiral.alignment = LineAlignment.View;
            spiral.textureMode = LineTextureMode.Stretch;
            spiral.sharedMaterial = material;
            spiral.widthMultiplier = SpiralWidth;
            spiral.colorGradient = gradient;
            spiral.sortingOrder = SpiralSortingOrder;
            spiral.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            spiral.receiveShadows = false;

            return spiral;
        }

        private static AimLine CreateSight(Material material)
        {
            string path = Folder + "/" + SightName + ".prefab";
            GameObject root = new GameObject(SightName);
            LineRenderer line = root.AddComponent<LineRenderer>();
            Gradient gradient = new Gradient();

            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(SightColor, 0f), new GradientColorKey(SightColor, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(SightNearAlpha, 0f), new GradientAlphaKey(1f, 1f) });

            line.useWorldSpace = true;
            line.positionCount = 2;
            line.alignment = LineAlignment.View;
            line.textureMode = LineTextureMode.Stretch;
            line.sharedMaterial = material;
            line.widthMultiplier = SightWidth;
            line.colorGradient = gradient;
            line.sortingOrder = SightSortingOrder;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;

            root.AddComponent<AimLine>();

            AimLine saved = PrefabUtility.SaveAsPrefabAsset(root, path).GetComponent<AimLine>();

            Object.DestroyImmediate(root);

            return saved;
        }

        private static Material GetMaterial()
        {
            Material found = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);

            if (found != null)
                return found;

            Material created = new Material(Shader.Find("Sprites/Default"));

            AssetDatabase.CreateAsset(created, MaterialPath);

            return created;
        }

        private static void Create(string name, Material material, Color color, float width, float tipShare,
            float lifetime, BeamSounds sounds, string piercePath, AimLine sight, bool hasSpiral)
        {
            string path = Folder + "/" + name + ".prefab";
            GameObject root = new GameObject(name);
            LineRenderer line = root.AddComponent<LineRenderer>();

            line.useWorldSpace = true;
            line.positionCount = 2;
            line.numCapVertices = 4;
            line.textureMode = LineTextureMode.Stretch;
            line.alignment = LineAlignment.View;
            line.sharedMaterial = material;
            line.widthCurve = AnimationCurve.Linear(0f, width, 1f, width * tipShare);
            line.colorGradient = GetGradient(color);
            line.sortingOrder = hasSpiral ? CoreSortingOrder : 5;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;

            BeamFlash flash = root.AddComponent<BeamFlash>();
            SerializedObject serialized = new SerializedObject(flash);

            serialized.FindProperty("_lifetime").floatValue = lifetime;
            SetSound(serialized, "_shotSound", sounds.ShotPath, sounds.ShotVolume);
            SetSound(serialized, "_hitSound", sounds.HitPath, sounds.HitVolume);
            SetSound(serialized, "_worldHitSound", sounds.WorldHitPath, sounds.WorldHitVolume);
            SetSound(serialized, "_loopSound", sounds.LoopPath, sounds.LoopVolume);
            SetSound(serialized, "_hitLoopSound", sounds.HitLoopPath, sounds.HitLoopVolume);
            serialized.FindProperty("_pierceEffect").objectReferenceValue = piercePath == null
                ? null
                : AssetDatabase.LoadAssetAtPath<ExplosionAnimation>(piercePath);
            serialized.FindProperty("_sight").objectReferenceValue = sight;
            serialized.FindProperty("_spiral").objectReferenceValue = hasSpiral ? AddSpiral(root, material) : null;
            serialized.FindProperty("_spiralLifetime").floatValue = lifetime * SpiralLifetimeShare;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void SetSound(SerializedObject serialized, string field, string path, float volume)
        {
            serialized.FindProperty(field + "._clip").objectReferenceValue = path == null
                ? null
                : AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            serialized.FindProperty(field + "._volume").floatValue = volume;
        }

        private static Gradient GetGradient(Color color)
        {
            Gradient gradient = new Gradient();

            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(color, 0.25f),
                    new GradientColorKey(color, 1f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                });

            return gradient;
        }
    }
}
