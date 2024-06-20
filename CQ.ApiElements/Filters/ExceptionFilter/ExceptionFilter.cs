using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CQ.ApiElements.Filters
{
    public class ExceptionFilter(ExceptionStoreService _exceptionStoreService) : IExceptionFilter
    {
        public virtual void OnException(ExceptionContext context)
        {
            if (context == null)
            {
                return;
            }

            var response = HandleException(context);

            LogResponse(response);

            context.Result = BuildResponse(response);
        }

        private ExceptionResponse HandleException(ExceptionContext context)
        {
            var customContext = BuildThrownContext(context);

            return _exceptionStoreService.HandleException(customContext);
        }

        protected virtual ExceptionThrownContext BuildThrownContext(ExceptionContext context)
        {
            return new ExceptionThrownContext(
                context,
                context.Exception,
                context.RouteData.Values["controller"].ToString()!,
                context.RouteData.Values["action"].ToString()!);
        }

        protected virtual IActionResult BuildResponse(ExceptionResponse response)
        {
            return new ObjectResult(new
            {
                Code = response.Code,
                Message = response.Message
            })
            {
                StatusCode = (int)response.StatusCode,
            };
        }

        protected virtual void LogResponse(ExceptionResponse response) { }
    }
}