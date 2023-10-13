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

    public record Log(string Message, object? ExtraInformation, LogLevel Level);

    public record LogLevel
    {
        public static LogLevel Warning = new LogLevel("Warning");

        public static LogLevel Error = new LogLevel("Error");

        public string Value { get; init; }

        private LogLevel(string value)
        {
            Value = value;
        }
    }
}
