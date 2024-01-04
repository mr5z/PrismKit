using Mopups.Interfaces;
using Mopups.Pages;
using Prism.Common;
using NavigationMode = Prism.Navigation.NavigationMode;

namespace PrismKit.Services.Implementation;

public class PopupPageNavigation : IPopupPageNavigation
{
	private static IContainerProvider Container => ContainerLocator.Current;
	private static IViewRegistry Registry => Container.Resolve<INavigationRegistry>();

	private IPageAccessor? pageAccessor;
	private IPageAccessor PageAccessor => pageAccessor ??= Container.Resolve<IPageAccessor>();

	private IPopupNavigation? popupNavigation;
	private IPopupNavigation PopupNavigation => popupNavigation ??= Container.Resolve<IPopupNavigation>();

	private Window? window;
	protected Window? Window {
		get
		{
			if (window is null && PageAccessor.Page is not null)
			{
				window = PageAccessor.Page.GetParentWindow();
			}

			if (window is null)
			{
				var windowManager = IPlatformApplication.Current?.Services.GetService<IWindowManager>();
				window = windowManager?.Windows.FirstOrDefault();
			}

			return window;
		}
	}

	public Task<NavigationResult> NavigateAsync(string name) => NavigateAsync(name, new NavigationParameters());
	
	public async Task<NavigationResult> NavigateAsync(string name, INavigationParameters parameters) {
		var fromPage = GetCurrentPage();
		var uri = UriParsingHelper.Parse(name);
		var segments = UriParsingHelper.GetUriSegments(uri);

		do
		{
			var nextSegment = segments.Dequeue();
			var segmentParameters = UriParsingHelper.GetSegmentParameters(nextSegment, parameters);
			var toPage = CreatePageFromSegment(nextSegment);
			
			segmentParameters = InjectNavigationMode(segmentParameters, NavigationMode.New);

			var canNavigate = await MvvmHelpers.CanNavigateAsync(fromPage, segmentParameters);
			if (!canNavigate)
			{
				var exception = new NavigationException(NavigationException.IConfirmNavigationReturnedFalse, toPage);
				return new NavigationResult(exception);
			}

			MvvmHelpers.OnNavigatedFrom(fromPage, segmentParameters);

			if (toPage is PopupPage popupPage)
			{
				await MvvmHelpers.OnInitializedAsync(popupPage, segmentParameters);
				await PopupNavigation.PushAsync(popupPage);
				MvvmHelpers.OnNavigatedTo(popupPage, segmentParameters);
			}
		}
		while (segments.Count > 0);

		return new NavigationResult(true);
	}

	public Task<NavigationResult> GoBackAsync() => GoBackAsync(new NavigationParameters());

	public async Task<NavigationResult> GoBackAsync(INavigationParameters parameters)
	{
		parameters = InjectNavigationMode(parameters, NavigationMode.Back);
		var page = GetCurrentPage();
		var canNavigate = await MvvmHelpers.CanNavigateAsync(page, parameters);
		if (!canNavigate)
		{
			var exception = new NavigationException(NavigationException.IConfirmNavigationReturnedFalse, page);
			return new NavigationResult(exception);
		}

		var previousPage = MvvmHelpers.GetOnNavigatedToTarget(page, Window?.Page, true);

		await PopupNavigation.PopAsync(IsAnimated(parameters));
		MvvmHelpers.OnNavigatedFrom(page, parameters);
		MvvmHelpers.OnNavigatedTo(previousPage, parameters);
		MvvmHelpers.DestroyPage(page);
		
		return new NavigationResult(true);
	}

	public Task<NavigationResult> GoBackToRootAsync() => GoBackToRootAsync(new NavigationParameters());
	public async Task<NavigationResult> GoBackToRootAsync(INavigationParameters parameters)
	{
		parameters = InjectNavigationMode(parameters, NavigationMode.Back);
		var page = GetCurrentPage() ?? throw new InvalidOperationException("Page is null.");
		var animated = IsAnimated(parameters);
		var canNavigate = await MvvmHelpers.CanNavigateAsync(page, parameters);
		if (!canNavigate)
		{
			var exception = new NavigationException(NavigationException.IConfirmNavigationReturnedFalse, page);
			return new NavigationResult(exception);
		}

		var pagesToDestroy = page.Navigation.NavigationStack.ToList(); // get all pages to destroy
		pagesToDestroy.Reverse(); // destroy them in reverse order
		var root = pagesToDestroy.Last();
		pagesToDestroy.Remove(root); //don't destroy the root page

		await PopupNavigation.PopAllAsync(animated);
		await page.Navigation.PopToRootAsync(animated);

		foreach (var destroyPage in pagesToDestroy)
		{
			MvvmHelpers.OnNavigatedFrom(destroyPage, parameters);
			MvvmHelpers.DestroyPage(destroyPage);
		}

		MvvmHelpers.OnNavigatedTo(root, parameters);
		
		return new NavigationResult(true);
	}

