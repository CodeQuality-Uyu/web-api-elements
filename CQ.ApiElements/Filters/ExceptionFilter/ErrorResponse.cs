using System.Net;
using CQ.Extensions.Environments;
using CQ.Utility;

namespace CQ.ApiElements.Filters.ExceptionFilter;

public record class ErrorResponse()
{
    public HttpStatusCode StatusCode { get; protected set; }

    public string Code { get; protected set; } = null!;

    public string Message { get; protected set; } = null!;

    public string? Description { get; set; }

    public string? Log { get; protected set; }

    public ExceptionMessage? Exception { get; private set; }

    /// <summary>
    /// Hard coded info
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="logMessage"></param>
    /// <param name="description"></param>
    public ErrorResponse(
        HttpStatusCode statusCode,
        string code,
        string message,
        string? logMessage = null,
        string? description = null,
        Exception? exception = null
        )
        : this()
    {
        StatusCode = statusCode;
        Code = code;
        Message = message;
        Log = logMessage;
        Description = description;
        SetException(exception);
    }

    private void SetException(Exception? exception)
    {
        if (EnvironmentExtensions.IsProd() ||
            Guard.IsNull(exception))
        {
            return;
        }

        Exception = new(exception);
    }

    public virtual ErrorResponse CompileErrorResponse(ExceptionThrownContext context)
    {
        SetException(context.Exception);

        return this;
    }
}
