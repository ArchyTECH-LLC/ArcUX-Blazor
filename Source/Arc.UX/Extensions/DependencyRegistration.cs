using Arc.UX.Styles;
using Arc.UX.Styles.Defaults;
using Microsoft.Extensions.DependencyInjection;

namespace Arc.UX.Extensions
{
    public static class DependencyRegistration
    {
        public static void AddArcUX(this IServiceCollection services)
        {
            services.AddSingleton<IComponentStyleProvider, BootstrapStyleProvider>();
            services.AddSingleton<IIconStyleProvider, FontAwesomeIconStyleProvider>();
        }
    }
}
