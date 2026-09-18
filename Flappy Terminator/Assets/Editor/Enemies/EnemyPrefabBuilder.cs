using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace EnemyContent
{
    public static class EnemyPrefabBuilder
    {
        private const string BasePath = "Assets/Prefabs/Enemy Ship.prefab";
        private const string Folder = "Assets/Prefabs/Enemies";
        private const string MountRootName = "Mounts";
        private const int ShipCount = 5;

        private static readonly Vector3[] MountPositions =
        {
            new Vector3(-0.6f, 0f, 0f),
            new Vector3(-0.5f, 0.32f, 0f),
            new Vector3(-0.5f, -0.32f, 0f),
            new Vector3(-0.85f, 0f, 0f)
        };

        [MenuItem("Tools/Enemies/Build Prefabs")]
        public static void Build()
        {
            if (Directory.Exists(Folder) == false)
                AssetDatabase.CreateFolder("Assets/Prefabs", "Enemies");

            string firstPath = GetPath(1);

            if (File.Exists(BasePath))
                AssetDatabase.MoveAsset(BasePath, firstPath);

            if (File.Exists(firstPath) == false)
            {
                Debug.LogError("EnemyPrefabBuilder: не найден исходный префаб " + firstPath);

                return;
            }

            for (int number = 1; number <= ShipCount; number++)
                Shape(number, firstPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Враги: собрано префабов — " + ShipCount);
        }

        private static void Shape(int number, string firstPath)
        {
            string path = GetPath(number);

            if (File.Exists(path) == false)
                AssetDatabase.CopyAsset(firstPath, path);

            GameObject root = PrefabUtility.LoadPrefabContents(path);

            root.name = GetName(number);
            SetSprite(root, number);
            SetController(root, number);
            SetMounts(root);

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void SetSprite(GameObject root, int number)
        {
            SpriteRenderer renderer = root.GetComponentInChildren<SpriteRenderer>(true);
            Sprite sprite = LoadFirstSprite("Assets/Sprites/Ships/" + GetName(number) + ".png");

            if (renderer == null || sprite == null)
            {
                Debug.LogWarning("EnemyPrefabBuilder: " + GetName(number) + " — рисователь "
                    + (renderer == null ? "не найден" : renderer.name) + ", спрайт "
                    + (sprite == null ? "не найден" : sprite.name));

                return;
            }

            renderer.sprite = sprite;
        }

        private static void SetController(GameObject root, int number)
        {
            Animator animator = root.GetComponentInChildren<Animator>(true);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                "Assets/Animations/" + GetName(number) + " Idle.anim");

            if (animator == null || clip == null)
                return;

            string path = "Assets/Animations/" + GetName(number) + " animation controller.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);

            if (controller == null)
                controller = AnimatorController.CreateAnimatorControllerAtPathWithClip(path, clip);

            animator.runtimeAnimatorController = controller;
        }

        private static void SetMounts(GameObject root)
        {
            Transform mounts = GetMountRoot(root.transform);

            mounts.localPosition = Vector3.zero;
            mounts.localRotation = Quaternion.identity;

            for (int i = mounts.childCount; i < MountPositions.Length; i++)
                new GameObject("Mount " + (i + 1)).transform.SetParent(mounts, false);

            for (int i = 0; i < MountPositions.Length; i++)
            {
                Transform mount = mounts.GetChild(i);

                mount.name = "Mount " + (i + 1);
                mount.localPosition = MountPositions[i];
                mount.localRotation = Quaternion.Euler(0f, 0f, 180f);
            }

            EnemyShooter shooter = root.GetComponent<EnemyShooter>();

            if (shooter == null)
                return;

            SerializedObject serialized = new SerializedObject(shooter);

            serialized.FindProperty("_mountRoot").objectReferenceValue = mounts;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform GetMountRoot(Transform root)
        {
            Transform found = root.Find(MountRootName);

            if (found != null)
                return found;

            found = root.Find("ShootPoint");

            if (found != null)
            {
                found.name = MountRootName;

                return found;
            }

            Transform created = new GameObject(MountRootName).transform;

            created.SetParent(root, false);

            return created;
        }

        private static Sprite LoadFirstSprite(string path)
        {
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                Sprite sprite = asset as Sprite;

                if (sprite != null)
                    return sprite;
            }

            return null;
        }

        private static string GetPath(int number)
        {
            return Folder + "/" + GetName(number) + ".prefab";
        }

        private static string GetName(int number)
        {
            return "Enemy " + number;
        }
    }
}
