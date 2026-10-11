using UnityEngine;

public readonly struct UpgradeRowData
{
    public UpgradeRowData(Sprite icon, string title, string hint, string value, int level, int maxLevel, int price,
        bool canBuy, Sprite currency)
    {
        Icon = icon;
        Title = title;
        Hint = hint;
        Value = value;
        Level = level;
        MaxLevel = maxLevel;
        Price = price;
        CanBuy = canBuy;
        Currency = currency;
    }

    public Sprite Icon { get; }

    public string Title { get; }

    public string Hint { get; }

    public string Value { get; }

    public int Level { get; }

    public int MaxLevel { get; }

    public int Price { get; }

    public bool CanBuy { get; }

    public Sprite Currency { get; }
}
