using CQ.ApiElements.Filters.ExceptionFilter;
using CQ.ApiElements.Filters.Exceptions;
using CQ.ApiElements.Filters.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Security.Principal;

namespace CQ.ApiElements.Filters.Authentications;
public abstract class SecureUserAttribute()
    : BaseAttribute,
    IAsyncAuthorizationFilter
{
    internal static IDictionary<Type, ErrorResponse> Errors { get; }= new Dictionary<Type, ErrorResponse>
    {
        {
            typeof(ContextItemNotFoundException),
            ValidateItemAttribute.Errors[typeof(ContextItemNotFoundException)]
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

    public virtual async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        try
        {
            var existErrorOnPrevFilter = context.Result != null;
            if (existErrorOnPrevFilter)
            {
                return;
            }

            var accountLogged = context.GetItem<IPrincipal>(ContextItems.AccountLogged);

            var userLogged = await GetUserLoggedAsync(accountLogged).ConfigureAwait(false);

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

    protected abstract Task<object> GetUserLoggedAsync(IPrincipal accountLogged);

}
