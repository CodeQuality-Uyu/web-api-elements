using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters
{
    public class ExceptionStoreService
    {
        protected readonly Dictionary<Type, ExceptionMappings> Mappings;

        public ExceptionStoreService()
        {
            Mappings = new Dictionary<Type, ExceptionMappings>();
        }

        internal ExceptionHttpResponse HandleException(CustomExceptionContext exceptionContext)
        {
            Exception exception = exceptionContext.Exception;
            if (!Mappings.ContainsKey(exception.GetType()))
            {
                return ExceptionHttpResponse.Default;
            }

            ExceptionMapping mapping = Mappings[exception.GetType()].GetMapping(exceptionContext.ControllerName);
            string message = mapping.LogMessage(exception, exceptionContext);
            object obj = mapping.ExtraInformationGetter(exception, exceptionContext);
            if (mapping.LogLevel == "Error")
            {
                object extraInformation = obj;
            }
            else
            {
                object extraInformation = obj;
            }

            return new ExceptionHttpResponse
            {
                Code = mapping.ResponseCode,
                Message = mapping.ResponseMessage(exception, exceptionContext),
                StatusCode = mapping.ResponseStatusCode
            };
        }

        public void RegisterMapping<TException>(string logMessage, Func<TException, CustomExceptionContext, object> extraInformationGetter, HttpStatusCode responseStatusCode, string responseMessage, string responseCode, string logLevel = "Warning", string controllerName = null, bool isOAuth2Error = false) where TException : Exception
        {
            Func<Exception, CustomExceptionContext, object> extraInformationGetter2 = delegate (Exception exception, CustomExceptionContext context)
            {
                TException arg = exception as TException;
                return extraInformationGetter(arg, context);
            };
            Func<Exception, CustomExceptionContext, string> logMessage2 = (Exception e, CustomExceptionContext c) => logMessage;
            Func<Exception, CustomExceptionContext, string> responseMessage2 = (Exception e, CustomExceptionContext c) => responseMessage;
            ExceptionMapping item = new ExceptionMapping
            {
                BaseLogMessage = logMessage,
                LogMessage = logMessage2,
                ExtraInformationGetter = extraInformationGetter2,
                ResponseStatusCode = responseStatusCode,
                ResponseMessage = responseMessage2,
                ResponseCode = responseCode,
                ControllerName = controllerName,
                LogLevel = logLevel,
                IsOAuth2Error = isOAuth2Error
            };

            AddException<TException>(item);
        }

        public void RegisterMapping<TException>(
            Func<Exception, CustomExceptionContext, string> logMessageFunc, 
            Func<TException, CustomExceptionContext, object> extraInformationGetter, 
            HttpStatusCode responseStatusCode, 
            Func<Exception, CustomExceptionContext, string> responseMessageFunc, 
            string responseCode, 
            string logLevel = "Warning", 
            string controllerName = null, 
            bool isOAuth2Error = false) where TException : Exception
        {
            Func<Exception, CustomExceptionContext, object> extraInformationGetter2 = delegate (Exception exception, CustomExceptionContext context)
            {
                TException arg = exception as TException;
                return extraInformationGetter(arg, context);
            };
            ExceptionMapping item = new ExceptionMapping
            {
                LogMessage = logMessageFunc,
                ExtraInformationGetter = extraInformationGetter2,
                ResponseStatusCode = responseStatusCode,
                ResponseMessage = responseMessageFunc,
                ResponseCode = responseCode,
                ControllerName = controllerName,
                LogLevel = logLevel,
                IsOAuth2Error = isOAuth2Error
            };

            AddException<TException>(item);
        }

        protected void AddException<TException>(ExceptionMapping item)
        {
            if (Mappings.ContainsKey(typeof(TException)))
            {
                Mappings[typeof(TException)].Mappings.Add(item);
                return;
            }

            ExceptionMappings exceptionMappings = new ExceptionMappings();
            exceptionMappings.Mappings.Add(item);
            Mappings.Add(typeof(TException), exceptionMappings);
        }
    }
}
