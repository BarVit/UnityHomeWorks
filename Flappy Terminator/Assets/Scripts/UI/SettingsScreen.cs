using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsScreen : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private VolumeSettings _volume;
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _effectsSlider;
    [SerializeField] private Button _backButton;

    public event Action CloseRequested;

    private void OnEnable()
    {
        _masterSlider.onValueChanged.AddListener(_volume.SetMaster);
        _musicSlider.onValueChanged.AddListener(_volume.SetMusic);
        _effectsSlider.onValueChanged.AddListener(_volume.SetEffects);
        _backButton.onClick.AddListener(OnBackClicked);
    }

    private void OnDisable()
    {
        _masterSlider.onValueChanged.RemoveListener(_volume.SetMaster);
        _musicSlider.onValueChanged.RemoveListener(_volume.SetMusic);
        _effectsSlider.onValueChanged.RemoveListener(_volume.SetEffects);
        _backButton.onClick.RemoveListener(OnBackClicked);
    }

    public void Show()
    {
        _masterSlider.SetValueWithoutNotify(_volume.Master);
        _musicSlider.SetValueWithoutNotify(_volume.Music);
        _effectsSlider.SetValueWithoutNotify(_volume.Effects);
        _panel.SetActive(true);
    }

    public void Hide()
    {
        _panel.SetActive(false);
    }

    private void OnBackClicked()
    {
        CloseRequested?.Invoke();
    }
}
