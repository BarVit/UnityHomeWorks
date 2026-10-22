using UnityEditor;
using UnityEngine;

namespace ShipContent
{
    public static class SentryRig
    {
        public const string SentryName = "Sentry Drones";

        private const string DroneSheet = "Assets/Sprites/Ships/Sentry Drone.png";
        private const string DroneFrame = "tinyShip1_3";
        private const string TracerMaterial = "Assets/Prefabs/Beams/Beam.mat";
        private const string SparksPrefab = "Assets/VFX/Sparks/Hit Sparks.prefab";
        private const float TracerWidth = 0.02f;
        private const int MaxSparks = 300;

        private static readonly string[] DroneNames = { "Drone Top", "Drone Bottom" };

        public static SentrySquad Attach(GameObject root, SpriteRenderer body, Health health, LayerMask targets)
        {
            Transform old = root.transform.Find(SentryName);

            if (old != null)
                Object.DestroyImmediate(old.gameObject);

            GameObject holder = new GameObject(SentryName);
            holder.transform.SetParent(root.transform, false);

            SpriteRenderer[] drones = new SpriteRenderer[DroneNames.Length];

            for (int i = 0; i < drones.Length; i++)
                drones[i] = CreateDrone(holder.transform, DroneNames[i], body);

            SentrySquad squad = holder.AddComponent<SentrySquad>();
            SerializedObject so = new SerializedObject(squad);
            so.FindProperty("_ship").objectReferenceValue = root.transform;
            so.FindProperty("_body").objectReferenceValue = body;
            so.FindProperty("_health").objectReferenceValue = health;
            so.FindProperty("_tracerTemplate").objectReferenceValue = CreateTracer(holder.transform, body);
            so.FindProperty("_sparks").objectReferenceValue = CreateSparks(holder.transform, body);
            so.FindProperty("_targetLayers").intValue = targets.value;

            SerializedProperty list = so.FindProperty("_drones");
            list.arraySize = drones.Length;

            for (int i = 0; i < drones.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = drones[i];

            so.ApplyModifiedPropertiesWithoutUndo();

            return squad;
        }

        private static SpriteRenderer CreateDrone(Transform parent, string name, SpriteRenderer body)
        {
            GameObject drone = new GameObject(name);
            drone.transform.SetParent(parent, false);
            drone.layer = body.gameObject.layer;

            SpriteRenderer sprite = drone.AddComponent<SpriteRenderer>();
            sprite.sprite = LoadFrame();
            sprite.sortingLayerID = body.sortingLayerID;
            sprite.sortingOrder = body.sortingOrder;
            sprite.enabled = false;

            return sprite;
        }

        private static Sprite LoadFrame()
        {
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(DroneSheet))
            {
                if (asset is Sprite sprite && sprite.name == DroneFrame)
                    return sprite;
            }

            throw new System.InvalidOperationException("SentryRig: нет кадра " + DroneFrame + " в " + DroneSheet);
        }

        private static SentryTracer CreateTracer(Transform parent, SpriteRenderer body)
        {
            GameObject tracer = new GameObject("Tracer");
            tracer.transform.SetParent(parent, false);

            LineRenderer line = tracer.AddComponent<LineRenderer>();
            line.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(TracerMaterial);
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.widthMultiplier = TracerWidth;
            line.numCapVertices = 0;
            line.sortingLayerID = body.sortingLayerID;
            line.sortingOrder = body.sortingOrder - 1;

            SentryTracer component = tracer.AddComponent<SentryTracer>();
            tracer.SetActive(false);

            return component;
        }

        private static ParticleSystem CreateSparks(Transform parent, SpriteRenderer body)
        {
            GameObject sparks = new GameObject("Sparks");
            sparks.transform.SetParent(parent, false);

            ParticleSystem system = sparks.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = system.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = MaxSparks;
            main.gravityModifier = 0f;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = false;

            ParticleSystem.ColorOverLifetimeModule fade = system.colorOverLifetime;
            fade.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;

            ParticleSystemRenderer renderer = sparks.GetComponent<ParticleSystemRenderer>();
            CopyLook(system, renderer);
            renderer.sortingLayerID = body.sortingLayerID;
            renderer.sortingOrder = body.sortingOrder + 1;

            return system;
        }

        private static void CopyLook(ParticleSystem system, ParticleSystemRenderer renderer)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SparksPrefab);
            ParticleSystem source = prefab == null ? null : prefab.GetComponentInChildren<ParticleSystem>(true);

            if (source == null)
                return;

            ParticleSystemRenderer sourceRenderer = source.GetComponent<ParticleSystemRenderer>();
            ParticleSystem.MainModule main = system.main;
            main.startColor = source.main.startColor;

            renderer.sharedMaterial = sourceRenderer.sharedMaterial;
            renderer.renderMode = sourceRenderer.renderMode;
            renderer.velocityScale = sourceRenderer.velocityScale;
            renderer.lengthScale = sourceRenderer.lengthScale;
        }
    }
}
