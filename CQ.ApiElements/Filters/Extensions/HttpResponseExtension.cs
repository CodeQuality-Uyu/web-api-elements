using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CQ.ApiElements.Filters.Extensions
{
    public static class HttpResponseExtension
    {
        public static IActionResult CreateCQErrorResponse(this HttpRequest request, HttpStatusCode statusCode, string code, string message)
        {
            return new ObjectResult(new
            {
                Code = code,
                Message = message
            })
            {
                StatusCode = (int)statusCode,
            };
        }
    }
}
