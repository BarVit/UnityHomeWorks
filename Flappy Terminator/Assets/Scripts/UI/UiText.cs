public static class UiText
{
    public static string Title(string id)
    {
        return id.Replace('_', ' ').ToUpperInvariant();
    }
}
