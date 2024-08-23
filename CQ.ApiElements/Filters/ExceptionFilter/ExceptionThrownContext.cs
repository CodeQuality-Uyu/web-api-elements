using Microsoft.AspNetCore.Mvc.Filters;

namespace CQ.ApiElements.Filters.ExceptionFilter;

public record class ExceptionThrownContext(
    FilterContext ExceptionContext,
    Exception Exception,
    string ControllerName,
    string Action);
