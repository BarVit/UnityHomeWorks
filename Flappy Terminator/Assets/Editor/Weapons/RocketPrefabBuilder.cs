using System.IO;
using UnityEditor;
using UnityEngine;

namespace WeaponContent
{
    public static class RocketPrefabBuilder
    {
        private const string SourcePath = "Assets/Prefabs/Enemy Bullet.prefab";
        private const string TargetPath = "Assets/Prefabs/Player Rocket.prefab";
        private const float HitVolume = 0.5f;
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
            Soften(root);

            PrefabUtility.SaveAsPrefabAsset(root, TargetPath);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Ракета игрока: собрана из вражеской");
        }

        private static void Soften(GameObject root)
        {
            Ammo ammo = root.GetComponent<Ammo>();

            if (ammo == null)
                return;

            SerializedObject serialized = new SerializedObject(ammo);

            serialized.FindProperty("_hitSound._volume").floatValue = HitVolume;
            serialized.FindProperty("_worldHitSound._volume").floatValue = HitVolume;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Move(Transform target)
        {
            target.gameObject.layer = PlayerAmmoLayer;

            for (int i = 0; i < target.childCount; i++)
                Move(target.GetChild(i));
        }
    }
}
