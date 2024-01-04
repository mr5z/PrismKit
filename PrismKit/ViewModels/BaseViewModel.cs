using NavigationMode = Prism.Navigation.NavigationMode;

namespace PrismKit.ViewModels;

public class BaseViewModel: BindableBase, INavigationAware, IInitialize, IInitializeAsync
{
    protected virtual void OnPageLoaded(INavigationParameters parameters)
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

    }

    public void OnNavigatedTo(INavigationParameters parameters)
    {
        var mode = parameters.GetNavigationMode();

        if (mode == NavigationMode.New)
            OnPageLoaded(parameters);
    }
}
