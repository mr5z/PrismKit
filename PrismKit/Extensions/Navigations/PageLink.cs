using CrossUtility.Helpers;
using MauiUtility.Extensions.Navigations;

namespace PrismKit.Extensions.Navigations;

public class PageLink : IPageLink
{
    public INavigationService NavigationService { get; private set; }
    private readonly List<PageWithQuery> pages = [];

    public string FullPath => string.Join("/", pages.Select(e => e.GetResolvedName()));
    public IEnumerable<Type?> PageTypes => pages.Select(e => e.PageType);

    public PageLink(INavigationService navigationService)
    {
        NavigationService = navigationService;
    }

    public PageLink(INavigationService navigationService, string? start)
    {
        NavigationService = navigationService;
        if (!string.IsNullOrEmpty(start))
            pages.Add(new PageWithQuery(start));
    }

    public IPageLink AppendSegment(string? pageName, Type? pageType, object? parameter)
    {
        if (!string.IsNullOrEmpty(pageName))
        {
            pages.Add(new PageWithQuery(pageName, pageType)
            {
                Parameter = parameter
            });
        }
        return this;
    }

    public IPageLink AppendSegment(string? pageName, object? parameter)
    {
        return AppendSegment(pageName, null, parameter);
    }

    internal class PageWithQuery(string? pageName)
    {
        public PageWithQuery(string? pageName, Type? pageType) : this(pageName)
        {
            PageType = pageType;
        }

        public string? PageName { get; } = pageName;
        public Type? PageType { get; }
        public object? Parameter { get; set; }
        public string? GetResolvedName()
        {
            if (Parameter == null)
                return PageName;

            var queryString = QueryStringHelper.ToQueryString(Parameter);
            return $"{PageName}?{queryString}";
        }
    }
}
