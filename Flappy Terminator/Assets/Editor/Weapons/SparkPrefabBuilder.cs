using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace WeaponContent
{
    public static class SparkPrefabBuilder
    {
        private const string SourcePath = "Assets/VFX/Sparks/vfx_Sparks_01.prefab";
        private const string WarmPath = "Assets/VFX/Sparks/Hit Sparks.prefab";
        private const string ColdPath = "Assets/VFX/Sparks/Hit Sparks Blue.prefab";
        private const string WhitePath = "Assets/VFX/Sparks/Hit Sparks White.prefab";
        private const string MuzzlePath = "Assets/VFX/Sparks/Muzzle Sparks.prefab";
        private const string CannonHitPath = "Assets/VFX/Sparks/Cannon Hit.prefab";
        private const string PiercePath = "Assets/VFX/Sparks/Pierce Sparks.prefab";
        private const string FlashMaterialPath = "Assets/VFX/Plasma Additive.mat";
        private const string BlendMaterialPath = "Assets/VFX/Plasma Blend.mat";
        private const float FadeMode = 2f;
        private const int FlashSortingOrder = 11;
        private const float FlashStretch = 0.06f;
        private const float SprayHalfAngle = 30f;
        private const float SprayShare = 0.5f;
        private const float SprayLifetimeShare = 0.5f;
        private static readonly Vector3 FlashOffset = new Vector3(0.3f, 0f, 0f);
        private static readonly Color Yellow = new Color(1f, 0.85f, 0.15f, 1f);
        private static readonly Color Cold = new Color(0.3f, 0.75f, 1f, 1f);
        private static readonly Burst[] NoGlows = new Burst[0];

        private static readonly Burst MuzzleCore = new Burst("Flash Core", 1, new Vector2(1f, 1.2f),
            new Vector2(0.12f, 0.16f), Vector2.zero, 0f, new Color(1f, 0.97f, 0.8f, 1f), new Color(1f, 0.88f, 0.5f, 1f));
        private static readonly Burst MuzzlePlume = new Burst("Flash Plume", 12, new Vector2(0.7f, 1.05f),
            new Vector2(0.16f, 0.24f), new Vector2(4f, 9f), 14f, new Color(1f, 0.78f, 0.32f, 1f), new Color(1f, 0.48f, 0.12f, 1f));
        private static readonly Burst CannonCircle = new Burst("Hit Circle", 1, new Vector2(0.45f, 0.55f),
            new Vector2(0.08f, 0.1f), Vector2.zero, 0f, new Color(1f, 0.92f, 0.35f, 1f), new Color(1f, 0.8f, 0.2f, 1f));
        private static readonly Burst PierceCore = new Burst("Flash Core", 1, new Vector2(0.5f, 0.6f),
            new Vector2(0.06f, 0.08f), Vector2.zero, 0f, new Color(0.85f, 0.97f, 1f, 1f), new Color(0.6f, 0.9f, 1f, 1f));
        private static readonly Burst PiercePlume = new Burst("Flash Plume", 10, new Vector2(0.45f, 0.65f),
            new Vector2(0.1f, 0.16f), new Vector2(6f, 12f), 10f, new Color(0.55f, 0.85f, 1f, 1f), new Color(0.3f, 0.6f, 1f, 1f));

        [MenuItem("Tools/Weapons/Build Hit Sparks")]
        public static void Build()
        {
            Make(WarmPath, "Hit Sparks", Color.clear, NoGlows);
            Make(ColdPath, "Hit Sparks Blue", Cold, NoGlows);
            Make(WhitePath, "Hit Sparks White", Color.white, NoGlows);
            Make(CannonHitPath, "Cannon Hit", Yellow, new[] { CannonCircle });
            MakeDirected(MuzzlePath, "Muzzle Sparks", Yellow, MuzzleCore, MuzzlePlume, FlashOffset);
            MakeDirected(PiercePath, "Pierce Sparks", Cold, PierceCore, PiercePlume, Vector3.zero);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Искры попадания: собраны");
        }

        private static void Make(string path, string name, Color tint, Burst[] glows)
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

            Material material = GetBlendMaterial();

            foreach (Burst glow in glows)
                AddBurst(root, glow, material, Vector3.zero);

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void MakeDirected(string path, string name, Color tint, Burst core, Burst plume,
            Vector3 flashOffset)
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
            spray.transform.localRotation = Quaternion.identity;

            Strip<BumpEffect>(spray);

            AudioSource audio = root.AddComponent<AudioSource>();

            audio.playOnAwake = false;

            ExplosionAnimation explosion = root.AddComponent<ExplosionAnimation>();
            SerializedObject serialized = new SerializedObject(explosion);

            serialized.FindProperty("_audioSource").objectReferenceValue = audio;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Paint(root, tint);
            Float(root);
            Aim(spray);
            AddFlash(root, core, plume, flashOffset);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void AddFlash(GameObject root, Burst core, Burst plume, Vector3 offset)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(FlashMaterialPath);

            AddBurst(root, core, material, offset);
            AddBurst(root, plume, material, offset);
        }

        private static void AddBurst(GameObject root, Burst burst, Material material, Vector3 offset)
        {
            GameObject holder = new GameObject(burst.Name);

            holder.transform.SetParent(root.transform, false);
            holder.transform.localPosition = offset;

            ParticleSystem system = holder.AddComponent<ParticleSystem>();

            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = system.main;

            main.duration = burst.Lifetime.y;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(burst.Lifetime.x, burst.Lifetime.y);
            main.startSpeed = new ParticleSystem.MinMaxCurve(burst.Speed.x, burst.Speed.y);
            main.startSize = new ParticleSystem.MinMaxCurve(burst.Size.x, burst.Size.y);
            main.startColor = new ParticleSystem.MinMaxGradient(burst.From, burst.To);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.gravityModifier = 0f;
            main.maxParticles = 16;

            ParticleSystem.EmissionModule emission = system.emission;

            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, burst.Count) });

            ParticleSystem.ShapeModule shape = system.shape;

            shape.enabled = burst.ConeAngle > 0f;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = burst.ConeAngle;
            shape.radius = 0.02f;
            shape.rotation = new Vector3(0f, 90f, 0f);

            ParticleSystem.SizeOverLifetimeModule size = system.sizeOverLifetime;

            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f,
                new AnimationCurve(new Keyframe(0f, 0.6f), new Keyframe(0.3f, 1f), new Keyframe(1f, 0.5f)));

            ParticleSystem.ColorOverLifetimeModule color = system.colorOverLifetime;
            Gradient fade = new Gradient();

            fade.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.4f), new GradientAlphaKey(0f, 1f) });
            color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(fade);

            ParticleSystemRenderer renderer = holder.GetComponent<ParticleSystemRenderer>();

            renderer.sharedMaterial = material;
            renderer.renderMode = burst.ConeAngle > 0f ? ParticleSystemRenderMode.Stretch : ParticleSystemRenderMode.Billboard;
            renderer.velocityScale = FlashStretch;
            renderer.lengthScale = 1f;
            renderer.sortingOrder = FlashSortingOrder;
        }

        private static void Aim(GameObject spray)
        {
            foreach (ParticleSystem system in spray.GetComponentsInChildren<ParticleSystem>(true))
            {
                ParticleSystem.ShapeModule shape = system.shape;

                shape.angle = SprayHalfAngle;
                shape.rotation = new Vector3(0f, 90f, 0f);

                ParticleSystem.MainModule main = system.main;

                main.startLifetimeMultiplier *= SprayLifetimeShare;

                ParticleSystem.EmissionModule emission = system.emission;
                ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[emission.burstCount];

                emission.rateOverTimeMultiplier *= SprayShare;
                emission.GetBursts(bursts);

                for (int i = 0; i < bursts.Length; i++)
                    bursts[i].count = Thin(bursts[i].count);

                emission.SetBursts(bursts);
            }
        }

        private static ParticleSystem.MinMaxCurve Thin(ParticleSystem.MinMaxCurve count)
        {
            if (count.mode == ParticleSystemCurveMode.TwoConstants)
                return new ParticleSystem.MinMaxCurve(count.constantMin * SprayShare, count.constantMax * SprayShare);

            return new ParticleSystem.MinMaxCurve(count.constant * SprayShare);
        }

        private static Material GetBlendMaterial()
        {
            if (File.Exists(BlendMaterialPath) == false)
                AssetDatabase.CopyAsset(FlashMaterialPath, BlendMaterialPath);

            Material material = AssetDatabase.LoadAssetAtPath<Material>(BlendMaterialPath);

            material.SetFloat("_Mode", FadeMode);
            material.SetFloat("_BlendOp", (float)BlendOp.Add);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material);

            return material;
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
