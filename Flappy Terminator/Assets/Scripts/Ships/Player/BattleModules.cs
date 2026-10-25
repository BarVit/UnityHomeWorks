using System;
using System.Globalization;
using UnityEngine;

public class BattleModules : MonoBehaviour
{
    private const int NoSlot = -1;

    [SerializeField] private InputReader _input;
    [SerializeField] private PlayerShip _ship;
    [SerializeField] private EnergyShield _shield;
    [SerializeField] private ShipRepair _repair;
    [SerializeField] private SentrySquad _sentries;
    [SerializeField] private EmpField _emp;
    [SerializeField] private EnemySpawner _spawner;

    private ModuleSpec?[] _specs = new ModuleSpec?[0];
    private float[] _cooldownLeft = new float[0];
    private int _shieldSlot = NoSlot;
    private int _repairSlot = NoSlot;
    private int _sentrySlot = NoSlot;
    private int _empSlot = NoSlot;

    public int Count => _specs.Length;

    public event Action Changed;

    private void OnEnable()
    {
        _input.ModuleActivated += Activate;
        _shield.Broken += OnShieldBroken;
        _repair.Finished += OnRepairFinished;
        _sentries.Finished += OnSentriesFinished;
        _emp.Finished += OnEmpFinished;
    }

    private void OnDisable()
    {
        _input.ModuleActivated -= Activate;
        _shield.Broken -= OnShieldBroken;
        _repair.Finished -= OnRepairFinished;
        _sentries.Finished -= OnSentriesFinished;
        _emp.Finished -= OnEmpFinished;
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
        _repair.SetPassiveRate(GetRegen(specs));
        ResetForLevel();
        Changed?.Invoke();
    }

    public void ResetForLevel()
    {
        _shield.Drop();
        _shieldSlot = NoSlot;
        _repair.Stop();
        _repairSlot = NoSlot;
        _sentries.Stop();
        _sentrySlot = NoSlot;
        _emp.Stop();
        _spawner.Pause(false);
        _empSlot = NoSlot;

        for (int i = 0; i < _cooldownLeft.Length; i++)
            _cooldownLeft[i] = 0f;
    }

    public ModuleSpec? GetSpec(int slot)
    {
        return slot >= 0 && slot < _specs.Length ? _specs[slot] : null;
    }

    public bool IsActive(int slot)
    {
        if (slot == _repairSlot)
            return _repair.IsRepairing;

        if (slot == _sentrySlot)
            return _sentries.IsActive;

        if (slot == _empSlot)
            return _emp.IsActive;

        if (GetSpec(slot)?.Kind == ModuleKind.RepairNanobots)
            return _repair.IsRegenerating;

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
        {
            if (spec.Value.Kind == ModuleKind.Emp)
                DamageLog.Event("emp_refused", note: IsActive(slot) ? "active"
                    : "cooldown " + _cooldownLeft[slot].ToString("0.0", CultureInfo.InvariantCulture));

            return;
        }

        switch (spec.Value.Kind)
        {
            case ModuleKind.Shield:
                _shieldSlot = slot;
                _shield.Raise(spec.Value.Capacity, spec.Value.Level);
                break;
            case ModuleKind.RepairDrone:
                if (_repair.CanRepair == false)
                    return;

                _repairSlot = slot;
                _repair.Begin(spec.Value.Repair);
                break;
            case ModuleKind.SentryDrones:
                _sentrySlot = slot;
                _sentries.Launch(spec.Value.Sentry);
                break;
            case ModuleKind.Emp:
                _empSlot = slot;
                _spawner.Pause(true);
                _emp.Launch(spec.Value.Duration);
                break;
        }

        Changed?.Invoke();
    }

    private void OnRepairFinished()
    {
        if (_repairSlot == NoSlot)
            return;

        ModuleSpec? spec = GetSpec(_repairSlot);

        if (spec != null)
            _cooldownLeft[_repairSlot] = spec.Value.Cooldown;

        _repairSlot = NoSlot;
        Changed?.Invoke();
    }

    private static int GetRegen(ModuleSpec?[] specs)
    {
        int regen = 0;

        foreach (ModuleSpec? spec in specs)
        {
            if (spec != null && spec.Value.Kind == ModuleKind.RepairNanobots)
                regen += spec.Value.Regen;
        }

        return regen;
    }

    private void OnEmpFinished()
    {
        _spawner.Pause(false);

        if (_empSlot == NoSlot)
            return;

        ModuleSpec? spec = GetSpec(_empSlot);

        if (spec != null)
            _cooldownLeft[_empSlot] = spec.Value.Cooldown;

        _empSlot = NoSlot;
        Changed?.Invoke();
    }

    private void OnSentriesFinished()
    {
        if (_sentrySlot == NoSlot)
            return;

        ModuleSpec? spec = GetSpec(_sentrySlot);

        if (spec != null)
            _cooldownLeft[_sentrySlot] = spec.Value.Cooldown;

        _sentrySlot = NoSlot;
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
