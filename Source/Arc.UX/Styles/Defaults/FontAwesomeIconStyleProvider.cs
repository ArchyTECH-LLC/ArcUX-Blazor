using ArchyTECH.Core.Extensions;

namespace Arc.UX.Styles.Defaults;

public class FontAwesomeIconStyleProvider : IIconStyleProvider
{
    public string? Icon(string? iconName, string? iconDesign, string? iconAnimation)
    {
        if (iconName == null) return null;

        var design = iconDesign?.ToLower() ?? "fa-solid";
        var animation = iconAnimation.HasValue() ? null : $" fa-{iconAnimation.ToLower()}";
        return $"fa-{iconName.ToLower()} {design} {animation}";
    }
}