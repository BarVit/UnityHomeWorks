using System.IO;
using UnityEditor;
using UnityEngine;

namespace WeaponContent
{
    public static class SparkPrefabBuilder
    {
        private const string SourcePath = "Assets/VFX/Sparks/vfx_Sparks_01.prefab";
        private const string WarmPath = "Assets/VFX/Sparks/Hit Sparks.prefab";
        private const string ColdPath = "Assets/VFX/Sparks/Hit Sparks Blue.prefab";
        private const string MuzzlePath = "Assets/VFX/Sparks/Muzzle Sparks.prefab";

        [MenuItem("Tools/Weapons/Build Hit Sparks")]
        public static void Build()
        {
            Make(WarmPath, "Hit Sparks", Color.clear);
            Make(ColdPath, "Hit Sparks Blue", new Color(0.3f, 0.75f, 1f, 1f));
            MakeDirected(MuzzlePath, "Muzzle Sparks", new Color(1f, 0.85f, 0.15f, 1f), -90f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Искры попадания: собраны");
        }

        private static void Make(string path, string name, Color tint)
        {
            if (File.Exists(path))
                AssetDatabase.DeleteAsset(path);

            if (AssetDatabase.CopyAsset(SourcePath, path) == false)
            {
                Debug.LogError("Искры: не удалось скопировать " + SourcePath);

                return;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(path);

            root.name = name;
            Strip<BumpEffect>(root);

            AudioSource audio = root.GetComponent<AudioSource>();

            if (audio == null)
                audio = root.AddComponent<AudioSource>();

            audio.playOnAwake = false;

            ExplosionAnimation explosion = root.GetComponent<ExplosionAnimation>();

            if (explosion == null)
                explosion = root.AddComponent<ExplosionAnimation>();

            SerializedObject serialized = new SerializedObject(explosion);

            serialized.FindProperty("_audioSource").objectReferenceValue = audio;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            if (tint != Color.clear)
                Paint(root, tint);

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void MakeDirected(string path, string name, Color tint, float turn)
        {
            if (File.Exists(path))
                AssetDatabase.DeleteAsset(path);

            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePath);

            if (source == null)
            {
                Debug.LogError("Искры: не найден исходник " + SourcePath);

                return;
            }

            GameObject root = new GameObject(name);
            GameObject spray = (GameObject)PrefabUtility.InstantiatePrefab(source, root.transform);

            PrefabUtility.UnpackPrefabInstance(spray, PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction);

            spray.name = "Spray";
            spray.transform.localPosition = Vector3.zero;
            spray.transform.localRotation = Quaternion.Euler(0f, 0f, turn);

            Strip<BumpEffect>(spray);

            AudioSource audio = root.AddComponent<AudioSource>();

            audio.playOnAwake = false;

            ExplosionAnimation explosion = root.AddComponent<ExplosionAnimation>();
            SerializedObject serialized = new SerializedObject(explosion);

            serialized.FindProperty("_audioSource").objectReferenceValue = audio;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Paint(root, tint);
            Float(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void Float(GameObject root)
        {
            foreach (ParticleSystem system in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                ParticleSystem.MainModule main = system.main;

                main.gravityModifier = 0f;
            }
        }

        private static void Strip<TComponent>(GameObject root) where TComponent : Component
        {
            TComponent found = root.GetComponent<TComponent>();

            if (found != null)
                Object.DestroyImmediate(found, true);
        }

        private static void Paint(GameObject root, Color tint)
        {
            foreach (ParticleSystem system in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                ParticleSystem.MainModule main = system.main;

                main.startColor = new ParticleSystem.MinMaxGradient(tint);
            }
        }
    }
}
