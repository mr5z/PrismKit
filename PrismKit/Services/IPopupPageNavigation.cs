namespace PrismKit.Services;

public interface IPopupPageNavigation
{
    Task<NavigationResult> NavigateAsync(string name);
    Task<NavigationResult> NavigateAsync(string name, INavigationParameters parameters);
    Task<NavigationResult> GoBackAsync();
    Task<NavigationResult> GoBackAsync(INavigationParameters parameters);
    Task<NavigationResult> GoBackToRootAsync();
    Task<NavigationResult> GoBackToRootAsync(INavigationParameters parameters);
}