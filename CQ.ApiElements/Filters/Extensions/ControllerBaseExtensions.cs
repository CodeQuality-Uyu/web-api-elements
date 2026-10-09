using Microsoft.AspNetCore.Mvc;

namespace CQ.ApiElements.Filters.Extensions;

public static class ControllerBaseExtensions
{
    public static TResult GetItem<TResult>(
        this ControllerBase controller,
        ContextItem item)
        where TResult : class
    {
        var result = controller.HttpContext.GetItem<TResult>(item);

        return result;
    }

    public static TResult GetItem<TKey, TResult>(
        this ControllerBase controller,
        TKey item)
        where TKey : Enum
        where TResult : class
    {
        var result = controller.HttpContext.GetItem<TKey, TResult>(item);

        return result;
    }
}
