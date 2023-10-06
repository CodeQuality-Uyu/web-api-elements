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
    public abstract class AuthenticationFilter : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                var token = context.HttpContext.Request.Headers["Authorization"];

                AssertToken(token);

                AssertTokenFormat(token);

                AssertUserPermissions(token, context);
            }
            catch (MissingTokenException)
            {
                context.Result = context.HttpContext.Request.CreatePlayerFinderErrorResponse(HttpStatusCode.Unauthorized, "Unauthenticated", $"Need to be authenticated");
            }
            catch (TokenIsNotValidException)
            {
                context.Result = context.HttpContext.Request.CreatePlayerFinderErrorResponse(HttpStatusCode.Forbidden, "InvalidTokenFormat", $"Invalid format of token");
            }
            catch (AccessDeniedException ex)
            {
                context.Result = context.HttpContext.Request.CreatePlayerFinderErrorResponse(HttpStatusCode.Forbidden, "AccessDenied", $"Missing permission {ex.Permission}");
            }
            catch (ArgumentNullException ex)
            {
                context.Result = context.HttpContext.Request.CreatePlayerFinderErrorResponse(HttpStatusCode.BadRequest, "RequestInvalid", $"Missing or invalid {ex.ParamName}");
            }
            catch (Exception)
            {
                context.Result = context.HttpContext.Request.CreatePlayerFinderErrorResponse(HttpStatusCode.InternalServerError, "InternalProblem", "Problems with the server");
            }
        }

        private void AssertToken(string? token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new MissingTokenException();
            }
        }

        private void AssertTokenFormat(string token)
        {
            if (!this.IsFormatOfTokenValid(token))
            {
                throw new TokenIsNotValidException();
            }
        }

        protected abstract bool IsFormatOfTokenValid(string token);

        protected virtual void AssertUserPermissions(string token, AuthorizationFilterContext context)
        {
            var userPermissions = this.GetUserPermissions(token);

            var permission = this.MapRequestToPermission(context.HttpContext.Request);

            var userHasPermission = userPermissions.Contains(permission);

            if (!userHasPermission)
            {
                throw new AccessDeniedException(permission);
            }
        }

        protected virtual List<string> GetUserPermissions(string token)
        {
            return new List<string>() { "generic" };
        }

        protected virtual string MapRequestToPermission(HttpRequest request) { return "generic"; }
    }
}
