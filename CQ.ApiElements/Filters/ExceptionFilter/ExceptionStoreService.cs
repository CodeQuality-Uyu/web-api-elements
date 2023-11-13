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
        private readonly IDictionary<OriginError, ExceptionsOfOrigin> _specificExceptions = new Dictionary<OriginError, ExceptionsOfOrigin>();

        private readonly IDictionary<Type, ExceptionResponse> _genericExceptions = new Dictionary<Type, ExceptionResponse>();

        public virtual ExceptionsOfOrigin AddOriginExceptions(OriginError error)
        {
            if (this._specificExceptions.ContainsKey(error)) return this._specificExceptions[error];

            var exceptionsOfOrigin = new ExceptionsOfOrigin();

            this._specificExceptions.Add(error, exceptionsOfOrigin);

            return exceptionsOfOrigin;
        }

        public virtual ExceptionStoreService AddGenericException<TException>(
            string code,
            HttpStatusCode statusCode,
            Func<TException, ExceptionThrownContext, string> messageFunction,
            Func<TException, ExceptionThrownContext, string>? logMessageFunction = null)
            where TException : Exception
        {
            if (this._genericExceptions.ContainsKey(typeof(TException))) return this;

            this._genericExceptions.Add(
                typeof(TException),
                new DinamicExceptionResponse<TException>(
                    code,
                    statusCode,
                    messageFunction,
                    logMessageFunction));

            return this;
        }

        public virtual ExceptionStoreService AddGenericException<TException>(
           Func<TException, ExceptionThrownContext, string> codeFunction,
           HttpStatusCode statusCode,
           Func<TException, ExceptionThrownContext, string> messageFunction,
           Func<TException, ExceptionThrownContext, string>? logMessageFunction = null)
           where TException : Exception
        {
            if (this._genericExceptions.ContainsKey(typeof(TException))) return this;

            this._genericExceptions.Add(
                typeof(TException),
                new DinamicExceptionResponse<TException>(
                    codeFunction,
                    statusCode,
                    messageFunction,
                    logMessageFunction));

            return this;
        }

        public virtual ExceptionStoreService AddGenericException<TException>(
            string code,
            HttpStatusCode statusCode,
            string message,
            string? logMessage = null)
            where TException : Exception
        {
            if (this._genericExceptions.ContainsKey(typeof(TException))) return this;

            this._genericExceptions.Add(
                typeof(TException),
                new ExceptionResponse(
                    code,
                    statusCode,
                    message,
                    logMessage));

            return this;
        }

        public virtual ExceptionResponse HandleException(ExceptionThrownContext context)
        {
            var exception = this.HandleSpecificException(context);

            exception ??= this.HandleTypeException(context);

            exception ??= new("ExceptionOccured", HttpStatusCode.InternalServerError, "An unpredicted exception ocurred");

            return exception;
        }

        private ExceptionResponse? HandleSpecificException(ExceptionThrownContext context)
        {
            var exception = context.Exception;
            var originError = new OriginError(context.ControllerName, context.Action);
            if (!this._specificExceptions.ContainsKey(originError))
            {
                return null;
            }

            var originErrorFound = this._specificExceptions[originError];

            if (!originErrorFound.Exceptions.ContainsKey(exception.GetType()))
            {
                return null;
            }

            var mapping = originErrorFound.Exceptions[exception.GetType()];

            mapping.SetContext(context);

            return mapping;
        }

        private ExceptionResponse? HandleTypeException(ExceptionThrownContext context)
        {
            var exception = context.Exception;
            var registeredType = this.GetRegisteredType(exception.GetType());

            if (registeredType == null) return null;

            var mapping = this._genericExceptions[registeredType];

            mapping.SetContext(context);

            return mapping;
        }

        private Type? GetRegisteredType(Type initialType)
        {
            if (initialType == typeof(Exception))
            {
                return null;
            }

            if (!this._genericExceptions.ContainsKey(initialType))
            {
                return this.GetRegisteredType(initialType.BaseType ?? typeof(Exception));
            }

            return initialType;
        }
    }
}
