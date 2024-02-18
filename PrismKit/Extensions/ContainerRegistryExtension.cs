using PrismKit.Services;
using PrismKit.Services.Implementation;

namespace PrismKit.Extensions;

public static class ContainerRegistryExtension
{
    public static IContainerRegistry RegisterPrismMopupsNavigation(this IContainerRegistry registry)
    {
        return registry
            .RegisterInstance(Mopups.Services.MopupService.Instance)
            .RegisterScoped<IPopupPageNavigation, PopupPageNavigation>();
    }
}