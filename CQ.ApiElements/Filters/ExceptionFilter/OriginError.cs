namespace CQ.ApiElements.Filters.ExceptionFilter;

public sealed record class OriginError(string ControllerName, string Action);
