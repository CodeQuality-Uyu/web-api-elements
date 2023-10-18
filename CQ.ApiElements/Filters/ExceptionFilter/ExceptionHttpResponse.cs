using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters
{
    public record ExceptionHttpResponse
    {
        public static ExceptionHttpResponse Default = new (
            "An exception has occurred",
            "ExceptionOccurred",
            HttpStatusCode.InternalServerError);

        public string Message { get; init; }

        public string Code { get; init; }

        public HttpStatusCode StatusCode { get; init; }

        public ExceptionHttpResponse(
                string message,
                string code,
                HttpStatusCode statusCode)
        {
            Message = message;
            Code = code;
            StatusCode = statusCode;
        }
    }
}
