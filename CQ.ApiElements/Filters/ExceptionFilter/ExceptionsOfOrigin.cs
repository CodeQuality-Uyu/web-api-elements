using System.Net;

namespace CQ.ApiElements.Filters.ExceptionFilter;

public sealed record class ExceptionsOfOrigin
{
    public readonly IDictionary<Type, ErrorResponse> Exceptions = new Dictionary<Type, ErrorResponse>();

    public ExceptionsOfOrigin AddException<TException>(
        HttpStatusCode statusCode,
        string code,
        string message,
        string? logMessage = null)
        where TException : Exception
    {
        if (Exceptions.ContainsKey(typeof(TException)))
        {
            return this;
        }

        Exceptions.Add(
            typeof(TException),
            new ErrorResponse(
                statusCode,
                code,
                message,
                logMessage));

        return this;
    }

    public ExceptionsOfOrigin AddException<TException>(
        HttpStatusCode statusCode,
        string code,
        Func<TException, ExceptionThrownContext, string> messageFunction,
        Func<TException, ExceptionThrownContext, string>? logMessageFunction = null)
        where TException : Exception
    {
        if (Exceptions.ContainsKey(typeof(TException)))
        {
            return this;
        }

        Exceptions.Add(
            typeof(TException),
            new DynamicErrorResponse<TException>(
                statusCode,
                code,
                messageFunction,
                logMessageFunction));

        return this;
    }


    public ExceptionsOfOrigin AddException<TException>(
        Func<TException,
            ExceptionThrownContext,
            (HttpStatusCode statusCode,
            string code,
            string message,
            string? logMessage)> function)
        where TException : Exception
    {
        if (Exceptions.ContainsKey(typeof(TException)))
        {
            return this;
        }

        Exceptions.Add(
            typeof(TException),
            new DynamicErrorResponse<TException>(function));

        return this;
    }
}
