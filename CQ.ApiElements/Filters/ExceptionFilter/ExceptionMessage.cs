using CQ.Utility;

namespace CQ.ApiElements.Filters.ExceptionFilter;

public class ExceptionMessage(Exception exception)
{
    public string Type { get; } = exception.GetType().ToString();

    public string Message { get; } = exception.Message;

    public ExceptionMessage? InnerException { get; } = Guard.IsNotNull(exception.InnerException)
        ? new(exception.InnerException!)
        : null;
}