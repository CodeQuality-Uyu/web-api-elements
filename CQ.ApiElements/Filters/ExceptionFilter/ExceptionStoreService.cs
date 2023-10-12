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

            var mapping = Mappings[exception.GetType()].GetMapping(exceptionContext.ControllerName);
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

        public void RegisterMapping<TException>(
            string logMessage, 
            Func<TException, CustomExceptionContext, object> extraInformationGetter, 
            HttpStatusCode responseStatusCode, 
            string responseMessage, 
            string responseCode, 
            string logLevel = "Warning", 
            string controllerName = null, 
            bool isDefault = false) where TException : Exception
        {
            Func<Exception, CustomExceptionContext, string> logMessage2 = (Exception e, CustomExceptionContext c) => logMessage;
            Func<Exception, CustomExceptionContext, string> responseMessage2 = (Exception e, CustomExceptionContext c) => responseMessage;
            var item = new ExceptionMapping<TException>
            {
                BaseLogMessage = logMessage,
                LogMessage = logMessage2,
                ExtraInformationGetter = extraInformationGetter,
                ResponseStatusCode = responseStatusCode,
                ResponseMessage = responseMessage2,
                ResponseCode = responseCode,
                ControllerName = controllerName,
                LogLevel = logLevel,
                IsDefault = isDefault
            };

            AddException(item);
        }

        public void RegisterMapping<TException>(
            Func<TException, CustomExceptionContext, string> logMessageFunc, 
            Func<TException, CustomExceptionContext, object> extraInformationGetter, 
            HttpStatusCode responseStatusCode, 
            Func<TException, CustomExceptionContext, string> responseMessageFunc, 
            string responseCode, 
            string logLevel = "Warning", 
            string controllerName = null, 
            bool isDefault = false) where TException : Exception
        {
            var item = new ExceptionMapping<TException>
            {
                LogMessage = logMessageFunc,
                ExtraInformationGetter = extraInformationGetter,
                ResponseStatusCode = responseStatusCode,
                ResponseMessage = responseMessageFunc,
                ResponseCode = responseCode,
                ControllerName = controllerName,
                LogLevel = logLevel,
                IsDefault = isDefault
            };

            AddException(item);
        }

        protected virtual void AddException<TException>(ExceptionMapping<TException> item)
            where TException : Exception
        {
            if (Mappings.ContainsKey(typeof(TException)))
            {
                var exceptions = Mappings[typeof(TException)].Mappings;

                var defaultException = exceptions.FirstOrDefault(e => e.IsDefault);

                if(defaultException != null)
                {
                    defaultException.IsDefault = false;
                }

                exceptions.Add(item);
                return;
            }

            var exceptionMappings = new ExceptionMappings();
            exceptionMappings.Mappings.Add(item);
            Mappings.Add(typeof(TException), exceptionMappings);
        }
    }
}
