using Microsoft.AspNetCore.Mvc;

namespace CQ.ApiElements.Filters.Extensions
{
    public static class ControllerBaseExtension
    {
        public static TResult GetItem<TResult>(
            this ControllerBase controller,
            ContextItems item)
            where TResult : class
        {
            var result = controller.HttpContext.GetItem<TResult>(item);

            return result;
        }
    }
}
