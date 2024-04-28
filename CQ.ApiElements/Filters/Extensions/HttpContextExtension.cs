using CQ.ApiElements.Filters.Exceptions;
using CQ.Utility;
using Microsoft.AspNetCore.Http;

namespace CQ.ApiElements.Filters.Extensions
{
    public static class HttpContextExtension
    {
        public static TResult GetItem<TResult>(this HttpContext context, ContextItems item) 
            where TResult : class
        {
            var element = context.Items[item];

            if (Guard.IsNull(element))
            {
                throw new ContextItemNotFoundException(item);
            }

            return (TResult) element;
        }
    }
}
