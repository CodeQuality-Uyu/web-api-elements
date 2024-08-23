using CQ.Utility;

namespace CQ.ApiElements.Filters.ExceptionFilter;

public class ExceptionBody(Exception exception)
{
    public string Type { get; init; } = exception.GetType().ToString();

    public string Message { get; init; } = exception.Message;

    public ExceptionBody? InnerException { get; init; } = Guard.IsNotNull(exception.InnerException)
        ? new(exception.InnerException!)
        : null;
}
