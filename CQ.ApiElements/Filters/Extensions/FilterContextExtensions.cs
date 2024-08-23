
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace CQ.ApiElements.Filters.Extensions;

public static class FilterContextExtensions
{
    public static TService GetService<TService>(this FilterContext context)
        where TService : class
    {
        return context.HttpContext.RequestServices.GetRequiredService<TService>();
    }

    public static object GetItem(
        this FilterContext context,
        ContextItems item)
    {
        return context.HttpContext.GetItem(item);
    }

    public static TItem GetItem<TItem>(
        this FilterContext context,
        ContextItems item)
        where TItem : class
    {
        return context.HttpContext.GetItem<TItem>(item);
    }

    public static TItem? GetItemOrDefault<TItem>(
        this FilterContext context,
        ContextItems item)
        where TItem : class
    {
        return context.HttpContext.GetItemOrDefault<TItem>(item);
    }

    public static object? GetItemOrDefault(
        this FilterContext context,
        ContextItems item)
    {
        return context.HttpContext.GetItemOrDefault(item);
    }

    public static void SetItem(
        this FilterContext context,
        ContextItems item,
        object value)
    {
        context.HttpContext.Items[item] = value;
    }
}
