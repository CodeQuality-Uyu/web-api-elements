using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters
{
    public class ExceptionStoreService
    {
        private readonly Dictionary<Type, ExceptionMappings> Mappings;

        private Action<Log>? Logger;

        private Action<Exception>? LoggerException;

        public ExceptionStoreService()
        {
            Mappings = new Dictionary<Type, ExceptionMappings>();
        }

        public void AddLogger(Action<Log> logger)
        {
            Logger = logger;
        }

        public void AddLoggerUnhandleException(Action<Exception> logger)
        {
            LoggerException = logger;
        }

        internal ExceptionHttpResponse HandleException(CustomExceptionContext exceptionContext)
        {
            Exception exception = exceptionContext.Exception;
            if (!Mappings.ContainsKey(exception.GetType()))
            {
                if(LoggerException!= null)
                {
                    LoggerException(exception);
                }

                return BuildDefaultResponse(exception, exceptionContext);
            }

            var mapping = Mappings[exception.GetType()].GetMapping(exceptionContext.ControllerName);

            LogMessage(mapping, exception, exceptionContext);

            return BuildResponse(mapping, exception, exceptionContext);
        }

        protected virtual ExceptionHttpResponse BuildDefaultResponse(Exception exception, CustomExceptionContext exceptionContext)
        {
            return ExceptionHttpResponse.Default;
        }

        private void LogMessage(BaseExceptionMapping mapping, Exception exception, CustomExceptionContext exceptionContext)
        {
            var log = mapping.GetLog(exception, exceptionContext);

            if (log == null || Logger == null)
            {
                return;
            }

            Logger(log);
        }

        protected virtual ExceptionHttpResponse BuildResponse(BaseExceptionMapping mapping, Exception exception, CustomExceptionContext context)
        {
            return new ExceptionHttpResponse(
               mapping.GetResponseMessage(exception, context),
               mapping.ResponseCode,
                mapping.ResponseStatusCode);
        }

        public void RegisterException(BaseExceptionMapping exception)
        {
            var exceptionType = exception.GetTypeRegistered();

            if (Mappings.ContainsKey(exceptionType))
            {
                var exceptions = Mappings[exceptionType];

                exceptions.AddMapping(exception);
                return;
            }

            var exceptionMappings = new ExceptionMappings(exception);
            Mappings.Add(exceptionType, exceptionMappings);
        }
    }
}
