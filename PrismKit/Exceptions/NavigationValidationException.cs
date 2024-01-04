using MauiUtility.Extensions.Navigations;
using PrismKit.Extensions.Navigations;

namespace PrismKit.Exceptions;

public class NavigationValidationException(ViewModelMetadata pageInfo, string? message = null) : Exception(message)
{
    public ViewModelMetadata PageInfo { get; } = pageInfo;
}
