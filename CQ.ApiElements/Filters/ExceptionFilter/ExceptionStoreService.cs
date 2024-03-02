using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters
{
    public sealed class ExceptionStoreService
    {
        public readonly IDictionary<OriginError, ExceptionsOfOrigin> SpecificExceptions = new Dictionary<OriginError, ExceptionsOfOrigin>();

        public readonly IDictionary<Type, ExceptionResponse> GenericExceptions = new Dictionary<Type, ExceptionResponse>();

        public ExceptionsOfOrigin AddOriginExceptions(OriginError error)
        {
            if (this.SpecificExceptions.ContainsKey(error)) return this.SpecificExceptions[error];

            var exceptionsOfOrigin = new ExceptionsOfOrigin();

            this.SpecificExceptions.Add(error, exceptionsOfOrigin);

            return exceptionsOfOrigin;
        }

        public ExceptionStoreService AddGenericException<TException>(
            string code,
            HttpStatusCode statusCode,
            string message,
            string? logMessage = null)
            where TException : Exception
        {
            if (this.GenericExceptions.ContainsKey(typeof(TException))) return this;

            this.GenericExceptions.Add(
                typeof(TException),
                new ExceptionResponse(
                    code,
                    statusCode,
                    message,
                    logMessage));

            return this;
        }

        public ExceptionStoreService AddGenericException<TException>(
            string code,
            HttpStatusCode statusCode,
            Func<TException, ExceptionThrownContext, string> messageFunction,
            Func<TException, ExceptionThrownContext, string>? logMessageFunction = null)
            where TException : Exception
        {
            if (this.GenericExceptions.ContainsKey(typeof(TException))) return this;

            this.GenericExceptions.Add(
                typeof(TException),
                new DinamicExceptionResponse<TException>(
                    code,
                    statusCode,
                    messageFunction,
                    logMessageFunction));

            return this;
        }

        public ExceptionStoreService AddGenericException<TException>(
           Func<TException, ExceptionThrownContext, string> codeFunction,
           HttpStatusCode statusCode,
           Func<TException, ExceptionThrownContext, string> messageFunction,
           Func<TException, ExceptionThrownContext, string>? logMessageFunction = null)
           where TException : Exception
        {
            if (this.GenericExceptions.ContainsKey(typeof(TException))) return this;

            this.GenericExceptions.Add(
                typeof(TException),
                new DinamicExceptionResponse<TException>(
                    codeFunction,
                    statusCode,
                    messageFunction,
                    logMessageFunction));

            return this;
        }

        public ExceptionStoreService AddGenericException<TException>(
           Func<TException, ExceptionThrownContext, string> codeFunction,
           Func<TException, ExceptionThrownContext, HttpStatusCode> statusCodeFunc,
           Func<TException, ExceptionThrownContext, string> messageFunction,
           Func<TException, ExceptionThrownContext, string>? logMessageFunction = null)
           where TException : Exception
        {
            if (this.GenericExceptions.ContainsKey(typeof(TException))) return this;

            this.GenericExceptions.Add(
                typeof(TException),
                new DinamicExceptionResponse<TException>(
                    codeFunction,
                    statusCodeFunc,
                    messageFunction,
                    logMessageFunction));

            return this;
        }

        public ExceptionStoreService AddGenericException<TException>(
            Func<TException, ExceptionThrownContext, (string code, HttpStatusCode statusCode, string message, string? logMessage)> function)
           where TException : Exception
        {
            if(this.GenericExceptions.ContainsKey(typeof(TException))) return this;

            this.GenericExceptions.Add(
                typeof(TException),
                new DinamicExceptionResponse<TException>(function));

            return this;
        }

        public ExceptionResponse HandleException(ExceptionThrownContext context)
        {
            var exception = this.HandleSpecificException(context);

            exception ??= this.HandleTypeException(context);

            exception ??= new("ExceptionOccured", HttpStatusCode.InternalServerError, "An unpredicted exception ocurred");

            exception.SetContext(context);

            return exception;
        }

        private ExceptionResponse? HandleSpecificException(ExceptionThrownContext context)
        {
            var exception = context.Exception;
            var originError = new OriginError(context.ControllerName, context.Action);
            if (!this.SpecificExceptions.ContainsKey(originError))
                return null;

            var originErrorFound = this.SpecificExceptions[originError];

            if (!originErrorFound.Exceptions.ContainsKey(exception.GetType()))
                return null;

            var mapping = originErrorFound.Exceptions[exception.GetType()];

            return mapping;
        }

        private ExceptionResponse? HandleTypeException(ExceptionThrownContext context)
        {
            var exception = context.Exception;
            var registeredType = this.GetRegisteredType(exception.GetType());

            if (registeredType == null)
                return null;

            var mapping = this.GenericExceptions[registeredType];

            return mapping;
        }

        private Type? GetRegisteredType(Type initialType)
        {
            if (initialType == typeof(Exception))
            {
                return null;
            }

            if (!this.GenericExceptions.ContainsKey(initialType))
            {
                return this.GetRegisteredType(initialType.BaseType ?? typeof(Exception));
            }

            return initialType;
        }
    }
}
