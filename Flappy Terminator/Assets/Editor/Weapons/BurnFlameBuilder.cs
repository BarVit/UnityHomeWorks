using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace WeaponContent
{
    public static class BurnFlameBuilder
    {
        private const string TexturePath = "Assets/Sprites/VFX/Smoke.png";
        private const string MaterialPath = "Assets/VFX/Burn Flame.mat";
        private const string SampleMaterialPath = "Assets/GabrielAguiarProductions/Flare00_AB_2.mat";
        private const string PrefabPath = "Assets/Prefabs/Burn Flame.prefab";
        private const string PlayerPath = "Assets/Prefabs/Player Ship.prefab";
        private const string FlameName = "Burn Flame";
        private const float AdditiveMode = 4f;

        [MenuItem("Tools/Weapons/Build Burn Flame")]
        public static void Build()
        {
            GameObject root = new GameObject(FlameName);
            ParticleSystem flame = root.AddComponent<ParticleSystem>();

            Shape(flame);
            Paint(root.GetComponent<ParticleSystemRenderer>());

            ParticleSystem prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath).GetComponent<ParticleSystem>();

            Object.DestroyImmediate(root);
            AttachToPlayer(prefab);
            AssetDatabase.SaveAssets();
            Debug.Log("Напалм: пламя собрано в " + PrefabPath);
        }

        private static void Shape(ParticleSystem flame)
        {
            flame.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = flame.main;

            main.playOnAwake = false;
            main.duration = 1f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.55f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.14f, 0.26f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.8f, 0.3f, 1f),
                new Color(1f, 0.5f, 0.12f, 1f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 30;

            ParticleSystem.EmissionModule emission = flame.emission;

            emission.rateOverTime = 16f;

            ParticleSystem.ShapeModule shape = flame.shape;

            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.25f;

            ParticleSystem.VelocityOverLifetimeModule rise = flame.velocityOverLifetime;

            rise.enabled = true;
            rise.space = ParticleSystemSimulationSpace.World;
            rise.x = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
            rise.y = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
            rise.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            ParticleSystem.SizeOverLifetimeModule shrink = flame.sizeOverLifetime;

            shrink.enabled = true;
            shrink.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.3f));

            ParticleSystem.ColorOverLifetimeModule fade = flame.colorOverLifetime;

            fade.enabled = true;
            fade.color = new ParticleSystem.MinMaxGradient(GetFade());
        }

        private static Gradient GetFade()
        {
            Gradient gradient = new Gradient();

            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(1f, 0.3f, 0.08f), 1f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.15f),
                    new GradientAlphaKey(0f, 1f)
                });

            return gradient;
        }

        private static void Paint(ParticleSystemRenderer renderer)
        {
            renderer.sharedMaterial = GetMaterial();
            renderer.sortingOrder = 7;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        private static Material GetMaterial()
        {
            if (File.Exists(MaterialPath))
                AssetDatabase.DeleteAsset(MaterialPath);

            if (AssetDatabase.CopyAsset(SampleMaterialPath, MaterialPath) == false)
            {
                Debug.LogError("Напалм: не удалось скопировать материал " + SampleMaterialPath);

                return null;
            }

            Material created = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);

            created.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath));

            if (created.HasProperty("_Color"))
                created.SetColor("_Color", Color.white);

            if (created.HasProperty("_TintColor"))
                created.SetColor("_TintColor", new Color(1f, 1f, 1f, 0.5f));

            MakeAdditive(created);

            EditorUtility.SetDirty(created);

            return created;
        }

        private static void MakeAdditive(Material material)
        {
            material.SetFloat("_Mode", AdditiveMode);
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.One);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.DisableKeyword("_ALPHAMODULATE_ON");
            material.renderQueue = (int)RenderQueue.Transparent;
        }

        private static void AttachToPlayer(ParticleSystem prefab)
        {
            GameObject player = PrefabUtility.LoadPrefabContents(PlayerPath);

            try
            {
                SerializedObject so = new SerializedObject(player.GetComponent<PlayerShooter>());
                so.FindProperty("_burnFlame").objectReferenceValue = prefab;
                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(player, PlayerPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(player);
            }
        }
    }
}
