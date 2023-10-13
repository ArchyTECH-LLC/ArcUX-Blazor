

using Microsoft.AspNetCore.Components;

namespace Arc.UX.Components;

public abstract class ArcComponent : ComponentBase
{
    [Parameter]
    public bool Visible { get; set; } = true;

    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public ElementStyle? Style { get; set; }

    public string ComponentName => GetType().Name;
}