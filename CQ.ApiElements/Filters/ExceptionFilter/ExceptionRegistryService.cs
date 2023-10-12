using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters
{
    public abstract class ExceptionRegistryService
    {
        public void RegisterKnownExceptions(ExceptionStoreService exceptionStoreService)
        {
            exceptionStoreService.RegisterMapping<ArgumentNullException>(
                (exception, context) =>
            {
                var customException = exception as ArgumentNullException;
                if (customException == null)
                {
                    return "Invalid argument";
                }

                return $"Missing or invalid {customException.ParamName}";
            }, 
                (exception, context) => new { Prop = exception.ParamName }, 
                HttpStatusCode.BadRequest, 
                (exception, context) =>
            {
                var customException = exception as ArgumentNullException;
                if (customException == null)
                {
                    return "Invalid argument";
                }

                return $"Missing or invalid {customException.ParamName}";
            },
                "InvalidArgument",
                isDefault: true);

            exceptionStoreService.RegisterMapping<ArgumentException>(
                (exception, context) =>
                {
                    var customException = exception as ArgumentException;
                    if (customException == null || string.IsNullOrEmpty(customException.Message))
                    {
                        return "Invalid argument";
                    }

                    return customException.Message;
                },
                (exception, context) => new { Prop = exception.ParamName },
                HttpStatusCode.BadRequest,
                (exception, context) =>
                {
                    var customException = exception as ArgumentException;
                    if (customException == null || string.IsNullOrEmpty(customException.Message))
                    {
                        return "Invalid argument";
                    }

                    return customException.Message;
                },
                "InvalidArgument",
                isDefault: true);

            RegisterBusinessExceptions(exceptionStoreService);
        }

        protected abstract void RegisterBusinessExceptions(ExceptionStoreService exceptionStoreService);
    }
}
