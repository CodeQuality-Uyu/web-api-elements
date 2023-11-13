using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters
{
    public record class ExceptionsOfOrigin
    {
        public readonly IDictionary<Type, ExceptionResponse> Exceptions = new Dictionary<Type, ExceptionResponse>();

        public virtual ExceptionsOfOrigin AddException<TException>(
            string code,
            HttpStatusCode statusCode,
            Func<TException, ExceptionThrownContext, string> messageFunction,
            Func<TException, ExceptionThrownContext, string>? logMessageFunction = null)
            where TException : Exception
        {
            if (this.Exceptions.ContainsKey(typeof(TException))) return this;

            this.Exceptions.Add(
                typeof(TException),
                new DinamicExceptionResponse<TException>(
                    code,
                    statusCode,
                    messageFunction,
                    logMessageFunction));

            return this;
        }

        public virtual ExceptionsOfOrigin AddException<TException>(
            string code,
            HttpStatusCode statusCode,
            string message,
            string? logMessage = null)
            where TException : Exception
        {
            if (this.Exceptions.ContainsKey(typeof(TException))) return this;

            this.Exceptions.Add(
                typeof(TException),
                new ExceptionResponse(
                    code,
                    statusCode,
                    message,
                    logMessage));

            return this;
        }
    }
}
