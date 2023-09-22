using Arc.UX.Components;

namespace Arc.UX.Styles;

public interface IComponentStyleProvider
{
    public string Alert(string? style);
    public string Alert(ElementStyle? style);

    public string Text(ElementStyle? style);
    public string Style(ElementStyle? style);
}