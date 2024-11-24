using CQ.ApiElements.Filters.ExceptionFilter;
using CQ.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CQ.ApiElements.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class BaseAttribute : Attribute
{
    protected virtual ErrorResponse BuildUnexpectedErrorResponse(Exception exception)
    {
        var errorResponse = new ErrorResponse(
            HttpStatusCode.InternalServerError,
            "InternalProblem",
            "Problems with the server",
            string.Empty,
            "An uncatch exception occured",
            exception
            );

        return errorResponse;
    }

    protected static ObjectResult BuildResponse(ErrorResponse response)
    {
        return new ObjectResult(response)
        {
            StatusCode = (int)response.StatusCode
        };
    }

    public virtual IActionResult BuildErrorResponse(
        IDictionary<Type, ErrorResponse> errors,
        ExceptionThrownContext exceptionContext)
    {
        var error = errors.FirstOrDefault(e => e.Key == exceptionContext.Exception.GetType());

        var errorBody = BuildUnexpectedErrorResponse(exceptionContext.Exception);
        if (Guard.IsNotNull(error))
        {
            errorBody = error.Value.CompileErrorResponse(exceptionContext);
        }

        return new ObjectResult(errorBody)
        {
            StatusCode = (int)errorBody.StatusCode
        };
    }
}
