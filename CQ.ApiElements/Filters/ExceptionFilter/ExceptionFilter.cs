

using CQ.ApiElements.Filters.Extension;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CQ.ApiElements.Filters
{
    public class ExceptionFilter : Attribute, IExceptionFilter
    {
        private readonly ExceptionStoreService _exeptionStore;

        public ExceptionFilter(
            ExceptionStoreService exeptionStore, 
            ExceptionRegistryService exeptionRegistryService)
        {
            _exeptionStore = exeptionStore;

            exeptionRegistryService.RegisterKnownExceptions(_exeptionStore);
        }

        public void OnException(ExceptionContext context)
        {
            if (context == null)
            {
                return;
            }

            var exceptionResponse = HandleException(context);

            context.Result = BuildResponse(context, exceptionResponse);
        }

        private ExceptionHttpResponse HandleException(ExceptionContext context)
        {
            var customcontext = new CustomExceptionContext(
                context.Exception,
                context.RouteData.Values["controller"].ToString());

            var customExceptionContext = this.BuildCustomExceptionContext(customcontext);

            return this._exeptionStore.HandleException(customExceptionContext);
        }

        protected virtual CustomExceptionContext BuildCustomExceptionContext(CustomExceptionContext context)
        {
            return context;
        }

        protected virtual IActionResult BuildResponse(ExceptionContext context, ExceptionHttpResponse error)
        {
            return context.HttpContext.Request.CreateCQErrorResponse(
                    error.StatusCode,
                    error.Code,
                    error.Message
                );
        }
    }
}