using MauiUtility.Extensions.Navigations;
using PrismKit.Exceptions;
using PrismKit.Extensions.Navigations;
using PrismKit.Services;
using PrismKit.ViewModels;
using static PrismKit.Extensions.Common;

namespace PrismKit.Extensions;

public static class NavigationExtension
{
    public static Func<IPopupPageNavigation, INavigationParameters?, ViewModelMetadata, Task<bool>>? Validator { get; set; }

    public static async Task<INavigationResult> PushAsync<TViewModel>(
        this INavigationService navigationService, INavigationParameters? parameters = null)
        where TViewModel : BaseViewModel
    {
        var pageName = ToPageName<TViewModel>();

        var validationResult = await PerformValidation(typeof(TViewModel), parameters);
        if (validationResult != null)
            return validationResult;

        var navigationResult = await navigationService.NavigateAsync(pageName, parameters);
        return navigationResult;
    }

    public static IPageLink Absolute(this INavigationService navigationService, bool withNavigation = false)
    {
        var rootPage = "/" + (withNavigation ? nameof(NavigationPage) : string.Empty);
        return new PageLink(navigationService, rootPage);
    }

    public static IPageLink Relative(this INavigationService navigationService, bool withNavigation = false)
    {
        var rootPage = withNavigation ? nameof(NavigationPage) : string.Empty;
        return new PageLink(navigationService, rootPage);
    }

    public static IPageLink Push<TViewModel>(this IPageLink pageLink, object? parameter = null) where TViewModel : BaseViewModel
    {
        var page = ToPageName<TViewModel>();
        return pageLink.AppendSegment(page, typeof(TViewModel), parameter);
    }

    public static IPageLink Pop(this IPageLink pageLink, int popCount = 1)
    {
        const string page = "..";
        for (var i = 0; i < popCount; ++i)
        {
            pageLink.AppendSegment(page);
        }
        return pageLink;
    }

    public static async Task<INavigationResult> NavigateAsync(this IPageLink pageLink,
        INavigationParameters? parameters = null)
    {
        var navigationService = (pageLink as PageLink)!.NavigationService;

        foreach (var page in pageLink.PageTypes)
        {
            if (page == null)
                continue;

            var validationResult = await PerformValidation(page, parameters);
            if (validationResult != null)
                return validationResult;
        }

        var fullPath = pageLink.FullPath;
        var navigationResult = await navigationService.NavigateAsync(fullPath, parameters);
        return navigationResult;
    }

    private static async Task<NavigationResult?> PerformValidation(
        Type pageType,
        INavigationParameters? parameters)
    {
        if (Validator == null)
            return null;

        var pageInfo = new ViewModelMetadata(pageType);
        var popupNavigation = ContainerLocator.Current.Resolve<IPopupPageNavigation>();
        var shouldProceed = await Validator.Invoke(popupNavigation, parameters, pageInfo);

        if (shouldProceed)
            return null;

        return new NavigationResult(new NavigationValidationException(pageInfo));
    }
}
