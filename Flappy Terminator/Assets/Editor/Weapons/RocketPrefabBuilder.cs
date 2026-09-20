using System.IO;
using UnityEditor;
using UnityEngine;

namespace WeaponContent
{
    public static class RocketPrefabBuilder
    {
        private const string SourcePath = "Assets/Prefabs/Enemy Bullet.prefab";
        private const string TargetPath = "Assets/Prefabs/Player Rocket.prefab";
        private const int PlayerAmmoLayer = 7;

        [MenuItem("Tools/Weapons/Build Player Rocket")]
        public static void Build()
        {
            if (File.Exists(TargetPath))
                AssetDatabase.DeleteAsset(TargetPath);

            if (AssetDatabase.CopyAsset(SourcePath, TargetPath) == false)
            {
                Debug.LogError("Ракета: не удалось скопировать " + SourcePath);

                return;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(TargetPath);

            root.name = "Player Rocket";
            Move(root.transform);

            PrefabUtility.SaveAsPrefabAsset(root, TargetPath);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Ракета игрока: собрана из вражеской");
        }

        private static void Move(Transform target)
        {
            target.gameObject.layer = PlayerAmmoLayer;

            for (int i = 0; i < target.childCount; i++)
                Move(target.GetChild(i));
        }
    }
}
