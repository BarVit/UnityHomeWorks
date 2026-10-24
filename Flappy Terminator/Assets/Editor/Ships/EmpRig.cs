using UnityEditor;
using UnityEngine;

namespace ShipContent
{
    public static class EmpRig
    {
        public const string EmpName = "EMP Field";

        private const string DomeSprite = "Assets/Sprites/UI/Dock/White Square.png";
        private const string DomeShader = "FlappyTerminator/EmpDome";
        private const string DomeMaterialPath = "Assets/VFX/Repair/Emp Dome.mat";
        private const string BoltMaterial = "Assets/Prefabs/Beams/Beam.mat";
        private const string PlasmaHitPrefab = "Assets/Prefabs/PlayerBulletHit.prefab";
        private const string PopPrefab = "Assets/Prefabs/Emp Pop.prefab";
        private const string EnemyAmmoLayer = "EnemyAmmo";
        private const float PopScale = 3.4f;
        private const float BoltWidth = 0.07f;
        private const float GlowWidth = 0.28f;
        private const int OverEnemies = 3;

        private static readonly Color PopTint = new Color(0.6f, 0.85f, 1f, 1f);

        public static EmpField Attach(GameObject root, SpriteRenderer body, LayerMask enemies)
        {
            Transform old = root.transform.Find(EmpName);

            if (old != null)
                Object.DestroyImmediate(old.gameObject);

            GameObject holder = new GameObject(EmpName);
            holder.transform.SetParent(root.transform, false);

            EmpField field = holder.AddComponent<EmpField>();
            SerializedObject so = new SerializedObject(field);
            so.FindProperty("_ship").objectReferenceValue = root.transform;
            so.FindProperty("_dome").objectReferenceValue = CreateDome(holder.transform, body);
            so.FindProperty("_boltTemplate").objectReferenceValue = CreateBolt(holder.transform, body);
            so.FindProperty("_popPrefab").objectReferenceValue = LoadPop();
            so.FindProperty("_enemyLayers").intValue = enemies.value;
            so.FindProperty("_enemyAmmoLayers").intValue = LayerMask.GetMask(EnemyAmmoLayer);
            so.ApplyModifiedPropertiesWithoutUndo();

            return field;
        }

        private static SpriteRenderer CreateDome(Transform parent, SpriteRenderer body)
        {
            GameObject dome = new GameObject("Dome");
            dome.transform.SetParent(parent, false);

            SpriteRenderer sprite = dome.AddComponent<SpriteRenderer>();
            sprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DomeSprite);
            sprite.sharedMaterial = LoadDomeMaterial();
            sprite.sortingLayerID = body.sortingLayerID;
            sprite.sortingOrder = body.sortingOrder + OverEnemies;
            sprite.enabled = false;

            return sprite;
        }

        private static EmpBolt CreateBolt(Transform parent, SpriteRenderer body)
        {
            GameObject bolt = new GameObject("Bolt");
            bolt.transform.SetParent(parent, false);

            LineRenderer line = bolt.AddComponent<LineRenderer>();
            line.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(BoltMaterial);
            line.useWorldSpace = true;
            line.widthMultiplier = BoltWidth;
            line.sortingLayerID = body.sortingLayerID;
            line.sortingOrder = body.sortingOrder + OverEnemies + 2;

            GameObject glow = new GameObject("Glow");
            glow.transform.SetParent(bolt.transform, false);

            LineRenderer glowLine = glow.AddComponent<LineRenderer>();
            glowLine.sharedMaterial = line.sharedMaterial;
            glowLine.useWorldSpace = true;
            glowLine.widthMultiplier = GlowWidth;
            glowLine.sortingLayerID = line.sortingLayerID;
            glowLine.sortingOrder = line.sortingOrder - 1;

            EmpBolt component = bolt.AddComponent<EmpBolt>();
            SerializedObject so = new SerializedObject(component);
            so.FindProperty("_glow").objectReferenceValue = glowLine;
            so.ApplyModifiedPropertiesWithoutUndo();
            bolt.SetActive(false);

            return component;
        }

        private static Material LoadDomeMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(DomeMaterialPath);

            if (material != null)
                return material;

            material = new Material(Shader.Find(DomeShader)) { name = "Emp Dome" };
            AssetDatabase.CreateAsset(material, DomeMaterialPath);

            return material;
        }

        private static ExplosionAnimation LoadPop()
        {
            ExplosionAnimation existing = AssetDatabase.LoadAssetAtPath<ExplosionAnimation>(PopPrefab);

            if (existing != null)
                return existing;

            GameObject copy = PrefabUtility.LoadPrefabContents(PlasmaHitPrefab);

            try
            {
                copy.name = "Emp Pop";
                copy.transform.localScale = new Vector3(PopScale, PopScale, 1f);

                foreach (SpriteRenderer sprite in copy.GetComponentsInChildren<SpriteRenderer>(true))
                    sprite.color = PopTint;

                PrefabUtility.SaveAsPrefabAsset(copy, PopPrefab);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(copy);
            }

            return AssetDatabase.LoadAssetAtPath<ExplosionAnimation>(PopPrefab);
        }
    }
}
