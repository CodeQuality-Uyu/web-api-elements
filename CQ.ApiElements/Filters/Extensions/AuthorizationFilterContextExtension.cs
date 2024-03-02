using CQ.ApiElements.Filters.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CQ.ApiElements.Filters.Extensions
{
    public static class AuthorizationFilterContextExtension
    {
        public static TResult GetItem<TResult>(this AuthorizationFilterContext context, ContextItems item) 
            where TResult : class
        {
            var element = context.HttpContext.Items[item];

            if (element == null)
                throw new ContextItemNotFoundException(item);

            return (TResult) element;
        }
    }
}
