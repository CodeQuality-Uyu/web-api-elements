using CQ.ApiElements.Filters.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Principal;

namespace CQ.ApiElements.Filters.Authentications;
public abstract class SecureUserAttribute(ContextItem ContextItem = ContextItem.UserLogged)
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
                ContextItem,
                userLogged);
        }
        catch (Exception ex)
        {
            var error = BuildUnexpectedErrorResponse(ex);
            var response = BuildResponse(error);
            context.Result = response;
        }
    }

    protected abstract Task<object> GetUserLoggedAsync(IPrincipal accountLogged);
}
