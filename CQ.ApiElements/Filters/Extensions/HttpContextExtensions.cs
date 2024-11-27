using CQ.ApiElements.Filters.Exceptions;
using CQ.Utility;
using Microsoft.AspNetCore.Http;

namespace CQ.ApiElements.Filters.Extensions;

public static class HttpContextExtensions
{
    public static TResult GetItem<TResult>(
        this HttpContext context,
        ContextItem item)
        where TResult : class
    {
        var element = GetItem(context, item);

        return (TResult)element;
    }

    public static object GetItem(
        this HttpContext context,
        ContextItem item)
    {
        var element = context.Items[item];

        if (Guard.IsNull(element))
        {
            ContextItemNotFoundException.Throw(item);
        }

        return element;
    }

    public static TResult GetItemOrDefault<TResult>(
        this HttpContext context,
        ContextItem item)
        where TResult : class
    {
        var element = context.Items[item];

        return (TResult)element;
    }

    public static object? GetItemOrDefault(
        this HttpContext context,
        ContextItem item)
    {
        var element = context.Items[item];

        return element;
    }
}
