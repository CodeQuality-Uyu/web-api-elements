namespace CQ.ApiElements.Filters.ExceptionFilter;

public sealed record OriginError(
    string ControllerName,
    string Action);
