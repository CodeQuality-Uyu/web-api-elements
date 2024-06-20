using CQ.ApiElements.Filters.Extensions;
using CQ.Utility;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace CQ.ApiElements.Filters.Authentications;
public abstract class SecureUserAttribute : BaseAttribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.Result != null)
        {
            return;
        }

        var existAccountLogged = base.GetItem(context, ContextItems.AccountLogged) != null;

        if (!existAccountLogged)
        {
            context.Result = context.HttpContext.Request.CreateCQErrorResponse(
            HttpStatusCode.Unauthorized,
            "Unauthenticated",
            $"Missing header Authorization or PrivateKey");
            return;
        }

        var userLogged = await GetUserLoggedAsync(context).ConfigureAwait(false);

        if (Guard.IsNull(userLogged))
        {
            context.Result = context.HttpContext.Request.CreateCQErrorResponse(
           HttpStatusCode.Unauthorized,
           "Unsync",
           $"Missing user for account");

            return;
        }

        base.SetItem(context, ContextItems.UserLogged, userLogged);
    }

    protected abstract Task<object> GetUserLoggedAsync(AuthorizationFilterContext context);

}
