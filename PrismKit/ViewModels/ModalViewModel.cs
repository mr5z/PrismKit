using CrossUtility.Extensions;
using CrossUtility.Helpers;
using PrismKit.Enums;
using PrismKit.Services;
using PropertyChanged;

namespace PrismKit.ViewModels;

/// <inheritdoc />
public class ModalViewModel(IPopupPageNavigation popupNavigation) : ModalViewModel<ModalResult>(popupNavigation)
{
}

/// <summary>
/// ViewModel for modal pages.
/// </summary>
/// <typeparam name="TReturnType"></typeparam>
public abstract class ModalViewModel<TReturnType>(IPopupPageNavigation popupNavigation)
    : BaseViewModel, IDismissableModal
{
    public override void Initialize(INavigationParameters parameters)
    {
        base.Initialize(parameters);
        TaskCompletion = parameters[nameof(TaskCompletion)] as TaskCompletionSource<TReturnType?>;
    }

    protected async Task DismissWithResult(TReturnType? result)
    {
        // TODO cast to object if null
        var parameter = result?.AsDictionary();
        var navParam = new NavigationParameters();
        if (parameter != null)
        {
            foreach (var entry in parameter)
            {
                navParam.Add(entry.Key, entry.Value);
            }
        }

        var navigationResult = await popupNavigation.GoBackAsync(navParam);
        // TODO https://github.com/dansiegel/Prism.Plugin.Popups/issues/129
        if (navigationResult.Success)
        {
            SetResult(result);
        }
        else
        {
            Contract.NotNull(TaskCompletion);
            TaskCompletion!.SetException(navigationResult.Exception ?? new InvalidOperationException("Exception occurred during navigation."));
        }
    }

    protected void SetResult(TReturnType? result)
    {
        Contract.NotNull(TaskCompletion);
        TaskCompletion!.TrySetResult(result);
    }

    public virtual bool DismissOnBackgroundClick()
    {
        return false;
    }

    public virtual bool DismissOnBackButtonPress()
    {
        return false;
    }

    #region Properties

    [DoNotNotify]
    public TaskCompletionSource<TReturnType?>? TaskCompletion { get; private set; }

    #endregion
}
