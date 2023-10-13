using CQ.Api.Filters.Exceptions;
using CQ.ApiElements.Filters.Extension;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters
{
    public class AuthenticationFilter : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                var token = context.HttpContext.Request.Headers["Authorization"];

                AssertToken(token);

                AssertUserPermissions(token, context);
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

        private void AssertToken(string? token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new MissingTokenException();
            }

            this.AssertTokenFormat(token);
        }

        private void AssertTokenFormat(string token)
        {
            if (!this.IsFormatOfTokenValid(token))
            {
                throw new TokenIsNotValidException();
            }
        }

        protected virtual bool IsFormatOfTokenValid(string token)
        {
            return true;
        }

        private void AssertUserPermissions(string token, AuthorizationFilterContext context)
        {
            var (isUserAuthorized, permission) = this.IsUserAuthorized(token, context);

            if (!isUserAuthorized)
            {
                throw new AccessDeniedException(permission);
            }
        }

        protected virtual (bool isAuthorized, string permission) IsUserAuthorized(string token, AuthorizationFilterContext context)
        {
            var userPermissions = this.GetUserPermissions(token);

            var permission = this.MapRequestToPermission(context.HttpContext.Request);

            var userHasPermission = userPermissions.Contains(permission);

            return (userHasPermission, permission);
        }

        protected virtual List<string> GetUserPermissions(string token)
        {
            return new List<string>() { "generic" };
        }

        protected virtual string MapRequestToPermission(HttpRequest request) { return "generic"; }

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
