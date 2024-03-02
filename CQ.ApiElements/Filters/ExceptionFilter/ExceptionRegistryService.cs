using CQ.Exceptions;
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
                .AddGenericException<ResourceNotFoundException>(
                "ResourceNotFound",
                HttpStatusCode.NotFound,
                (exception, context) => $"Resource: {exception.Resource} was not found with parameters: {string.Join(", ", exception.Parameters)}")

                .AddGenericException<ResourceDuplicatedException>(
                "ResourceDuplicated",
                HttpStatusCode.Conflict,
                (exception, context) => $"Resource: {exception.Resource} is duplicated with parameters: {string.Join(", ", exception.Parameters)}")

                .AddGenericException<ArgumentException>(
                    "InvalidArgument",
                    HttpStatusCode.InternalServerError,
                    (exception, context) => $"Invalid argument '{exception.ParamName}'. {exception.Message}")

                .AddGenericException<ArgumentNullException>(
                    "InvalidArgument",
                    HttpStatusCode.InternalServerError,
                    (exception, context) => $"Invalid argument '{exception.ParamName}'. {exception.Message}")


                .AddGenericException<InvalidOperationException>(
                    "InterruptedOperation",
                    HttpStatusCode.InternalServerError,
                    "The operation was interrupted due to an error.")

                .AddGenericException<InvalidRequestException>(
                "InvalidRequest",
                HttpStatusCode.BadRequest,
                (exception, context) => $"Invalid argument '{exception.Prop}'. {exception.InnerException.Message}",
                (exception, context) => $"Invalid argument '{exception.Prop}' with value '{exception.Value}'. {exception.InnerException.Message}"
                );

            RegisterBusinessExceptions(exceptionStoreService);
        }

        protected abstract void RegisterBusinessExceptions(ExceptionStoreService exceptionStoreService);
    }
}
