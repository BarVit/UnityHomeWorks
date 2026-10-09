using UnityEngine;

public class SkinnedSprite
{
    private readonly SpriteRenderer _renderer;

    private SpriteSkin _skin;

    public SkinnedSprite(SpriteRenderer renderer)
    {
        _renderer = renderer;
    }

    public void Wear(SpriteSkin skin)
    {
        _skin = skin;
        Apply();
    }

    public void Apply()
    {
        if (_skin == null || _renderer == null)
            return;

        _renderer.sprite = _skin.Get(_renderer.sprite);
    }
}
