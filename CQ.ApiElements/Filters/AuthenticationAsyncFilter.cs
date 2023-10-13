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

namespace CQ.ApiElements.Filters
{
    public class AuthenticationAsyncFilter : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            try
            {
                var token = context.HttpContext.Request.Headers["Authorization"];

                await AssertTokenAsync(token).ConfigureAwait(false);

                await AssertUserPermissionsAsync(token, context).ConfigureAwait(false);
            }
            catch (MissingTokenException ex)
            {
                context.Result = BuildMissingAuthenticationResponse(ex, context);
            }
            catch (TokenIsNotValidException ex)
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

        private async Task AssertTokenAsync(string? token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new MissingTokenException();
            }

            await this.AssertTokenFormatAsync(token).ConfigureAwait(false);
        }

        private async Task AssertTokenFormatAsync(string token)
        {
            var isFormatValid = await this.IsFormatOfTokenValidAsync(token).ConfigureAwait(false);
            
            if (!isFormatValid)
            {
                throw new TokenIsNotValidException();
            }
        }

        protected virtual async Task<bool> IsFormatOfTokenValidAsync(string token)
        {
            return true;
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
            var userPermissions = await this.GetUserPermissionsAsync(token).ConfigureAwait(false);

            var permission = await this.MapRequestToPermissionAsync(context.HttpContext.Request).ConfigureAwait(false);

            var userHasPermission = userPermissions.Contains(permission);

            return (userHasPermission, permission);
        }

        protected virtual async Task<List<string>> GetUserPermissionsAsync(string token)
        {
            return new List<string>() { "generic" };
        }

        protected virtual async Task<string> MapRequestToPermissionAsync(HttpRequest request) { return "generic"; }

        protected virtual IActionResult BuildMissingAuthenticationResponse(MissingTokenException ex, AuthorizationFilterContext context)
        {
            return context.HttpContext.Request.CreateCQErrorResponse(HttpStatusCode.Unauthorized, "Unauthenticated", $"Need to be authenticated");
        }

        protected virtual IActionResult BuildInvalidAuthenticationFormatResponse(TokenIsNotValidException ex, AuthorizationFilterContext context)
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
