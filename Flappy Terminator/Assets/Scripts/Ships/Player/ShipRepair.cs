using System;
using UnityEngine;

public class ShipRepair : MonoBehaviour
{
    private const int Directions = 8;
    private const float FullTurn = Mathf.PI * 2f;

    [SerializeField] private Health _health;
    [SerializeField] private SpriteRenderer _body;
    [SerializeField] private Material _outlineMaterial;
    [SerializeField] private float _duration = 2f;
    [SerializeField] private float _thicknessPixels = 1.5f;
    [SerializeField] private Color _color = new Color(0.3f, 1f, 0.45f, 1f);
    [SerializeField] private float _flickerRate = 6f;
    [SerializeField] private float _minAlpha = 0.3f;
    [SerializeField] private float _maxAlpha = 1f;
    [SerializeField] private Material _armorMaterial;
    [SerializeField] private float _armorFadeSpeed = 4f;

    private readonly SpriteRenderer[] _outline = new SpriteRenderer[Directions];

    private SpriteRenderer _armor;
    private float _armorAlpha;

    private int _amount;
    private float _passiveRate;
    private float _passiveOwed;
    private int _healed;
    private float _elapsed;

    public bool IsRepairing { get; private set; }

    public bool CanRepair => _health.Value > 0 && _health.Value < _health.MaxValue;

    public bool IsRegenerating => _passiveRate > 0f && CanRepair;

    public event Action Finished;

    private void Awake()
    {
        for (int i = 0; i < Directions; i++)
            _outline[i] = CreateOutline(i);

        if (_armorMaterial != null)
            _armor = CreateArmor();

        Show(false);
    }

    private void Update()
    {
        Regenerate(Time.deltaTime);

        if (IsRepairing == false)
            return;

        if (_health.Value == 0)
        {
            Stop();

            return;
        }

        _elapsed += Time.deltaTime;

        int due = Mathf.FloorToInt(_amount * Mathf.Clamp01(_elapsed / _duration));

        if (due > _healed)
        {
            _health.Heal(due - _healed);
            _healed = due;
        }

        if (_elapsed < _duration)
            return;

        Stop();
        Finished?.Invoke();
    }

    private void LateUpdate()
    {
        if (IsRepairing)
            Follow();

        ShowArmor(Time.deltaTime);
    }

    public void Begin(int amount)
    {
        _amount = Mathf.Max(0, amount);
        _healed = 0;
        _elapsed = 0f;
        IsRepairing = true;
        Follow();
        Show(true);
    }

    public void SetPassiveRate(float healthPerSecond)
    {
        _passiveRate = Mathf.Max(0f, healthPerSecond);
        _passiveOwed = 0f;
    }

    public void Stop()
    {
        IsRepairing = false;
        Show(false);
    }

    private SpriteRenderer CreateArmor()
    {
        GameObject copy = new GameObject("Nanobot Armor");
        copy.transform.SetParent(_body.transform, false);
        copy.layer = _body.gameObject.layer;

        SpriteRenderer armor = copy.AddComponent<SpriteRenderer>();
        armor.sharedMaterial = _armorMaterial;
        armor.sortingLayerID = _body.sortingLayerID;
        armor.sortingOrder = _body.sortingOrder + 1;
        armor.enabled = false;

        return armor;
    }

    private void ShowArmor(float deltaTime)
    {
        if (_armor == null)
            return;

        float target = IsRegenerating && _body.enabled ? 1f : 0f;

        _armorAlpha = Mathf.MoveTowards(_armorAlpha, target, _armorFadeSpeed * deltaTime);
        _armor.enabled = _armorAlpha > 0f;

        if (_armor.enabled == false)
            return;

        _armor.sprite = _body.sprite;
        _armor.flipX = _body.flipX;
        _armor.flipY = _body.flipY;
        _armor.color = new Color(1f, 1f, 1f, _armorAlpha);
    }

    private void Regenerate(float deltaTime)
    {
        if (IsRegenerating == false)
        {
            _passiveOwed = 0f;

            return;
        }

        _passiveOwed += _passiveRate * deltaTime;

        int heal = Mathf.FloorToInt(_passiveOwed);

        if (heal <= 0)
            return;

        _passiveOwed -= heal;
        _health.Heal(heal);
    }

    private SpriteRenderer CreateOutline(int index)
    {
        GameObject copy = new GameObject("Repair Outline " + index);
        copy.transform.SetParent(_body.transform, false);
        copy.layer = _body.gameObject.layer;

        SpriteRenderer outline = copy.AddComponent<SpriteRenderer>();
        outline.sharedMaterial = _outlineMaterial;
        outline.sortingLayerID = _body.sortingLayerID;
        outline.sortingOrder = _body.sortingOrder - 1;

        return outline;
    }

    private void Follow()
    {
        float alpha = Mathf.Lerp(_minAlpha, _maxAlpha, 0.5f + 0.5f * Mathf.Sin(_elapsed * _flickerRate * FullTurn));
        Color color = new Color(_color.r, _color.g, _color.b, _color.a * alpha);
        float pixelsPerUnit = _body.sprite == null ? 1f : _body.sprite.pixelsPerUnit;
        float offset = _thicknessPixels / pixelsPerUnit;

        for (int i = 0; i < Directions; i++)
        {
            float angle = i * FullTurn / Directions;
            SpriteRenderer outline = _outline[i];

            outline.sprite = _body.sprite;
            outline.flipX = _body.flipX;
            outline.flipY = _body.flipY;
            outline.color = color;
            outline.enabled = _body.enabled;
            outline.transform.localPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * offset;
        }
    }

    private void Show(bool isShown)
    {
        foreach (SpriteRenderer outline in _outline)
        {
            if (outline != null)
                outline.enabled = isShown && _body.enabled;
        }
    }
}
