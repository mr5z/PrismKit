using NavigationMode = Prism.Navigation.NavigationMode;

namespace PrismKit.ViewModels;

public class BaseViewModel: INavigatedAware, IInitialize, IInitializeAsync
{
    protected virtual void OnPageLoaded(INavigationParameters parameters)
    {

    }

    protected virtual void OnPageUnloaded(INavigationParameters parameters)
    {

    }

    public virtual void Initialize(INavigationParameters parameters)
    {

    }

    public virtual Task InitializeAsync(INavigationParameters parameters)
    {
        return Task.CompletedTask;
    }

    public void OnNavigatedFrom(INavigationParameters parameters)
    {
        var mode = parameters.GetNavigationMode();

        if (mode == NavigationMode.Back)
        {
            OnPageUnloaded(parameters);
        }
    }

    public void OnNavigatedTo(INavigationParameters parameters)
    {
        var mode = parameters.GetNavigationMode();

        if (mode == NavigationMode.New)
        {
            OnPageLoaded(parameters);
        }
    }
}
