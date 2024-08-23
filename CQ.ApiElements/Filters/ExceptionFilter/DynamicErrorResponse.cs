using System.Net;

namespace CQ.ApiElements.Filters.ExceptionFilter;

internal sealed record class DynamicErrorResponse<TException>
    : ErrorResponse
    where TException : Exception
{
    private readonly Func<TException, ExceptionThrownContext, string>? _messageFunction;

    private readonly Func<TException, ExceptionThrownContext, string>? _logMessageFunction;

    private readonly Func<TException, ExceptionThrownContext, string>? _codeFunction;

    private readonly Func<TException, ExceptionThrownContext, HttpStatusCode>? _statusCodeFunction;

    private readonly Func<TException, ExceptionThrownContext, (HttpStatusCode statusCode, string code, string message, string? logMessage)>? _function;

    /// <summary>
    /// Message and log message dynamic
    /// </summary>
    /// <param name="code"></param>
    /// <param name="statusCode"></param>
    /// <param name="messageFunction"></param>
    /// <param name="logMessageFunction"></param>
    public DynamicErrorResponse(
        HttpStatusCode statusCode,
        string code,
        Func<TException, ExceptionThrownContext, string> messageFunction,
        Func<TException, ExceptionThrownContext, string>? logMessageFunction = null)
        : base(
              statusCode,
              code,
              string.Empty,
              string.Empty)
    {
        _messageFunction = messageFunction;
        _logMessageFunction = logMessageFunction ?? messageFunction;
    }

    /// <summary>
    /// Internal code, message and log message dynamic
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="codeFunction"></param>
    /// <param name="messageFunction"></param>
    /// <param name="logMessageFunction"></param>
    public DynamicErrorResponse(
        HttpStatusCode statusCode,
        Func<TException, ExceptionThrownContext, string> codeFunction,
        Func<TException, ExceptionThrownContext, string> messageFunction,
        Func<TException, ExceptionThrownContext, string>? logMessageFunction = null)
        : base(
              statusCode,
              string.Empty,
              string.Empty,
              string.Empty)
    {
        _codeFunction = codeFunction;
        _messageFunction = messageFunction;
        _logMessageFunction = logMessageFunction ?? messageFunction;
    }

    /// <summary>
    /// All dynamic
    /// </summary>
    /// <param name="codeFunction"></param>
    /// <param name="statusCodeFunction"></param>
    /// <param name="messageFunction"></param>
    /// <param name="logMessageFunction"></param>
    public DynamicErrorResponse(
        Func<TException, ExceptionThrownContext, HttpStatusCode> statusCodeFunction,
        Func<TException, ExceptionThrownContext, string> codeFunction,
        Func<TException, ExceptionThrownContext, string> messageFunction,
        Func<TException, ExceptionThrownContext, string>? logMessageFunction = null)
    {
        _codeFunction = codeFunction;
        _statusCodeFunction = statusCodeFunction;
        _messageFunction = messageFunction;
        _logMessageFunction = logMessageFunction ?? messageFunction;
    }

    /// <summary>
    /// All dynamic
    /// </summary>
    /// <param name="function"></param>
    public DynamicErrorResponse(Func<
        TException,
        ExceptionThrownContext,
        (HttpStatusCode statusCode,
        string code,
        string message,
        string? logMessage)> function)
    {
        _function = function;
    }

    public override ErrorResponse CompileErrorResponse(ExceptionThrownContext context)
    {
        base.CompileErrorResponse(context);

        Code = _codeFunction != null ? BuildElement(_codeFunction, context) : Code;
        StatusCode = _statusCodeFunction != null ? BuildElement(_statusCodeFunction, context) : StatusCode;
        Message = _messageFunction != null ? BuildElement(_messageFunction, context) : Message;
        LogMessage = _logMessageFunction != null ? BuildElement(_logMessageFunction, context) : LogMessage;
        (StatusCode, Code, Message, LogMessage) = _function != null
            ? BuildElement(_function, context)
            : (
            StatusCode,
            Code,
            Message,
            LogMessage);

        return this;
    }

    private static TElement BuildElement<TElement>(
        Func<TException, ExceptionThrownContext, TElement> function,
        ExceptionThrownContext context)
    {
        var concreteException = (TException)context.Exception;

        return function(concreteException, context);
    }
}
