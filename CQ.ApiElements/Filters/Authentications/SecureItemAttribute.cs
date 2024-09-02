using CQ.ApiElements.Filters.ExceptionFilter;
using CQ.ApiElements.Filters.Exceptions;
using CQ.ApiElements.Filters.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace CQ.ApiElements.Filters.Authentications;
public class SecureItemAttribute(ContextItems Item)
    : BaseAttribute,
    IAuthorizationFilter
{
    private static IDictionary<Type, ErrorResponse>? _errors;

    internal static IDictionary<Type, ErrorResponse> Errors
    {
        get
        {
            _errors ??= new Dictionary<Type, ErrorResponse>
                {
                    {
                        typeof(ContextItemNotFoundException),
                        new ErrorResponse(
                            HttpStatusCode.Unauthorized,
                            "Unauthenticated",
                            "Item not saved",
                            string.Empty,
                            "Missing item in context related to token sent"
                            )
                    }
                };

            return _errors;
        }
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        try
        {
            context.GetItem(Item);
        }
        catch (Exception ex)
        {
            var exceptionContext = new ExceptionThrownContext(
                context,
                ex,
                string.Empty,
                string.Empty);

            context.Result = BuildErrorResponse(
                Errors,
                exceptionContext);
        }
    }
}
