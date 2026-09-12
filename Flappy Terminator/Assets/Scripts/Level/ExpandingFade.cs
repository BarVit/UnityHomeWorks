using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ExpandingFade : MonoBehaviour
{
    private static readonly int StrengthProperty = Shader.PropertyToID("_Strength");

    [SerializeField] private float _duration = 0.7f;
    [SerializeField] private float _startScale = 0.5f;
    [SerializeField] private float _endScale = 14f;
    [SerializeField] private float _startStrength = 0.06f;
    [SerializeField] private AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;
    private Coroutine _playing;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _propertyBlock = new MaterialPropertyBlock();

        Hide();
    }

    public void Play(Vector3 position)
    {
        if (_playing != null)
            StopCoroutine(_playing);

        transform.position = position;
        _playing = StartCoroutine(Expand());
    }

    private IEnumerator Expand()
    {
        _renderer.enabled = true;

        for (float elapsed = 0f; elapsed < _duration; elapsed += Time.deltaTime)
        {
            float progress = elapsed / _duration;

            SetScale(Mathf.Lerp(_startScale, _endScale, _scaleCurve.Evaluate(progress)));
            SetStrength(Mathf.Lerp(_startStrength, 0f, progress));

            yield return null;
        }

        Hide();
        _playing = null;
    }

    private void Hide()
    {
        SetStrength(0f);
        _renderer.enabled = false;
    }

    private void SetScale(float scale)
    {
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void SetStrength(float strength)
    {
        _renderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetFloat(StrengthProperty, strength);
        _renderer.SetPropertyBlock(_propertyBlock);
    }
}
