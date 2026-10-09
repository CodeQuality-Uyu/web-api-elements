using CQ.ApiElements.Filters.ExceptionFilter;
using CQ.ApiElements.Filters.Extensions;
using CQ.Utility;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Security.Principal;

namespace CQ.ApiElements.Filters.Authorizations;

public class SecureAuthorizationAttribute(
    params string[] permissions)
    : BaseAttribute,
    IAsyncAuthorizationFilter
{
    public virtual async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        try
        {
            var accountLogged = context.GetItemOrDefault<IPrincipal>(ContextItem.AccountLogged);

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
            var (isHeaderAuthorized, permissions) = await IsRequestAuthorizedAsync(accountLogged, context).ConfigureAwait(false);
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
        IPrincipal accountLogged,
        AuthorizationFilterContext context)
    {
        var permissions = BuildPermissions(context);

        try
        {
            var isAuthorized = await HasRequestPermissionAsync(
                permissions,
                accountLogged,
                context).ConfigureAwait(false);

            return (isAuthorized, permissions);
        }
        catch (Exception)
        {
            return (false, permissions);
        }
    }

    protected virtual List<string> BuildPermissions(
        AuthorizationFilterContext context)
    {
        if (permissions.Length != 0)
        {
            return permissions.ToList();
        }

        return [$"{context.RouteData.Values["action"].ToString().ToLower()}-{context.RouteData.Values["controller"].ToString().ToLower()}"];
    }

    protected virtual Task<bool> HasRequestPermissionAsync(
        List<string> permissions,
        IPrincipal accountLogged,
        AuthorizationFilterContext context)
    {
        var hasPermissionAccount = permissions.Any(accountLogged.IsInRole);

        return Task.FromResult(hasPermissionAccount);
    }
    #endregion
}
