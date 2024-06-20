using CQ.Exceptions;
using System.Net;

namespace CQ.ApiElements.Filters;
public class ExceptionStoreService
{
    public readonly IDictionary<OriginError, ExceptionsOfOrigin> SpecificExceptions = new Dictionary<OriginError, ExceptionsOfOrigin>();

    public readonly IDictionary<Type, ExceptionResponse> GenericExceptions = new Dictionary<Type, ExceptionResponse>();

    public ExceptionStoreService()
    {
        AddGenericException<ResourceNotFoundException>(
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

        RegisterBusinessExceptions();
    }

    public ExceptionsOfOrigin AddOriginExceptions(OriginError error)
    {
        if (this.SpecificExceptions.ContainsKey(error))
        {
            return this.SpecificExceptions[error];
        }

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
        if (this.GenericExceptions.ContainsKey(typeof(TException)))
        {
            return this;
        }

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
        if (this.GenericExceptions.ContainsKey(typeof(TException)))
        {
            return this;
        }

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
        if (this.GenericExceptions.ContainsKey(typeof(TException)))
        {
            return this;
        }

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
        if (this.GenericExceptions.ContainsKey(typeof(TException)))
        {
            return this;
        }

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
        if (this.GenericExceptions.ContainsKey(typeof(TException)))
        {
            return this;
        }

        this.GenericExceptions.Add(
            typeof(TException),
            new DinamicExceptionResponse<TException>(function));

        return this;
    }

    public ExceptionResponse HandleException(ExceptionThrownContext context)
    {
        var exception = HandleSpecificException(context);

        exception ??= HandleTypeException(context);

        exception ??= new("ExceptionOccured", HttpStatusCode.InternalServerError, "An unpredicted exception ocurred");

        exception.SetContext(context);

        return exception;
    }

    private ExceptionResponse? HandleSpecificException(ExceptionThrownContext context)
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

    private ExceptionResponse? HandleTypeException(ExceptionThrownContext context)
    {
        var exception = context.Exception;
        var registeredType = this.GetRegisteredType(exception.GetType());

        if (registeredType == null)
        {
            return null;
        }

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

    protected virtual void RegisterBusinessExceptions()
    {
    }
}
