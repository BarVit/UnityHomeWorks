using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class BossAnnouncement : MonoBehaviour
{
    [SerializeField] private EnemySpawner _spawner;
    [SerializeField] private TMP_Text _label;
    [SerializeField] private float _fade = 0.25f;

    private CanvasGroup _group;
    private float _age;
    private float _duration;

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        _group.alpha = 0f;
    }

    private void OnEnable()
    {
        _spawner.BossAnnounced += Show;
    }

    private void OnDisable()
    {
        _spawner.BossAnnounced -= Show;
    }

    private void Update()
    {
        if (_age >= _duration)
        {
            _group.alpha = 0f;

            return;
        }

        _age += Time.deltaTime;
        _group.alpha = Mathf.Clamp01(Mathf.Min(_age, _duration - _age) / _fade);
    }

    private void Show(string text, float duration)
    {
        _label.text = text;
        _age = 0f;
        _duration = duration;
    }
}
