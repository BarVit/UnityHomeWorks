using UnityEditor;
using UnityEngine;

namespace ShipContent
{
    public static class ShieldRig
    {
        public const string ShieldName = "Energy Shield";

        private const string BubbleSprite = "Assets/Sprites/VFX/Energy Shield Bubble.png";
        private const float ShieldSizeShare = 1.16f;
        private const float BubbleRingShare = 116f / 128f;

        public static EnergyShield Attach(GameObject root, SpriteRenderer body, AmmoHitHandler bodyHits,
            out AmmoHitHandler shieldHits, float sizeScale = 1f)
        {
            Transform old = root.transform.Find(ShieldName);

            if (old != null)
                Object.DestroyImmediate(old.gameObject);

            Sprite bubbleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BubbleSprite);

            GameObject holder = new GameObject(ShieldName);
            holder.transform.SetParent(root.transform, false);
            holder.transform.localPosition = root.transform.InverseTransformPoint(body.bounds.center);

            GameObject bubbleObject = new GameObject("Bubble");
            bubbleObject.transform.SetParent(holder.transform, false);
            SpriteRenderer bubble = bubbleObject.AddComponent<SpriteRenderer>();
            bubble.sprite = bubbleSprite;
            bubble.sortingLayerID = body.sortingLayerID;
            bubble.sortingOrder = body.sortingOrder + 1;

            float size = Mathf.Max(body.bounds.size.x, body.bounds.size.y) * ShieldSizeShare * sizeScale;
            float scale = size / (bubbleSprite.bounds.size.x * root.transform.lossyScale.x);
            bubbleObject.transform.localScale = Vector3.one * scale;

            holder.layer = body.gameObject.layer;
            CircleCollider2D circle = holder.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            circle.radius = bubbleSprite.bounds.extents.x * BubbleRingShare * scale;
            circle.enabled = false;

            shieldHits = holder.AddComponent<AmmoHitHandler>();
            SerializedObject hitsSo = new SerializedObject(shieldHits);
            hitsSo.FindProperty("_ammoLayers").intValue = new SerializedObject(bodyHits).FindProperty("_ammoLayers").intValue;
            hitsSo.ApplyModifiedPropertiesWithoutUndo();

            EnergyShield shield = holder.AddComponent<EnergyShield>();
            SerializedObject shieldSo = new SerializedObject(shield);
            shieldSo.FindProperty("_bubble").objectReferenceValue = bubble;
            shieldSo.FindProperty("_collider").objectReferenceValue = circle;
            shieldSo.ApplyModifiedPropertiesWithoutUndo();

            return shield;
        }
    }
}
