using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters
{
    public abstract class BaseExceptionMapping
    {
        public string ResponseCode { get; }

        public HttpStatusCode ResponseStatusCode { get; }

        public string? ControllerName { get; }

        public BaseExceptionMapping(string responseCode, HttpStatusCode responseStatusCode, string? controllerName)
        {
            ResponseCode = responseCode;
            ResponseStatusCode = responseStatusCode;
            ControllerName = controllerName;
        }

        public abstract string GetResponseMessage(Exception exception, CustomExceptionContext context);

        public abstract Log? GetLog(Exception exception, CustomExceptionContext context);

        public abstract Type GetTypeRegistered();
    }

    public record Log
    {
        public string Message { get; }

        public object? ExtraInformation { get; }

        public string Level { get; }

        public Log(string message, object? extraInformation, string level)
        {
            Message = message;
            ExtraInformation = extraInformation;
            Level = level;
        }
    }
}
