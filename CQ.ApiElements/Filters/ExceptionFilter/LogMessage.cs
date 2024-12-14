namespace CQ.ApiElements.Filters.ExceptionFilter;

internal sealed record LogMessage(
    string Key,
    string Message,
    object? Body = null);
