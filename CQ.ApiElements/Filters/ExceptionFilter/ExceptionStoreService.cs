using CQ.Exceptions;
using System.Net;

namespace CQ.ApiElements.Filters.ExceptionFilter;
public class ExceptionStoreService
{
    public readonly IDictionary<OriginError, ExceptionsOfOrigin> SpecificExceptions = new Dictionary<OriginError, ExceptionsOfOrigin>();

    public readonly IDictionary<Type, ErrorResponse> GenericExceptions = new Dictionary<Type, ErrorResponse>();

    public ExceptionStoreService()
    {
        AddGenericException<ResourceNotFoundException>(
            HttpStatusCode.NotFound,
            "ResourceNotFound",
            (exception, context) => $"Resource: {exception.Resource} was not found with parameters: {string.Join(", ", exception.Parameters)}")
            
            .AddGenericException<ResourceDuplicatedException>(
            HttpStatusCode.Conflict,
            "ResourceDuplicated",
            (exception, context) => $"Resource: {exception.Resource} is duplicated with parameters: {string.Join(", ", exception.Parameters)}")

            .AddGenericException<ArgumentException>(
                HttpStatusCode.InternalServerError,
                "InvalidArgument",
                (exception, context) => $"Invalid argument '{exception.ParamName}'. {exception.Message}")

            .AddGenericException<ArgumentNullException>(
                HttpStatusCode.InternalServerError,
                "InvalidArgument",
                (exception, context) => $"Invalid argument '{exception.ParamName}'. {exception.Message}")

            .AddGenericException<InvalidOperationException>(
                HttpStatusCode.InternalServerError,
                "InterruptedOperation",
                "The operation was interrupted due to an error.")

            .AddGenericException<InvalidRequestException>(
            HttpStatusCode.BadRequest,
            "InvalidRequest",
            (exception, context) => $"Invalid argument '{exception.Prop}'. {exception.InnerException.Message}",
            (exception, context) => $"Invalid argument '{exception.Prop}' with value '{exception.Value}'. {exception.InnerException.Message}"
            );

        RegisterBusinessExceptions();
    }

    public ExceptionsOfOrigin AddOriginExceptions(OriginError error)
    {
        if (SpecificExceptions.TryGetValue(error, out var value))
        {
            return value;
        }

        var exceptionsOfOrigin = new ExceptionsOfOrigin();

        SpecificExceptions.Add(error, exceptionsOfOrigin);

        return exceptionsOfOrigin;
    }

    public ExceptionStoreService AddGenericException<TException>(
        HttpStatusCode statusCode,
        string code,
        string message,
        string? logMessage = null)
        where TException : Exception
    {
        if (GenericExceptions.ContainsKey(typeof(TException)))
        {
            return this;
        }

        GenericExceptions.Add(
            typeof(TException),
            new ErrorResponse(
                statusCode,
                code,
                message,
                logMessage));

        return this;
    }

    public ExceptionStoreService AddGenericException<TException>(
        HttpStatusCode statusCode,
        string code,
        Func<TException, ExceptionThrownContext, string> messageFunction,
        Func<TException, ExceptionThrownContext, string>? logMessageFunction = null)
        where TException : Exception
    {
        if (GenericExceptions.ContainsKey(typeof(TException)))
        {
            return this;
        }

        GenericExceptions.Add(
            typeof(TException),
            new DynamicErrorResponse<TException>(
                statusCode,
                code,
                messageFunction,
                logMessageFunction));

        return this;
    }

    public ExceptionStoreService AddGenericException<TException>(
       HttpStatusCode statusCode,
       Func<TException, ExceptionThrownContext, string> codeFunction,
       Func<TException, ExceptionThrownContext, string> messageFunction,
       Func<TException, ExceptionThrownContext, string>? logMessageFunction = null)
       where TException : Exception
    {
        if (GenericExceptions.ContainsKey(typeof(TException)))
        {
            return this;
        }

        GenericExceptions.Add(
            typeof(TException),
            new DynamicErrorResponse<TException>(
                statusCode,
                codeFunction,
                messageFunction,
                logMessageFunction));

        return this;
    }

    public ExceptionStoreService AddGenericException<TException>(
       Func<TException, ExceptionThrownContext, HttpStatusCode> statusCodeFunc,
       Func<TException, ExceptionThrownContext, string> codeFunction,
       Func<TException, ExceptionThrownContext, string> messageFunction,
       Func<TException, ExceptionThrownContext, string>? logMessageFunction = null)
       where TException : Exception
    {
        if (GenericExceptions.ContainsKey(typeof(TException)))
        {
            return this;
        }

        GenericExceptions.Add(
            typeof(TException),
            new DynamicErrorResponse<TException>(
                statusCodeFunc,
                codeFunction,
                messageFunction,
                logMessageFunction));

        return this;
    }

    public ExceptionStoreService AddGenericException<TException>(Func<
        TException,
        ExceptionThrownContext,
        (HttpStatusCode statusCode,
        string code,
        string message,
        string? logMessage)> function)
       where TException : Exception
    {
        if (GenericExceptions.ContainsKey(typeof(TException)))
        {
            return this;
        }

        GenericExceptions.Add(
            typeof(TException),
            new DynamicErrorResponse<TException>(function));

        return this;
    }

    public ErrorResponse? HandleException(ExceptionThrownContext context)
    {
        var exception = HandleSpecificException(context);

        exception ??= HandleTypeException(context);

        exception?.CompileErrorResponse(context);

        return exception;
    }

    private ErrorResponse? HandleSpecificException(ExceptionThrownContext context)
    {
        var exception = context.Exception;
        var originError = new OriginError(context.ControllerName, context.Action);
        if (!SpecificExceptions.ContainsKey(originError))
        {
            return null;
        }

        var originErrorFound = SpecificExceptions[originError];

        if (!originErrorFound.Exceptions.ContainsKey(exception.GetType()))
        {
            return null;
        }

        var mapping = originErrorFound.Exceptions[exception.GetType()];

        return mapping;
    }

    private ErrorResponse? HandleTypeException(ExceptionThrownContext context)
    {
        var exception = context.Exception;
        var registeredType = GetRegisteredType(exception.GetType());

        if (registeredType == null)
        {
            return null;
        }

        var mapping = GenericExceptions[registeredType];

        return mapping;
    }

    private Type? GetRegisteredType(Type initialType)
    {
        if (initialType == typeof(Exception))
        {
            return null;
        }

        if (!GenericExceptions.ContainsKey(initialType))
        {
            return GetRegisteredType(initialType.BaseType ?? typeof(Exception));
        }

        return initialType;
    }

    protected virtual void RegisterBusinessExceptions()
    {
    }
}
