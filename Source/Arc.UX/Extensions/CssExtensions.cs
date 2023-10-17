namespace Arc.UX.Extensions;

internal static class CssExtensions
{
    public static string? ToLoweredCssClass<T>(this T? obj, string? prefix = null)
    {
        return obj != null ? $"{prefix}{obj.ToString().ToLower()}" : null;
    }

    public static string? ToggleCssClass<T>(this T? obj, string cssClass)
    {
        return obj != null ? cssClass : null;
    }

}