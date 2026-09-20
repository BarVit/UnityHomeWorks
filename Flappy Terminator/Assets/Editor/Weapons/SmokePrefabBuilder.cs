using System.IO;
using UnityEditor;
using UnityEngine;

namespace WeaponContent
{
    public static class SmokePrefabBuilder
    {
        private const string TexturePath = "Assets/Sprites/VFX/Smoke.png";
        private const string MaterialPath = "Assets/VFX/Smoke.mat";
        private const string SampleMaterialPath = "Assets/GabrielAguiarProductions/Flare00_AB_2.mat";
        private const string PrefabPath = "Assets/Prefabs/Overheat Smoke.prefab";
        private const string MountPath = "Player Ship/ShootPoint/Mount 1";
        private const string SmokeName = "Overheat Smoke";

        [MenuItem("Tools/Weapons/Build Overheat Smoke")]
        public static void Build()
        {
            GameObject root = new GameObject(SmokeName);
            ParticleSystem smoke = root.AddComponent<ParticleSystem>();

            Shape(smoke);
            Paint(root.GetComponent<ParticleSystemRenderer>());

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Перегрев: дым собран");
        }

        private static void Shape(ParticleSystem smoke)
        {
            smoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = smoke.main;

            main.playOnAwake = false;
            main.duration = 1f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.26f);
            main.startColor = new Color(1f, 1f, 1f, 0.55f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 60;

            ParticleSystem.EmissionModule emission = smoke.emission;

            emission.rateOverTime = 16f;

            ParticleSystem.ShapeModule shape = smoke.shape;

            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.05f;

            ParticleSystem.VelocityOverLifetimeModule drift = smoke.velocityOverLifetime;

            drift.enabled = true;
            drift.space = ParticleSystemSimulationSpace.World;
            drift.x = new ParticleSystem.MinMaxCurve(-0.45f, -0.15f);
            drift.y = new ParticleSystem.MinMaxCurve(0.25f, 0.7f);
            drift.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            ParticleSystem.SizeOverLifetimeModule grow = smoke.sizeOverLifetime;

            grow.enabled = true;
            grow.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.6f, 1f, 1.6f));

            ParticleSystem.ColorOverLifetimeModule fade = smoke.colorOverLifetime;

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
                    new GradientColorKey(new Color(0.85f, 0.88f, 0.92f), 1f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.25f),
                    new GradientAlphaKey(0f, 1f)
                });

            return gradient;
        }

        private static void Paint(ParticleSystemRenderer renderer)
        {
            renderer.sharedMaterial = GetMaterial();
            renderer.sortingOrder = 6;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        private static Material GetMaterial()
        {
            if (File.Exists(MaterialPath))
                AssetDatabase.DeleteAsset(MaterialPath);

            if (AssetDatabase.CopyAsset(SampleMaterialPath, MaterialPath) == false)
            {
                Debug.LogError("Перегрев: не удалось скопировать материал " + SampleMaterialPath);

                return null;
            }

            Material created = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);

            created.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath));

            if (created.HasProperty("_Color"))
                created.SetColor("_Color", new Color(0.85f, 0.87f, 0.9f, 1f));

            if (created.HasProperty("_TintColor"))
                created.SetColor("_TintColor", new Color(0.85f, 0.87f, 0.9f, 0.5f));

            EditorUtility.SetDirty(created);

            return created;
        }

        [MenuItem("Tools/Weapons/Attach Overheat Smoke")]
        public static void Attach()
        {
            if (File.Exists(PrefabPath) == false)
                Build();

            GameObject ship = GameObject.Find("Player Ship");
            GameObject mount = GameObject.Find(MountPath);

            if (ship == null || mount == null)
            {
                Debug.LogError("Перегрев: не найден корабль или точка стрельбы");

                return;
            }

            Transform existing = mount.transform.Find(SmokeName);

            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), mount.transform);

            instance.transform.localPosition = Vector3.zero;

            OverheatSmoke control = ship.GetComponent<OverheatSmoke>();

            if (control == null)
                control = ship.AddComponent<OverheatSmoke>();

            SerializedObject serialized = new SerializedObject(control);

            serialized.FindProperty("_shooter").objectReferenceValue = ship.GetComponent<PlayerShooter>();
            serialized.FindProperty("_smoke").objectReferenceValue = instance.GetComponent<ParticleSystem>();
            serialized.ApplyModifiedPropertiesWithoutUndo();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(ship.scene);
            Debug.Log("Перегрев: дым подключён к точке стрельбы");
        }
    }
}
