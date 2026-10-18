using UnityEditor;
using UnityEngine;

namespace ShipContent
{
    public static class RepairRig
    {
        public const string RepairName = "Repair Drone";

        private const string ShaderName = "FlappyTerminator/SpriteSilhouette";
        private const string MaterialPath = "Assets/VFX/Repair/Repair Outline.mat";

        public static ShipRepair Attach(GameObject root, SpriteRenderer body, Health health)
        {
            Transform old = root.transform.Find(RepairName);

            if (old != null)
                Object.DestroyImmediate(old.gameObject);

            GameObject holder = new GameObject(RepairName);
            holder.transform.SetParent(root.transform, false);

            ShipRepair repair = holder.AddComponent<ShipRepair>();
            SerializedObject so = new SerializedObject(repair);
            so.FindProperty("_health").objectReferenceValue = health;
            so.FindProperty("_body").objectReferenceValue = body;
            so.FindProperty("_outlineMaterial").objectReferenceValue = LoadMaterial();
            so.ApplyModifiedPropertiesWithoutUndo();

            return repair;
        }

        private static Material LoadMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);

            if (material != null)
                return material;

            material = new Material(Shader.Find(ShaderName)) { name = "Repair Outline" };
            AssetDatabase.CreateAsset(material, MaterialPath);

            return material;
        }
    }
}
