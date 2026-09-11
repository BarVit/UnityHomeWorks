using UnityEngine;

public class ShockwaveEffect : MonoBehaviour
{
    public Material shockwaveMaterial;
    public float duration = 1.5f;
    public float maxScale = 15f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve distortionCurve = AnimationCurve.EaseInOut(0, 0.15f, 1, 0);

    private float startTime;
    private Vector3 initialScale;
    private MeshRenderer meshRenderer;
    private Material instanceMaterial;

    void Start()
    {
        startTime = Time.time;
        initialScale = transform.localScale;
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null && shockwaveMaterial != null)
        {
            // Создаем копию материала, чтобы не влиять на оригинал
            instanceMaterial = new Material(shockwaveMaterial);
            meshRenderer.material = instanceMaterial;
        }
    }

    void Update()
    {
        float elapsed = Time.time - startTime;
        float progress = elapsed / duration;

        if (progress >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // Увеличиваем размер волны
        float scale = scaleCurve.Evaluate(progress) * maxScale;
        transform.localScale = initialScale * scale;

        // Меняем силу искажения со временем (сначала сильнее, потом слабее)
        if (instanceMaterial != null)
        {
            float distortionStrength = distortionCurve.Evaluate(progress);
            instanceMaterial.SetFloat("_DistortionStrength", distortionStrength);
        }
    }

    void OnDestroy()
    {
        // Очищаем созданный материал
        if (instanceMaterial != null)
        {
            Destroy(instanceMaterial);
        }
    }
}