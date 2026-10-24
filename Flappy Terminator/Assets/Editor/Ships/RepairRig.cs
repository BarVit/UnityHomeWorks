using UnityEditor;
using UnityEngine;

namespace ShipContent
{
    public static class RepairRig
    {
        public const string RepairName = "Repair Drone";

        private const string ShaderName = "FlappyTerminator/SpriteSilhouette";
        private const string MaterialPath = "Assets/VFX/Repair/Repair Outline.mat";
        private const string ArmorShaderName = "FlappyTerminator/HexArmor";
        private const string ArmorMaterialPath = "Assets/VFX/Repair/Nanobot Armor.mat";

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
            so.FindProperty("_outlineMaterial").objectReferenceValue = LoadMaterial(MaterialPath, ShaderName, "Repair Outline");
            so.FindProperty("_armorMaterial").objectReferenceValue = LoadMaterial(ArmorMaterialPath, ArmorShaderName,
                "Nanobot Armor");
            so.ApplyModifiedPropertiesWithoutUndo();

            return repair;
        }

        private static Material LoadMaterial(string path, string shaderName, string name)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material != null)
                return material;

            material = new Material(Shader.Find(shaderName)) { name = name };
            AssetDatabase.CreateAsset(material, path);

            return material;
        }
    }
}
