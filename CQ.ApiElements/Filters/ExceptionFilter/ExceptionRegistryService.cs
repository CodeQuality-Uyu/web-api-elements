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
            exceptionStoreService
                .AddGenericException<ArgumentException>(
                    "InvalidArgument",
                    HttpStatusCode.InternalServerError,
                    (ArgumentException exception,
                    ExceptionThrownContext context) => $"Invalid argument '{exception.ParamName}'. {exception.Message}")

                .AddGenericException<ArgumentNullException>(
                    "InvalidArgument",
                    HttpStatusCode.InternalServerError,
                    (ArgumentNullException exception,
                    ExceptionThrownContext context) => $"Invalid argument '{exception.ParamName}'. {exception.Message}")


                .AddGenericException<InvalidOperationException>(
                    "InterruptedOperation",
                    HttpStatusCode.InternalServerError,
                    "The operation was interrupted due to an exception.");

            RegisterBusinessExceptions(exceptionStoreService);
        }

        protected abstract void RegisterBusinessExceptions(ExceptionStoreService exceptionStoreService);
    }
}
