using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSoundFade
{
    private readonly Dictionary<AudioSource, float> _volumes = new Dictionary<AudioSource, float>();

    public IEnumerator Fade(float duration)
    {
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            Capture();
            Scale(1f - elapsed / duration);

            yield return null;
        }

        Finish();
    }

    public void Finish()
    {
        foreach (KeyValuePair<AudioSource, float> pair in _volumes)
        {
            if (pair.Key == null)
                continue;

            pair.Key.Stop();
            pair.Key.volume = pair.Value;
        }

        _volumes.Clear();
    }

    private void Capture()
    {
        foreach (AudioSource source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
        {
            if (source.isPlaying && _volumes.ContainsKey(source) == false && source.GetComponent<MusicPlayer>() == null)
                _volumes.Add(source, source.volume);
        }
    }

    private void Scale(float share)
    {
        foreach (KeyValuePair<AudioSource, float> pair in _volumes)
        {
            if (pair.Key != null)
                pair.Key.volume = pair.Value * share;
        }
    }
}
