using CrossUtility.Helpers;
using PrismKit.Enums;
using PrismKit.Services;
using PrismKit.ViewModels;
using static PrismKit.Extensions.Common;

namespace PrismKit.Extensions;

public static class PopupNavigationExtension
{
    public static Task<ModalResult> PushModal<TModalPopup>(
        this IPopupPageNavigation navigationService, 
        INavigationParameters? parameters = null)
        where TModalPopup : ModalViewModel
    {
        return PushModal<TModalPopup, ModalResult>(navigationService, parameters);
    }
    
    public static async Task<TReturnType?> PushModal<TModalPopup, TReturnType>(
        this IPopupPageNavigation popupNavigation,
        INavigationParameters? parameters = null)
        where TModalPopup : ModalViewModel<TReturnType>
    {
        parameters ??= new NavigationParameters();
        var completion = new TaskCompletionSource<TReturnType>();
        parameters.Add(nameof(ModalViewModel.TaskCompletion), completion);

        var page = ToPageName<TModalPopup>();
        var navResult = await popupNavigation.NavigateAsync(page, parameters);
        
        Contract.ThrowOn(navResult.Exception);

        return await completion.Task;
    }
}