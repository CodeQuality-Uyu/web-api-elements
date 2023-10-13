using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters
{
    public class DinamicExceptionMapping<TException> : BaseExceptionMapping
        where TException : Exception
    {
        public LogExceptionMapping<TException>? Log { get; }

        public Func<TException, CustomExceptionContext, string> ResponseMessage { get; }

        public DinamicExceptionMapping(
            Func<TException, CustomExceptionContext, string> responseMessage,
            string responseCode,
            HttpStatusCode responseStatusCode,
            LogExceptionMapping<TException>? log = null,
            string? controllerName = null) : base(responseCode, responseStatusCode, controllerName)
        {
            Log = log;
            ResponseMessage = responseMessage;
        }

        public override string GetResponseMessage(Exception exception, CustomExceptionContext context)
        {
            var customException = exception as TException;

            if (customException == null)
            {
                return "Cannot handle error";
            }

            return ResponseMessage(customException, context);
        }

        public override Log? GetLog(Exception exception, CustomExceptionContext context)
        {
            var customException = exception as TException;

            if (customException == null)
            {
                return new Log(
                    GetResponseMessage(customException, context),
                    null,
                    LogLevel.Warning);
            }

            if (Log == null)
            {
                return null;
            }

            return new Filters.Log(
                Log.Message(customException, context),
                Log.ExtraInformation(customException, context),
                Log.Level);
        }

        public override Type GetTypeRegistered()
        {
            return typeof(TException);
        }
    }

    public record LogExceptionMapping<TException>(
        Func<TException, CustomExceptionContext, string> Message, 
        Func<TException, CustomExceptionContext, object>? ExtraInformation, 
        LogLevel Level) 
        where TException : Exception;
}
