using CQ.ApiElements.Filters.ExceptionFilter;
using CQ.ApiElements.Filters.Exceptions;
using CQ.ApiElements.Filters.Extensions;
using CQ.Utility;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace CQ.ApiElements.Filters.Authentications;
public abstract class SecureUserAttribute()
    : BaseAttribute,
    IAsyncAuthorizationFilter
{
    private static IDictionary<Type, ErrorResponse>? _errors;

    internal static IDictionary<Type, ErrorResponse> Errors
    {
        get
        {
            if (Guard.IsNull(_errors))
            {
                var contextItemNotFoundErrorResponse = ValidateItemAttribute.Errors[typeof(ContextItemNotFoundException)];
                _errors = new Dictionary<Type, ErrorResponse>
                {
                    {
                        typeof(ContextItemNotFoundException),
                        contextItemNotFoundErrorResponse
                    },
                    {
                        typeof(Exception),
                        new ErrorResponse(
                            HttpStatusCode.Unauthorized,
                            "Unsync",
                            "User for account not found",
                            string.Empty,
                            "Exist an account but not linked to an existent user"
                            )
                    }
                };
            }
            
            return _errors!;
        }
    }

    public virtual async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        try
        {
            var existErrorOnPrevFilter = context.Result != null;
            if (existErrorOnPrevFilter)
            {
                return;
            }

            context.GetItem(ContextItems.AccountLogged);

            var userLogged = await GetUserLoggedAsync(context).ConfigureAwait(false);

            context.SetItem(
                ContextItems.UserLogged,
                userLogged);
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

    protected abstract Task<object> GetUserLoggedAsync(AuthorizationFilterContext context);

}
