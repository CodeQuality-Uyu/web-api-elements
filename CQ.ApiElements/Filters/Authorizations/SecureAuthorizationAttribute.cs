using CQ.ApiElements.Filters.ExceptionFilter;
using CQ.ApiElements.Filters.Extensions;
using CQ.Utility;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Security.Principal;

namespace CQ.ApiElements.Filters.Authorizations;

public class SecureAuthorizationAttribute(
    params string[]? Permission)
    : BaseAttribute,
    IAsyncAuthorizationFilter
{
    public virtual async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        try
        {
            var accountLogged = context.GetItemOrDefault(ContextItem.AccountLogged);

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
            var (isHeaderAuthorized, permissions) = await IsRequestAuthorizedAsync(authorizationHeader, context).ConfigureAwait(false);
            if (!isHeaderAuthorized)
            {
                var errorResponse = new ErrorResponse(
                    HttpStatusCode.Forbidden,
                    "Forbidden",
                    "Insufficient permissions",
                    string.Empty,
                    $"Missing one of the following permissions: {string.Join(", ", permissions)} to access this request",
                    null
                    );

                context.Result = BuildResponse(errorResponse);
                return;
            }
        }
        catch (Exception ex)
        {
            var error = BuildUnexpectedErrorResponse(ex);
            context.Result = BuildResponse(error);
        }
    }

    #region Assert permission
    private async Task<(bool isAuthorized, List<string> permission)> IsRequestAuthorizedAsync(
        string headerValue,
        AuthorizationFilterContext context)
    {
        var permissions = BuildPermissions(headerValue, context);

        try
        {
            var isAuthorized = await HasRequestPermissionAsync(headerValue, permissions, context).ConfigureAwait(false);

            return (isAuthorized, permissions);
        }
        catch (Exception)
        {
            return (false, permissions);
        }
    }

    protected virtual List<string> BuildPermissions(
        string headerValue,
        AuthorizationFilterContext context)
    {
        return Permission?.ToList() ?? [$"{context.RouteData.Values["action"].ToString().ToLower()}-{context.RouteData.Values["controller"].ToString().ToLower()}"];
    }

    protected virtual Task<bool> HasRequestPermissionAsync(
        string headerValue,
        List<string> permissions,
        AuthorizationFilterContext context)
    {
        var accountLogged = context.GetItem<IPrincipal>(ContextItem.AccountLogged);

        var hasPermissionAccount = permissions.Any(accountLogged.IsInRole);

        return Task.FromResult(hasPermissionAccount);
    }
    #endregion
}
