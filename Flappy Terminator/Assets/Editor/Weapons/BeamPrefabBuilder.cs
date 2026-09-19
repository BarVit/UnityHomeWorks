using System.IO;
using UnityEditor;
using UnityEngine;

namespace WeaponContent
{
    public static class BeamPrefabBuilder
    {
        private const string Folder = "Assets/Prefabs/Beams";
        private const string MaterialPath = "Assets/Prefabs/Beams/Beam.mat";

        [MenuItem("Tools/Weapons/Build Beams")]
        public static void Build()
        {
            if (Directory.Exists(Folder) == false)
                AssetDatabase.CreateFolder("Assets/Prefabs", "Beams");

            Material material = GetMaterial();

            Create("Railgun Beam", material, new Color(0.65f, 0.9f, 1f), 0.16f, 0.07f,
                "Assets/Sounds/Rocket 2.wav", 0.25f);
            Create("Laser Beam", material, new Color(1f, 0.35f, 0.25f), 0.09f, 0.05f,
                "Assets/Sounds/Plasma hit 2.wav", 0.12f);

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
            float lifetime, string soundPath, float volume)
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
            serialized.FindProperty("_shotSound._clip").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<AudioClip>(soundPath);
            serialized.FindProperty("_shotSound._volume").floatValue = volume;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
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
