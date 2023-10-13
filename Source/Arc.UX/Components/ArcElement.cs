

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Arc.UX.Components;


public interface IArcComponent
{
    public bool Visible { get; set; } 
    public string? CssClasses { get; set; }
    public string ComponentName { get; }
}

public abstract class ArcElement : ComponentBase, IArcComponent
{
    [Parameter]
    public bool Visible { get; set; } = true;

    [Parameter]
    public string? CssClasses { get; set; }


    public string ComponentName => GetType().Name;
}


public abstract class ArcInputBase<T>: InputBase<T>, IArcComponent
{
    [Parameter]
    public bool Visible { get; set; } = true;

    [Parameter]
    public string? CssClasses { get; set; }


    public string ComponentName => GetType().Name;
}