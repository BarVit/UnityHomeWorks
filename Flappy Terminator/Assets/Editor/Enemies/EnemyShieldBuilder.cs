using ShipContent;
using UnityEditor;
using UnityEngine;

namespace EnemyContent
{
    public static class EnemyShieldBuilder
    {
        private const string Folder = "Assets/Prefabs/Enemies/";
        private const int ShipCount = 5;

        [MenuItem("Tools/Enemies/Build Shields")]
        public static void Build()
        {
            for (int number = 1; number <= ShipCount; number++)
                Attach(Folder + "Enemy " + number + ".prefab");

            AssetDatabase.SaveAssets();
            Debug.Log("Враги: щиты поставлены на " + ShipCount + " префабов");
        }

        private static void Attach(string path)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);

            try
            {
                EnemyShip ship = root.GetComponent<EnemyShip>();
                SerializedObject shipSo = new SerializedObject(ship);
                AmmoHitHandler bodyHits = (AmmoHitHandler)shipSo.FindProperty("_ammoHitHandler").objectReferenceValue;
                SpriteRenderer body = root.GetComponentInChildren<SpriteRenderer>(true);

                EnergyShield shield = ShieldRig.Attach(root, body, bodyHits, out AmmoHitHandler shieldHits);

                shipSo.FindProperty("_shield").objectReferenceValue = shield;
                shipSo.FindProperty("_shieldHitHandler").objectReferenceValue = shieldHits;
                shipSo.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
