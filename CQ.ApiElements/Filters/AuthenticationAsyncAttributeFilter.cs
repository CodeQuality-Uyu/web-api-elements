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
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
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
            var isFormatValid = await this.IsFormatOfTokenValidAsync(token, context).ConfigureAwait(false);
            
            if (!isFormatValid)
            {
                throw new InvalidTokenException();
            }
        }

        protected virtual bool IsFormatOfTokenValid(string token, AuthorizationFilterContext context)
        {
            return true;
        }

        protected virtual Task<bool> IsFormatOfTokenValidAsync(string token, AuthorizationFilterContext context)
        {
            var result = IsFormatOfTokenValid(token, context);

            return Task.FromResult(result);
        }

        private async Task AssertUserPermissionsAsync(string token, AuthorizationFilterContext context)
        {
            var (isUserAuthorized, permission) = await this.IsUserAuthorizedAsync(token, context).ConfigureAwait(false);

            if (!isUserAuthorized)
            {
                throw new AccessDeniedException(permission);
            }
        }

        protected virtual (bool IsAuthorized, string Permission) IsUserAuthorized(string token, AuthorizationFilterContext context)
        {
            return (true, "permission");
        }

        protected virtual Task<(bool isAuthorized, string permission)> IsUserAuthorizedAsync(string token, AuthorizationFilterContext context)
        {
            var result = IsUserAuthorized(token, context);

            return Task.FromResult(result);
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
    }
}
