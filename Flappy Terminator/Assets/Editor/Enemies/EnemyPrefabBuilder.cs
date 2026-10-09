using System.Collections.Generic;
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
        private const string BossFolder = "Assets/Prefabs/Bosses";
        private const int BossCount = 5;
        private const float BaseHalfWidth = 0.39f;
        private const float BaseHalfHeight = 0.405f;
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

        [MenuItem("Tools/Enemies/Build Boss Prefabs")]
        public static void BuildBosses()
        {
            if (Directory.Exists(BossFolder) == false)
                AssetDatabase.CreateFolder("Assets/Prefabs", "Bosses");

            for (int number = 1; number <= BossCount; number++)
                ShapeBoss(number);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Боссы: собрано префабов — " + BossCount);
        }

        private static void Shape(int number, string firstPath)
        {
            string path = GetPath(number);

            if (File.Exists(path) == false)
                AssetDatabase.CopyAsset(firstPath, path);

            GameObject root = PrefabUtility.LoadPrefabContents(path);

            root.name = GetName(number);

            SpriteRenderer renderer = SetSprite(root, GetName(number));

            SetController(root, GetName(number));
            SetMounts(root, Vector2.one);
            FitCollider(renderer);

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void ShapeBoss(int number)
        {
            string name = "Boss " + number;
            string path = BossFolder + "/" + name + ".prefab";

            if (File.Exists(path) == false)
                AssetDatabase.CopyAsset(GetPath(1), path);

            GameObject root = PrefabUtility.LoadPrefabContents(path);

            root.name = name;

            SpriteRenderer renderer = SetSprite(root, name);

            SetController(root, name);
            SetMounts(root, GetStretch(renderer));
            FitCollider(renderer);

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static Vector2 GetStretch(SpriteRenderer renderer)
        {
            if (renderer == null || renderer.sprite == null)
                return Vector2.one;

            Vector3 scale = renderer.transform.localScale;
            Vector3 extents = renderer.sprite.bounds.extents;

            return new Vector2(extents.x * scale.x / BaseHalfWidth, extents.y * scale.y / BaseHalfHeight);
        }

        private static void FitCollider(SpriteRenderer renderer)
        {
            if (renderer == null || renderer.sprite == null)
                return;

            PolygonCollider2D collider = renderer.GetComponent<PolygonCollider2D>();

            if (collider == null)
                return;

            Sprite sprite = renderer.sprite;
            int count = sprite.GetPhysicsShapeCount();
            List<Vector2> points = new List<Vector2>();

            if (count == 0)
            {
                Bounds bounds = sprite.bounds;

                collider.pathCount = 1;
                collider.SetPath(0, new[]
                {
                    new Vector2(bounds.min.x, bounds.min.y), new Vector2(bounds.min.x, bounds.max.y),
                    new Vector2(bounds.max.x, bounds.max.y), new Vector2(bounds.max.x, bounds.min.y)
                });

                return;
            }

            collider.pathCount = count;

            for (int shape = 0; shape < count; shape++)
            {
                points.Clear();
                sprite.GetPhysicsShape(shape, points);
                collider.SetPath(shape, points);
            }
        }

        private static SpriteRenderer SetSprite(GameObject root, string name)
        {
            SpriteRenderer renderer = root.GetComponentInChildren<SpriteRenderer>(true);
            Sprite sprite = LoadFirstSprite("Assets/Sprites/Ships/" + name + ".png");

            if (renderer == null || sprite == null)
            {
                Debug.LogWarning("EnemyPrefabBuilder: " + name + " — рисователь "
                    + (renderer == null ? "не найден" : renderer.name) + ", спрайт "
                    + (sprite == null ? "не найден" : sprite.name));

                return renderer;
            }

            renderer.sprite = sprite;

            return renderer;
        }

        private static void SetController(GameObject root, string name)
        {
            Animator animator = root.GetComponentInChildren<Animator>(true);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                "Assets/Animations/" + name + " Idle.anim");

            if (animator == null || clip == null)
                return;

            string path = "Assets/Animations/" + name + " animation controller.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);

            if (controller == null)
                controller = AnimatorController.CreateAnimatorControllerAtPathWithClip(path, clip);

            animator.runtimeAnimatorController = controller;
        }

        private static void SetMounts(GameObject root, Vector2 stretch)
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
                mount.localPosition = Vector3.Scale(MountPositions[i], new Vector3(stretch.x, stretch.y, 1f));
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
