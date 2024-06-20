using CQ.ApiElements.Filters.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace CQ.ApiElements.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class BaseAttribute : Attribute
{
    protected virtual TService GetService<TService>(FilterContext context) where TService : class
    {
        return context.HttpContext.RequestServices.GetRequiredService<TService>();
    }

    protected TItem? GetItem<TItem>(FilterContext context, ContextItems item)
        where TItem : class
    {
        return context.HttpContext.GetItem<TItem>(item);
    }

    protected object? GetItem(FilterContext context, ContextItems item)
    {
        return context.HttpContext.GetItem(item);
    }

    protected void SetItem(FilterContext context, ContextItems item, object value)
    {
        context.HttpContext.Items[item] = value;
    }
}
