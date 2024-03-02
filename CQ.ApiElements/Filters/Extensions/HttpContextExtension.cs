using CQ.ApiElements.Filters.Exceptions;
using Microsoft.AspNetCore.Http;

namespace CQ.ApiElements.Filters.Extensions
{
    public static class HttpContextExtension
    {
        public static TResult GetItem<TResult>(this HttpContext context, ContextItems item) 
            where TResult : class
        {
            var element = context.Items[item];

            if (element == null)
                throw new ContextItemNotFoundException(item);

            return (TResult) element;
        }
    }
}
