using PrismKit.ViewModels;

namespace PrismKit.Extensions;

internal class Common
{
    private const string KnownViewModelPattern = "ViewModel";
    private const string KnownPagePattern = "Page";
    
    internal static string ToPageName<TViewModel>() where TViewModel : BaseViewModel
    {
        return typeof(TViewModel).Name.Replace(KnownViewModelPattern, KnownPagePattern);
    }
}