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
            exceptionStoreService.RegisterException(
                new DinamicExceptionMapping<ArgumentNullException>
                (
                    (exception, context) =>
                    {
                        if (string.IsNullOrEmpty(exception.Message))
                        {
                            return $"Missing or invalid {exception.ParamName}";
                        }

                        return exception.Message;
                    },
                   "InvalidArgument",
                    HttpStatusCode.BadRequest
                ));

            exceptionStoreService.RegisterException(
                new DinamicExceptionMapping<ArgumentException>
                (
                    (exception, context) =>
                    {
                        if (string.IsNullOrEmpty(exception.Message))
                        {
                            return "Invalid argument";
                        }

                        return exception.Message;
                    },
                    "InvalidArgument",
                    HttpStatusCode.BadRequest
                ));

            RegisterBusinessExceptions(exceptionStoreService);
        }

        protected abstract void RegisterBusinessExceptions(ExceptionStoreService exceptionStoreService);
    }
}
