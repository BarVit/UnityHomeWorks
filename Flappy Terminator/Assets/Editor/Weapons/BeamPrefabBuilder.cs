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

        [MenuItem("Tools/Weapons/Build Beams")]
        public static void Build()
        {
            if (Directory.Exists(Folder) == false)
                AssetDatabase.CreateFolder("Assets/Prefabs", "Beams");

            Material material = GetMaterial();

            Create("Railgun Beam", material, new Color(0.65f, 0.9f, 1f), 0.16f, 0.07f,
                new BeamSounds(RailgunShotPath, RailgunShotVolume, RailgunHitPath, RailgunHitVolume,
                    RailgunWorldHitPath, RailgunWorldHitVolume, null, 0f, null, 0f), RailgunPiercePath);
            Create("Laser Beam", material, new Color(1f, 0.35f, 0.25f), 0.09f, 0.05f,
                new BeamSounds(null, 0f, null, 0f, null, 0f, LaserLoopPath, LaserLoopVolume,
                    LaserBurnPath, LaserBurnVolume), null);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Лучи: префабы собраны");
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

        private static void Create(string name, Material material, Color color, float width,
            float lifetime, BeamSounds sounds, string piercePath)
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
            line.widthCurve = AnimationCurve.Linear(0f, width, 1f, width * 0.55f);
            line.colorGradient = GetGradient(color);
            line.sortingOrder = 5;
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
