using CQ.Api.Filters.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CQ.ApiElements.Filters.Extension;
using Microsoft.Extensions.DependencyInjection;

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
            if (string.IsNullOrEmpty(token))
            {
                throw new MissingTokenException();
            }

            await this.AssertTokenFormatAsync(token, context).ConfigureAwait(false);
        }

        private async Task AssertTokenFormatAsync(string token, AuthorizationFilterContext context)
        {
            try
            {
                var isFormatValid = await this.IsFormatOfTokenValidAsync(token, context).ConfigureAwait(false);

                if (!isFormatValid)
                {
                    throw new InvalidTokenException(token);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidTokenException(token, ex);
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
            var permission = this._permission ?? $"{context.HttpContext.Request.Method.ToLower()}-{context.HttpContext.Request.Path.Value.Substring(1)}";

            return permission;
        }

        protected virtual Task<bool> HasUserPermissionAsync(string token, string permission, AuthorizationFilterContext context)
        {
            return Task.FromResult(true);
        }

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


        protected virtual TService GetService<TService>(AuthorizationFilterContext context)
        {
            return context.HttpContext.RequestServices.GetRequiredService<TService>();
        }
    }
}
