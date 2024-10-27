using CQ.ApiElements.Filters.Authentications;
using CQ.ApiElements.Filters.ExceptionFilter;
using CQ.ApiElements.Filters.Exceptions;
using CQ.ApiElements.Filters.Extensions;
using CQ.Exceptions;
using CQ.Utility;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Security.Principal;

namespace CQ.ApiElements.Filters.Authorizations;

public class SecureAuthorizationAttribute(
    string? _permission = null)
    : BaseAttribute,
    IAsyncAuthorizationFilter
{
    internal static IDictionary<Type, ErrorResponse>? Errors { get; } = new Dictionary<Type, ErrorResponse>
                {
                    {
                        typeof(ContextItemNotFoundException),
                        SecureItemAttribute.Errors[typeof(ContextItemNotFoundException)]
                    },
                    {
                        typeof(AccessDeniedException),
                        new DynamicErrorResponse<AccessDeniedException>(
                        HttpStatusCode.Forbidden,
                        "AccessDenied",
                        (ex, context) => $"Missing permission {ex.Permission}"
                            )
                    },
                };

    public virtual async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        try
        {
            var accountLogged = context.GetItemOrDefault(ContextItems.AccountLogged);
            var systemLogged = context.GetItemOrDefault(ContextItems.ClientSystemLogged);

            if (Guard.IsNull(accountLogged) && Guard.IsNull(systemLogged))
            {
                ContextItemNotFoundException.Throw(ContextItems.AccountLogged);
            }

            if (Guard.IsNotNull(accountLogged))
            {
                var authorizationHeader = context.HttpContext.Request.Headers[HeaderNames.Authorization];
                await AssertRequestPermissionsAsync(authorizationHeader, context).ConfigureAwait(false);

                return;
            }

            var privateKeyHeader = context.HttpContext.Request.Headers["PrivateKey"];
            await AssertRequestPermissionsAsync(privateKeyHeader, context).ConfigureAwait(false);
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

    #region Assert permission
    private async Task AssertRequestPermissionsAsync(
        string headerValue,
        AuthorizationFilterContext context)
    {
        var (isHeaderAuthorized, permission) = await IsRequestAuthorizedAsync(headerValue, context).ConfigureAwait(false);

        if (!isHeaderAuthorized)
        {
            AccessDeniedException.Throw(permission);
        }
    }

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
        return _permission ?? $"{context.RouteData.Values["action"].ToString().ToLower()}-{context.RouteData.Values["controller"].ToString().ToLower()}";
    }

    protected virtual Task<bool> HasRequestPermissionAsync(
        string headerValue,
        string permission,
        AuthorizationFilterContext context)
    {
        var accountLogged = context.GetItem<IPrincipal>(ContextItems.AccountLogged);

        var hasPermissionAccount = accountLogged.IsInRole(permission);

        return Task.FromResult(hasPermissionAccount);
    }
    #endregion
}
