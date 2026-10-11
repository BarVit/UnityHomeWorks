using System;
using UnityEngine;

public class BattleModules : MonoBehaviour
{
    private const int NoSlot = -1;

    [SerializeField] private InputReader _input;
    [SerializeField] private PlayerShip _ship;
    [SerializeField] private EnergyShield _shield;

    private ModuleSpec?[] _specs = new ModuleSpec?[0];
    private float[] _cooldownLeft = new float[0];
    private int _shieldSlot = NoSlot;

    public int Count => _specs.Length;

    public event Action Changed;

    private void OnEnable()
    {
        _input.ModuleActivated += Activate;
        _shield.Broken += OnShieldBroken;
    }

    private void OnDisable()
    {
        _input.ModuleActivated -= Activate;
        _shield.Broken -= OnShieldBroken;
    }

    private void Update()
    {
        for (int i = 0; i < _cooldownLeft.Length; i++)
        {
            if (_cooldownLeft[i] > 0f)
                _cooldownLeft[i] = Mathf.Max(0f, _cooldownLeft[i] - Time.deltaTime);
        }
    }

    public void Load(ModuleSpec?[] specs)
    {
        _specs = specs;
        _cooldownLeft = new float[specs.Length];
        ResetForLevel();
        Changed?.Invoke();
    }

    public void ResetForLevel()
    {
        _shield.Drop();
        _shieldSlot = NoSlot;

        for (int i = 0; i < _cooldownLeft.Length; i++)
            _cooldownLeft[i] = 0f;
    }

    public ModuleSpec? GetSpec(int slot)
    {
        return slot >= 0 && slot < _specs.Length ? _specs[slot] : null;
    }

    public bool IsActive(int slot)
    {
        return slot == _shieldSlot && _shield.IsUp;
    }

    public float GetReadiness(int slot)
    {
        ModuleSpec? spec = GetSpec(slot);

        if (spec == null || spec.Value.Cooldown <= 0f)
            return 1f;

        return 1f - Mathf.Clamp01(_cooldownLeft[slot] / spec.Value.Cooldown);
    }

    public void Activate(int slot)
    {
        ModuleSpec? spec = GetSpec(slot);

        if (spec == null || _ship.IsControlEnabled == false || Time.timeScale == 0f)
            return;

        if (_cooldownLeft[slot] > 0f || IsActive(slot))
            return;

        switch (spec.Value.Kind)
        {
            case ModuleKind.Shield:
                _shieldSlot = slot;
                _shield.Raise(spec.Value.Capacity);
                break;
        }

        Changed?.Invoke();
    }

    private void OnShieldBroken()
    {
        if (_shieldSlot == NoSlot)
            return;

        ModuleSpec? spec = GetSpec(_shieldSlot);

        if (spec != null)
            _cooldownLeft[_shieldSlot] = spec.Value.Cooldown;

        _shieldSlot = NoSlot;
        Changed?.Invoke();
    }
}
