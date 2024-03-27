using CQ.ApiElements.Filters.Authentications;
using CQ.ApiElements.Filters.Extensions;
using CQ.Exceptions;
using CQ.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters.Authorizations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public abstract class SecureAuthorizationAsyncAttributeFilter : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string? _permission;

        private readonly SecureAuthenticationAsyncAttributeFilter _authenticationFilter;

        public SecureAuthorizationAsyncAttributeFilter(
            SecureAuthenticationAsyncAttributeFilter authenticationFilter,
            string? permission = null)
        {
            _permission = permission;
            _authenticationFilter = authenticationFilter;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            await _authenticationFilter.OnAuthorizationAsync(context).ConfigureAwait(false);

            var existAuthenticationError = context.Result != null;
            if (existAuthenticationError)
                return;

            var accountIsLogged = context.HttpContext.Items[ContextItems.AccountLogged] != null;

            try
            {
                if (accountIsLogged)
                {
                    var authorizationHeader = context.HttpContext.Request.Headers["Authorization"];
                    await AssertRequestPermissionsAsync(authorizationHeader, context).ConfigureAwait(false);

                    return;
                }

                var privateKeyHeader = context.HttpContext.Request.Headers["PrivateKey"];
                await AssertRequestPermissionsAsync(privateKeyHeader, context).ConfigureAwait(false);
            }
            catch (AccessDeniedException ex)
            {
                context.Result = BuildUnauthorizedResponse(ex, context);
            }
            catch (ArgumentNullException ex)
            {
                context.Result = BuldInvalidArgumentResponse(ex, context);
            }
            catch (Exception)
            {
                context.Result = BuildGenericResponse(context);
            }
        }

        #region Assert permission
        private async Task AssertRequestPermissionsAsync(string headerValue, AuthorizationFilterContext context)
        {
            var (isHeaderAuthorized, permission) = await IsRequestAuthorizedAsync(headerValue, context).ConfigureAwait(false);

            if (!isHeaderAuthorized)
                throw new AccessDeniedException(permission);
        }

        private async Task<(bool isAuthorized, string permission)> IsRequestAuthorizedAsync(string headerValue, AuthorizationFilterContext context)
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

        protected virtual string BuildPermission(string headerValue, AuthorizationFilterContext context)
        {
            return _permission ?? $"{context.RouteData.Values["action"].ToString().ToLower()}-{context.RouteData.Values["controller"].ToString().ToLower()}";
        }

        protected abstract Task<bool> HasRequestPermissionAsync(string headerValue, string permission, AuthorizationFilterContext context);
        #endregion

        #region Build responses
        protected virtual IActionResult BuildUnauthorizedResponse(AccessDeniedException ex, AuthorizationFilterContext context)
        {
            return context.HttpContext.Request.CreateCQErrorResponse(HttpStatusCode.Forbidden, "AccessDenied", $"Missing permission {ex.Permission}");
        }

        protected virtual IActionResult BuldInvalidArgumentResponse(ArgumentNullException ex, AuthorizationFilterContext context)
        {
            return context.HttpContext.Request.CreateCQErrorResponse(HttpStatusCode.BadRequest, "RequestInvalid", $"Missing or invalid {ex.ParamName}");
        }

        protected virtual IActionResult BuildGenericResponse(AuthorizationFilterContext context)
        {
            return context.HttpContext.Request.CreateCQErrorResponse(HttpStatusCode.InternalServerError, "InternalProblem", "Problems with the server");
        }
        #endregion

        protected virtual TService GetService<TService>(AuthorizationFilterContext context)
        {
            return context.HttpContext.RequestServices.GetRequiredService<TService>();
        }
    }
}
