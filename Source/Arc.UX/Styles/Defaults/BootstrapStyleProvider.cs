using Arc.UX.Components;

namespace Arc.UX.Styles.Defaults;

public class BootstrapStyleProvider : IComponentStyleProvider
{
    public string Style(string? style) => style?.ToLower() ?? "secondary";

    public string Style(ElementStyle? style)
    {
        switch (style)
        {
            case ElementStyle.Hero: return Style("primary");
            case ElementStyle.Default: return Style("secondary");
            case ElementStyle.Error: return Style("danger");

            default: return Style(style?.ToString()?.ToLower());
        }
    }

    public string Alert(string? style)
    {
        return $"alert alert-{Style(style)}";
    }

    public string Alert(ElementStyle? style)
    {
        return $"alert alert-{Style(style)}";
    }

    public string Text(ElementStyle? style)
    {
        return $"text-{Style(style)}";
    }
}