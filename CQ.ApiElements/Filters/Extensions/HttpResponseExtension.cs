using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
