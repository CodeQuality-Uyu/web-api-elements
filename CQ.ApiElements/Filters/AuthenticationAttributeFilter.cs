using CQ.ApiElements.Filters.Extensions;
using CQ.Exceptions;
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
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthenticationAttributeFilter : Attribute, IAuthorizationFilter
    {
        private readonly string? _permission;

        public AuthenticationAttributeFilter() { }

        public AuthenticationAttributeFilter(string permission)
        {
            _permission = permission;
        }

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
            try
            {
                if (!this.IsFormatOfTokenValid(token))
                {
                    throw new InvalidTokenException(token);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidTokenException(token, ex);
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
            var permission = this.BuildPermission(token, context);

            try
            {
                var isAuthorized = this.HasUserPermission(token, permission, context);

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

        protected virtual bool HasUserPermission(string token, string permission, AuthorizationFilterContext context)
        {
            return true;
        }

        protected virtual IActionResult BuildMissingAuthenticationResponse(MissingTokenException ex, AuthorizationFilterContext context)
        {
            return context.HttpContext.Request.CreateCQErrorResponse(HttpStatusCode.Unauthorized, "Unauthenticated", $"Need to be authenticated");
        }

        protected virtual IActionResult BuildInvalidAuthenticationFormatResponse(InvalidTokenException ex, AuthorizationFilterContext context)
        {
            return context.HttpContext.Request.CreateCQErrorResponse(HttpStatusCode.Unauthorized, "InvalidToken", $"Invalid auth token provided");
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
            return context.HttpContext.Request.CreateCQErrorResponse(HttpStatusCode.InternalServerError, "ExceptionOccured", "An unpredicted exception ocurred");
        }
    }
}
