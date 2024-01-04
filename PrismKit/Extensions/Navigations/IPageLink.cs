namespace MauiUtility.Extensions.Navigations;

public interface IPageLink
{
    string FullPath { get; }
    IEnumerable<Type?> PageTypes { get; }
    IPageLink AppendSegment(string pageName, object? parameter = null);
    IPageLink AppendSegment(string pageName, Type pageType, object? parameter = null);
}
