using CQ.ApiElements.Filters.ExceptionFilter;
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
}
