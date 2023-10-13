using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters
{
    public class StaticExceptionMapping<TException> : BaseExceptionMapping
        where TException : Exception
    {
        public string Message { get; }

        public Log? Log { get; }

        public StaticExceptionMapping(
            string message,
            string responseCode,
            HttpStatusCode responseStatusCode,
            Log? log = null,
            string? controllerName = null) : base(responseCode, responseStatusCode, controllerName)
        {
            Message = message;
            Log = log;
        }

        public override string GetResponseMessage(Exception exception, CustomExceptionContext context)
        {
            return Message;
        }

        public override Log? GetLog(Exception exception, CustomExceptionContext context)
        {
            return Log;
        }

        public override Type GetTypeRegistered()
        {
            return typeof(TException);
        }
    }
}
