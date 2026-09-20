using System.IO;
using UnityEditor;
using UnityEngine;

namespace WeaponContent
{
    public static class OrbPrefabBuilder
    {
        private const string SourcePath = "Assets/Prefabs/PlayerBullet.prefab";
        private const string TargetPath = "Assets/Prefabs/Plasma Orb.prefab";
        private const string CorePath = "Assets/Sprites/Ammo/Orb/Orb Core.png";
        private const string HaloPath = "Assets/Sprites/Ammo/Orb/Orb Halo.png";
        private const string VeinsPath = "Assets/Sprites/Ammo/Orb/Orb Veins.png";
        private const string SamplePath = "Assets/GabrielAguiarProductions/Flare00_AB_2.mat";
        private const string AdditivePath = "Assets/VFX/Plasma Additive.mat";
        private const string ArcMaterialPath = "Assets/VFX/Plasma Arc.mat";

        [MenuItem("Tools/Weapons/Build Plasma Orb")]
        public static void Build()
        {
            if (File.Exists(TargetPath) == false)
                AssetDatabase.CopyAsset(SourcePath, TargetPath);

            Material additive = GetAdditive();
            GameObject root = PrefabUtility.LoadPrefabContents(TargetPath);

            root.name = "Plasma Orb";
            Strip<Animator>(root);
            SetCore(root);
            SetCollider(root);

            if (root.GetComponent<OrbGrowth>() == null)
                root.AddComponent<OrbGrowth>();

            Layer(root, "Halo", HaloPath, additive, 1.9f, -1, 0f, 0.85f);
            Layer(root, "Veins A", VeinsPath, additive, 0.98f, 1, 38f, 0.8f);
            Layer(root, "Veins B", VeinsPath, additive, 0.82f, 2, -57f, 0.65f);
            SetArcs(root);
            SetTether(root);

            PrefabUtility.SaveAsPrefabAsset(root, TargetPath);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Плазменный шар: собран");
        }

        private static void Strip<TComponent>(GameObject root) where TComponent : Component
        {
            TComponent found = root.GetComponent<TComponent>();

            if (found != null)
                Object.DestroyImmediate(found, true);
        }

        private static void SetCore(GameObject root)
        {
            SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CorePath);

            if (renderer == null || sprite == null)
            {
                Debug.LogWarning("Плазменный шар: не найден спрайт ядра");

                return;
            }

            renderer.sprite = sprite;
            renderer.color = Color.white;
            renderer.sortingOrder = 0;
        }

        private static void SetCollider(GameObject root)
        {
            CapsuleCollider2D collider = root.GetComponent<CapsuleCollider2D>();

            if (collider == null)
                return;

            collider.direction = CapsuleDirection2D.Horizontal;
            collider.size = new Vector2(1.2f, 1.2f);
            collider.offset = Vector2.zero;
        }

        private static void Layer(GameObject root, string name, string spritePath, Material material,
            float scale, int order, float spin, float alpha)
        {
            Transform found = root.transform.Find(name);

            if (found == null)
            {
                found = new GameObject(name).transform;
                found.SetParent(root.transform, false);
            }

            found.localPosition = Vector3.zero;
            found.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer renderer = found.GetComponent<SpriteRenderer>();

            if (renderer == null)
                renderer = found.gameObject.AddComponent<SpriteRenderer>();

            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            renderer.sharedMaterial = material;
            renderer.sortingOrder = order;
            renderer.color = new Color(1f, 1f, 1f, alpha);

            if (Mathf.Approximately(spin, 0f) == false)
                Spin(found.gameObject, spin);
            else
                Flicker(found.gameObject);
        }

        private static void Spin(GameObject target, float speed)
        {
            PlasmaSwirl swirl = target.GetComponent<PlasmaSwirl>();

            if (swirl == null)
                swirl = target.AddComponent<PlasmaSwirl>();

            SerializedObject serialized = new SerializedObject(swirl);

            serialized.FindProperty("_speed").floatValue = speed;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Flicker(GameObject target)
        {
            if (target.GetComponent<PlasmaFlicker>() == null)
                target.AddComponent<PlasmaFlicker>();
        }

        private static void SetArcs(GameObject root)
        {
            Transform found = root.transform.Find("Arcs");

            if (found == null)
            {
                found = new GameObject("Arcs").transform;
                found.SetParent(root.transform, false);
            }

            found.localPosition = Vector3.zero;
            found.localScale = Vector3.one;

            PlasmaArcs arcs = found.GetComponent<PlasmaArcs>();

            if (arcs == null)
                arcs = found.gameObject.AddComponent<PlasmaArcs>();

            SerializedObject serialized = new SerializedObject(arcs);

            serialized.FindProperty("_material").objectReferenceValue = GetArcMaterial();
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetTether(GameObject root)
        {
            Transform found = root.transform.Find("Tether");

            if (found == null)
            {
                found = new GameObject("Tether").transform;
                found.SetParent(root.transform, false);
            }

            found.localPosition = Vector3.zero;
            found.localScale = Vector3.one;

            PlasmaTether tether = found.GetComponent<PlasmaTether>();

            if (tether == null)
                tether = found.gameObject.AddComponent<PlasmaTether>();

            SerializedObject serialized = new SerializedObject(tether);

            serialized.FindProperty("_material").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Material>(ArcMaterialPath);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Material GetArcMaterial()
        {
            if (File.Exists(ArcMaterialPath))
                AssetDatabase.DeleteAsset(ArcMaterialPath);

            AssetDatabase.CopyAsset(SamplePath, ArcMaterialPath);

            Material created = AssetDatabase.LoadAssetAtPath<Material>(ArcMaterialPath);

            created.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            created.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            created.SetFloat("_ZWrite", 0f);
            created.SetTexture("_MainTex", null);

            if (created.HasProperty("_Color"))
                created.SetColor("_Color", new Color(0.72f, 0.9f, 1f, 1f));

            EditorUtility.SetDirty(created);

            return created;
        }

        private static Material GetAdditive()
        {
            if (File.Exists(AdditivePath))
                AssetDatabase.DeleteAsset(AdditivePath);

            AssetDatabase.CopyAsset(SamplePath, AdditivePath);

            Material created = AssetDatabase.LoadAssetAtPath<Material>(AdditivePath);

            created.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            created.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            created.SetFloat("_ZWrite", 0f);

            if (created.HasProperty("_Color"))
                created.SetColor("_Color", Color.white);

            EditorUtility.SetDirty(created);

            return created;
        }
    }
}
