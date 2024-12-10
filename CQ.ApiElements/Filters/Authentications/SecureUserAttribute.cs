using CQ.ApiElements.Filters.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Principal;

namespace CQ.ApiElements.Filters.Authentications;
public abstract class SecureUserAttribute
    : BaseAttribute,
    IAsyncAuthorizationFilter
{
    public virtual async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        try
        {
            var accountLogged = context.GetItem<IPrincipal>(ContextItem.AccountLogged);

            var userLogged = await GetUserLoggedAsync(accountLogged).ConfigureAwait(false);

            context.SetItem(
                ContextItem.AccountLogged,
                userLogged);
        }
        catch (Exception ex)
        {
            var error = BuildUnexpectedErrorResponse(ex);
            context.Result = BuildResponse(error);
        }
    }

    protected abstract Task<object> GetUserLoggedAsync(IPrincipal accountLogged);
}
