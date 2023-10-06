

using CQ.ApiElements.Filters.Extension;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CQ.ApiElements.Filters
{
    public class ExceptionFilter : Attribute, IExceptionFilter
    {
        private readonly ExceptionStoreService _exeptionStore;

        public ExceptionFilter(ExceptionStoreService exeptionStore, ExceptionRegistryService exeptionRegistryService)
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
            var customExceptionContext = this.BuildCustomExceptionContext(context);

            return this._exeptionStore.HandleException(customExceptionContext);
        }

        protected CustomExceptionContext BuildCustomExceptionContext(ExceptionContext context)
        {
            return new CustomExceptionContext
            {
                Exception = context.Exception,
            };
        }

        protected IActionResult BuildResponse(ExceptionContext context, ExceptionHttpResponse error)
        {
            return context.HttpContext.Request.CreatePlayerFinderErrorResponse(
                    error.StatusCode,
                    error.Code,
                    error.Message
                );
        }
    }
}