	protected virtual Page? GetCurrentPage()
	{
		var sourcePage = PopupNavigation.PopupStack.LastOrDefault();
		if (sourcePage != null) return sourcePage;

		return PageAccessor.Page ?? GetPageFromWindow();
	}

	private Page? GetPageFromWindow() {
		try
		{
			return Window?.Page;
		}
#if DEBUG
		catch (Exception ex)
		{
			Console.Error.WriteLine(ex);
			return null;
		}
#else
        catch
        {
            return null;
        }
#endif
	}

	protected virtual Page CreatePageFromSegment(string segment)
	{
		var segmentName = UriParsingHelper.GetSegmentName(segment);
		var page = CreatePage(segmentName);
		if (page is null) {
			var innerException = new NullReferenceException(string.Format("{0} could not be created. Please make sure you have registered {0} for navigation.", segmentName));
			throw new NavigationException(NavigationException.NoPageIsRegistered, segmentName, PageAccessor.Page, innerException);
		}

		return page;
	}

	protected virtual Page CreatePage(string segmentName)
	{
		try
		{
			var scope = Container.CreateScope();
			var page = (Page)Registry.CreateView(scope, segmentName);
			// var viewModelType = Registry.Registrations.Single(e => e.View == page.GetType()).ViewModel;
			// var viewModel = scope.Resolve(viewModelType);
			// page.BindingContext = viewModel;

			if (page is null)
				throw new NullReferenceException($"The resolved type for {segmentName} was null. You may be attempting to navigate to a Non-Page type");

			return page;
		}
		catch (NavigationException)
		{
			throw;
		}
		catch (KeyNotFoundException knfe)
		{
			throw new NavigationException(NavigationException.NoPageIsRegistered, segmentName, knfe);
		}
		catch (ViewModelCreationException vmce)
		{
			throw new NavigationException(NavigationException.ErrorCreatingViewModel, segmentName, PageAccessor.Page, vmce);
		}
		//catch(ViewCreationException viewCreationException)
		//{
		//    if(!string.IsNullOrEmpty(viewCreationException.InnerException?.Message) && viewCreationException.InnerException.Message.Contains("Maui"))
		//        throw new NavigationException(NavigationException.)
		//}
		catch (Exception ex)
		{
			var inner = ex.InnerException;
			while (inner is not null)
			{
				if (inner.Message.Contains("thread with a dispatcher"))
					throw new NavigationException(NavigationException.UnsupportedMauiCreation, segmentName, PageAccessor.Page, ex);
				inner = inner.InnerException;
			}
			throw new NavigationException(NavigationException.ErrorCreatingPage, segmentName, ex);
		}
	}

	internal bool UseModalGoBack(Page currentPage, INavigationParameters parameters)
	{
		if (parameters.ContainsKey(KnownNavigationParameters.UseModalNavigation))
			return parameters.GetValue<bool>(KnownNavigationParameters.UseModalNavigation);
		
		if (currentPage is NavigationPage navPage)
			return GoBackModal(navPage);
		
		return true;
	}

	private bool GoBackModal(NavigationPage navPage)
	{
		var rootPage = GetPageFromWindow();
		if (navPage.CurrentPage != navPage.RootPage)
			return false;
		if (navPage.CurrentPage == navPage.RootPage && navPage.Parent is Application && rootPage != navPage)
			return true;
		return navPage.Parent is TabbedPage tabbed && tabbed != rootPage;
	}

	/// <summary>
	/// Hack!
	/// </summary>
	private static INavigationParameters InjectNavigationMode(INavigationParameters parameters, NavigationMode mode)
	{
		const string KnownNavigationKey = "__NavigationMode";
		
		if (!parameters.ContainsKey(KnownNavigationParameters.UseModalNavigation))
			parameters.Add(KnownNavigationParameters.UseModalNavigation, true);
		
		var internalParams = (INavigationParametersInternal)parameters;
		if (!internalParams.ContainsKey(KnownNavigationKey))
		{
			internalParams.Add(KnownNavigationKey, mode);
		}

		return (INavigationParameters)internalParams;
	}

	private static bool IsAnimated(INavigationParameters parameters)
	{
		return !parameters.ContainsKey(KnownNavigationParameters.Animated) || parameters.GetValue<bool>(KnownNavigationParameters.Animated);
	}
}