using System.Text;

public static class NumberFormat
{
    private const int SignificantDigits = 3;
    private const char DecimalSeparator = ',';
    private const char GroupSeparator = ' ';

    private static readonly long[] Units = { 1000000000000L, 1000000000L, 1000000L, 1000L };
    private static readonly string[] Suffixes = { "t", "b", "m", "k" };

    public static string Compact(long value)
    {
        if (value < 0)
            return "-" + Compact(-value);

        for (int i = 0; i < Units.Length; i++)
        {
            if (value >= Units[i])
                return Shorten(value, Units[i]) + Suffixes[i];
        }

        return value.ToString();
    }

    public static string Full(long value)
    {
        string digits = value < 0 ? (-value).ToString() : value.ToString();
        StringBuilder grouped = new StringBuilder(value < 0 ? "-" : string.Empty);

        for (int i = 0; i < digits.Length; i++)
        {
            if (i > 0 && (digits.Length - i) % 3 == 0)
                grouped.Append(GroupSeparator);

            grouped.Append(digits[i]);
        }

        return grouped.ToString();
    }

    private static string Shorten(long value, long unit)
    {
        long whole = value / unit;
        int decimals = SignificantDigits - whole.ToString().Length;

        if (decimals <= 0)
            return whole.ToString();

        long scale = Pow10(decimals);
        long fraction = value % unit * scale / unit;

        if (fraction == 0)
            return whole.ToString();

        return whole.ToString() + DecimalSeparator + fraction.ToString().PadLeft(decimals, '0');
    }

    private static long Pow10(int power)
    {
        long result = 1;

        for (int i = 0; i < power; i++)
            result *= 10;

        return result;
    }
}
