using System.Reflection;
using PrismKit.Attributes;
using PrismKit.ViewModels;

namespace PrismKit.Extensions.Navigations;

public class ViewModelMetadata
{
    public ViewModelMetadata(Type pageType)
    {
        if (!pageType.IsSubclassOf(typeof(BaseViewModel)))
            throw new ArgumentException($"'{nameof(pageType)}' should be assignable from '{nameof(BaseViewModel)}'");

        PageType = pageType;
        RequiresAuthentication = AuthenticationInfo();
        Roles = RolesInfo();
    }

    public Type PageType { get; }
    public bool RequiresAuthentication { get; }
    public IReadOnlyList<string> Roles { get; }
    public string PageName => PageType.Name;

    private bool AuthenticationInfo()
    {
        var attribute = PageType.GetCustomAttribute<RequiresAuthenticationAttribute>();
        return attribute?.IsRequired ?? false;
    }

    private IReadOnlyList<string> RolesInfo()
    {
        var attribute = PageType.GetCustomAttribute<RequiresRoleAttribute>();
        var roles = attribute?.Roles.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return new List<string>(roles ?? Enumerable.Empty<string>());
    }
}
