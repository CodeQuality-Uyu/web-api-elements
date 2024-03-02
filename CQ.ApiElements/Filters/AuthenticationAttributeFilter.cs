using CQ.ApiElements.Filters.Extensions;
using CQ.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
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

                AssertToken(token, context);

                var accountLogged = context.HttpContext.Items[ContextItems.AccountLogged];

                if (accountLogged == null)
                {
                    accountLogged = GetAccountByToken(token, context);

                    context.HttpContext.Items[ContextItems.AccountLogged] = accountLogged;
                }

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

        private void AssertToken(string? token, AuthorizationFilterContext context)
        {
            if (AccountIsLogged(context))
                return;

            if (string.IsNullOrEmpty(token))
                throw new MissingTokenException();

            this.AssertTokenFormat(token, context);
        }

        private bool AccountIsLogged(AuthorizationFilterContext context)
        {
            return context.HttpContext.Items[ContextItems.AccountLogged] != null;
        }

        private void AssertTokenFormat(string token, AuthorizationFilterContext context)
        {
            bool isFormatValid;
            try
            {
                isFormatValid = this.IsFormatOfTokenValid(token, context);
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

        protected virtual bool IsFormatOfTokenValid(string token, AuthorizationFilterContext context)
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

        protected virtual object GetAccountByToken(string token, AuthorizationFilterContext context)
        {
            return null;
        }


        #region Build response
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
        #endregion

        protected virtual TService GetService<TService>(AuthorizationFilterContext context)
        {
            return context.HttpContext.RequestServices.GetRequiredService<TService>();
        }
    }
}
