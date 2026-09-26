using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

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
        private const string ShotSoundPath = "Assets/Sounds/Plasma shot [565450].wav";
        private const float ShotVolume = 0.3f;
        private const float AdditiveMode = 4f;
        private const string ArcSoundPath = "Assets/Sounds/Electric arc [567269].wav";
        private const string MixerPath = "Assets/Sounds/AudioMixer.mixer";
        private const string EffectsGroup = "Effects";
        private const float ArcVolume = 0.3f;

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

            SetShotSound(root);

            Layer(root, "Halo", HaloPath, additive, 1.9f, -1, 0f, 0.85f);
            Layer(root, "Veins A", VeinsPath, additive, 0.98f, 1, 38f, 0.8f);
            Layer(root, "Veins B", VeinsPath, additive, 0.82f, 2, -57f, 0.65f);
            SetArcs(root);
            SetTether(root);
            SetArcSound(root);

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

        private static void SetShotSound(GameObject root)
        {
            Ammo ammo = root.GetComponent<Ammo>();

            if (ammo == null)
                return;

            SerializedObject serialized = new SerializedObject(ammo);

            serialized.FindProperty("_shotSound._clip").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<AudioClip>(ShotSoundPath);
            serialized.FindProperty("_shotSound._volume").floatValue = ShotVolume;
            serialized.ApplyModifiedPropertiesWithoutUndo();
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

        private static void SetArcSound(GameObject root)
        {
            Transform found = root.transform.Find("Arc Sound");

            if (found == null)
            {
                found = new GameObject("Arc Sound").transform;
                found.SetParent(root.transform, false);
            }

            AudioSource source = found.GetComponent<AudioSource>();

            if (source == null)
                source = found.gameObject.AddComponent<AudioSource>();

            source.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ArcSoundPath);
            source.outputAudioMixerGroup = FindEffectsGroup();
            source.volume = ArcVolume;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;

            if (found.GetComponent<OrbArcSound>() == null)
                found.gameObject.AddComponent<OrbArcSound>();
        }

        private static AudioMixerGroup FindEffectsGroup()
        {
            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);

            if (mixer == null)
                return null;

            AudioMixerGroup[] groups = mixer.FindMatchingGroups(EffectsGroup);

            return groups.Length == 0 ? null : groups[0];
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
            Material material = LoadOrCopy(ArcMaterialPath);

            material.SetTexture("_MainTex", null);

            if (material.HasProperty("_Color"))
                material.SetColor("_Color", new Color(0.72f, 0.9f, 1f, 1f));

            MakeAdditive(material);

            return material;
        }

        private static Material GetAdditive()
        {
            Material material = LoadOrCopy(AdditivePath);

            if (material.HasProperty("_Color"))
                material.SetColor("_Color", Color.white);

            MakeAdditive(material);

            return material;
        }

        private static Material LoadOrCopy(string path)
        {
            if (File.Exists(path) == false)
                AssetDatabase.CopyAsset(SamplePath, path);

            return AssetDatabase.LoadAssetAtPath<Material>(path);
        }

        private static void MakeAdditive(Material material)
        {
            material.SetFloat("_Mode", AdditiveMode);
            material.SetFloat("_BlendOp", (float)BlendOp.Add);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.One);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.DisableKeyword("_ALPHAMODULATE_ON");
            material.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
        }
    }
}
