using UnityEngine;

public class ModuleHud : MonoBehaviour
{
    [SerializeField] private BattleModules _modules;
    [SerializeField] private ModuleButtonView[] _buttons;

    private void OnEnable()
    {
        foreach (ModuleButtonView button in _buttons)
            button.Clicked += _modules.Activate;
    }

    private void OnDisable()
    {
        foreach (ModuleButtonView button in _buttons)
            button.Clicked -= _modules.Activate;
    }

    private void Update()
    {
        foreach (ModuleButtonView button in _buttons)
        {
            ModuleSpec? spec = _modules.GetSpec(button.Slot);

            if (spec == null)
                button.Hide();
            else
                button.Show(spec.Value.Icon, _modules.GetReadiness(button.Slot), _modules.IsActive(button.Slot));
        }
    }
}
