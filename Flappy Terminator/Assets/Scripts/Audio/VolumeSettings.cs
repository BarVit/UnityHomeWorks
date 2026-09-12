using UnityEngine;
using UnityEngine.Audio;

public class VolumeSettings : MonoBehaviour
{
    private const string MasterParameter = "Master Volume";
    private const string MusicParameter = "Music Volume";
    private const string EffectsParameter = "Effects Volume";

    private const float MinPercent = 0f;
    private const float MaxPercent = 100f;
    private const float SilencePercent = 0.01f;
    private const int DecibelsPerDecade = 20;

    [SerializeField] private AudioMixer _mixer;
    [SerializeField, Range(MinPercent, MaxPercent)] private float _master = 40f;
    [SerializeField, Range(MinPercent, MaxPercent)] private float _music = 40f;
    [SerializeField, Range(MinPercent, MaxPercent)] private float _effects = 40f;

    public float Master => _master;
    public float Music => _music;
    public float Effects => _effects;

    private void Start()
    {
        SetMaster(_master);
        SetMusic(_music);
        SetEffects(_effects);
    }

    public void SetMaster(float percent)
    {
        _master = Clamp(percent);
        Apply(MasterParameter, _master);
    }

    public void SetMusic(float percent)
    {
        _music = Clamp(percent);
        Apply(MusicParameter, _music);
    }

    public void SetEffects(float percent)
    {
        _effects = Clamp(percent);
        Apply(EffectsParameter, _effects);
    }

    private void Apply(string parameter, float percent)
    {
        _mixer.SetFloat(parameter, ToDecibels(percent));
    }

    private float Clamp(float percent)
    {
        return Mathf.Clamp(percent, MinPercent, MaxPercent);
    }

    private float ToDecibels(float percent)
    {
        float part = Mathf.Max(percent, SilencePercent) / MaxPercent;

        return Mathf.Log10(part) * DecibelsPerDecade;
    }
}
