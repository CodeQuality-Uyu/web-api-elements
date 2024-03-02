using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using CQ.Exceptions;
using CQ.ApiElements.Filters.Extensions;

namespace CQ.ApiElements.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthenticationAsyncAttributeFilter : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string? _permission;

        public AuthenticationAsyncAttributeFilter() { }

        public AuthenticationAsyncAttributeFilter(string permission)
        {
            _permission = permission;
        }

        public virtual async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            try
            {
                var token = context.HttpContext.Request.Headers["Authorization"];

                await AssertTokenAsync(token, context).ConfigureAwait(false);

                var accountLogged = context.HttpContext.Items[ContextItems.AccountLogged];

                if (accountLogged == null)
                {
                    accountLogged = await GetAccountByTokenAsync(token, context).ConfigureAwait(false);

                    context.HttpContext.Items[ContextItems.AccountLogged] = accountLogged;
                }

                await AssertUserPermissionsAsync(token, context).ConfigureAwait(false);
            }
            catch (MissingTokenException ex)
            {
                context.Result = BuildMissingAuthenticationResponse(ex, context);
            }
            catch (InvalidTokenException ex)
            {
                context.Result = BuildInvalidAuthenticationFormatResponse(ex, context);
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

        private async Task AssertTokenAsync(string? token, AuthorizationFilterContext context)
        {
            if (AccountIsLogged(context))
                return;

            if (string.IsNullOrEmpty(token))
            {
                throw new MissingTokenException();
            }

            await this.AssertTokenFormatAsync(token, context).ConfigureAwait(false);
        }

        private bool AccountIsLogged(AuthorizationFilterContext context)
        {
            return context.HttpContext.Items[ContextItems.AccountLogged] != null;
        }

        private async Task AssertTokenFormatAsync(string token, AuthorizationFilterContext context)
        {
            bool isFormatValid;
            try
            {
                isFormatValid = await this.IsFormatOfTokenValidAsync(token, context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new InvalidTokenException(token, ex);
            }

            if (!isFormatValid)
            {
                throw new InvalidTokenException(token);
            }
        }

        protected virtual Task<bool> IsFormatOfTokenValidAsync(string token, AuthorizationFilterContext context)
        {
            return Task.FromResult(true);
        }

        private async Task AssertUserPermissionsAsync(string token, AuthorizationFilterContext context)
        {
            var (isUserAuthorized, permission) = await this.IsUserAuthorizedAsync(token, context).ConfigureAwait(false);

            if (!isUserAuthorized)
            {
                throw new AccessDeniedException(permission);
            }
        }

        protected virtual async Task<(bool isAuthorized, string permission)> IsUserAuthorizedAsync(string token, AuthorizationFilterContext context)
        {
            var permission = this.BuildPermission(token, context);

            try
            {
                var isAuthorized = await this.HasUserPermissionAsync(token, permission, context).ConfigureAwait(false);

                return (isAuthorized, permission);
            }
            catch (Exception)
            {
                return (false, permission);
            }
        }

        protected virtual string BuildPermission(string token, AuthorizationFilterContext context)
        {
            return _permission ?? $"{context.RouteData.Values["action"].ToString().ToLower()}-{context.RouteData.Values["controller"].ToString().ToLower()}";
        }

        protected virtual Task<bool> HasUserPermissionAsync(string token, string permission, AuthorizationFilterContext context)
        {
            return Task.FromResult(true);
        }

        protected virtual Task<object> GetAccountByTokenAsync(string token, AuthorizationFilterContext context)
        {
            return Task.FromResult<object>(null);
        }

        #region Build responses
        protected virtual IActionResult BuildMissingAuthenticationResponse(MissingTokenException ex, AuthorizationFilterContext context)
        {
            return context.HttpContext.Request.CreateCQErrorResponse(HttpStatusCode.Unauthorized, "Unauthenticated", $"Need to be authenticated");
        }

        protected virtual IActionResult BuildInvalidAuthenticationFormatResponse(InvalidTokenException ex, AuthorizationFilterContext context)
        {
            return context.HttpContext.Request.CreateCQErrorResponse(HttpStatusCode.Forbidden, "InvalidTokenFormat", $"Invalid format of token");
        }

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
