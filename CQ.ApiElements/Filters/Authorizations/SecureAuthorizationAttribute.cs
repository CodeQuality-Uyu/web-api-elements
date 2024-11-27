using CQ.ApiElements.Filters.ExceptionFilter;
using CQ.ApiElements.Filters.Extensions;
using CQ.Utility;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Security.Principal;

namespace CQ.ApiElements.Filters.Authorizations;

public class SecureAuthorizationAttribute(
    ContextItem ContextItem,
    string? Permission = null)
    : BaseAttribute,
    IAsyncAuthorizationFilter
{
    public virtual async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        try
        {
            var accountLogged = context.GetItemOrDefault(ContextItem);
            var isLogged = context.GetItemOrDefault(ContextItem.IsAuthenticated);

            if(Guard.IsNull(accountLogged) && Guard.IsNotNull(isLogged))
            {
                return;
            }

            if (Guard.IsNull(accountLogged))
            {
                var response = new ErrorResponse(
                HttpStatusCode.Unauthorized,
                "Unauthenticated",
                "Item not saved",
                string.Empty,
                "Missing item in context related to token in Auhtorization header"
                );
                context.Result = BuildResponse(response);
                return;
            }

            var authorizationHeader = context.HttpContext.Request.Headers[HeaderNames.Authorization];
            var (isHeaderAuthorized, permission) = await IsRequestAuthorizedAsync(authorizationHeader, context).ConfigureAwait(false);
            if(!isHeaderAuthorized)
            {
                var errorResponse = new ErrorResponse(
                    HttpStatusCode.Unauthorized,
                    "Unauthorized",
                    "Insufficient permissions",
                    string.Empty,
                    $"You don't have the permission {permission} to access this request",
                    null
                    );

                context.Result = BuildResponse(errorResponse);
                return;
            }
        }
        catch (Exception ex)
        {
            var error = BuildUnexpectedErrorResponse(ex);
            var response = BuildResponse(error);
            context.Result = response;
        }
    }

    #region Assert permission
    private async Task<(bool isAuthorized, string permission)> IsRequestAuthorizedAsync(
        string headerValue,
        AuthorizationFilterContext context)
    {
        var permission = BuildPermission(headerValue, context);

        try
        {
            var isAuthorized = await HasRequestPermissionAsync(headerValue, permission, context).ConfigureAwait(false);

            return (isAuthorized, permission);
        }
        catch (Exception)
        {
            return (false, permission);
        }
    }

    protected virtual string BuildPermission(
        string headerValue,
        AuthorizationFilterContext context)
    {
        return Permission ?? $"{context.RouteData.Values["action"].ToString().ToLower()}-{context.RouteData.Values["controller"].ToString().ToLower()}";
    }

    protected virtual Task<bool> HasRequestPermissionAsync(
        string headerValue,
        string permission,
        AuthorizationFilterContext context)
    {
        var accountLogged = context.GetItem<IPrincipal>(ContextItem);

        var hasPermissionAccount = accountLogged.IsInRole(permission);

        return Task.FromResult(hasPermissionAccount);
    }
    #endregion
}
