using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CQ.ApiElements.Filters.ExceptionFilter;

public class ExceptionFilter(ExceptionStoreService exceptionStoreService)
    : BaseAttribute,
    IExceptionFilter
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

    private ErrorResponse HandleException(ExceptionContext context)
    {
        var customContext = BuildThrownContext(context);

        var errorResponse = exceptionStoreService.HandleException(customContext);

        return errorResponse ??
            BuildUnexpectedErrorResponse(context.Exception);
    }

    protected virtual ExceptionThrownContext BuildThrownContext(ExceptionContext context)
    {
        return new ExceptionThrownContext(
            context,
            context.Exception,
            context.RouteData.Values["controller"].ToString()!,
            context.RouteData.Values["action"].ToString()!);
    }

    protected virtual IActionResult BuildResponse(ErrorResponse response)
    {
        return new ObjectResult(response)
        {
            StatusCode = (int)response.StatusCode,
        };
    }

    protected virtual void LogResponse(ErrorResponse response)
    {
    }
}