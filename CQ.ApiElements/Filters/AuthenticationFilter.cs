using CQ.Api.Filters.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
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
                context.Result = new ObjectResult(new
                {
                    Code = "Unauthenticated",
                    Message = $"Need to be authenticated"
                })
                {
                    StatusCode = 401,
                };
            }
            catch (TokenIsNotValidException)
            {
                context.Result = new ObjectResult(new
                {
                    Code = "InvalidTokenFormat",
                    Message = $"Invalid format of token"
                })
                {
                    StatusCode = 403,
                };
            }
            catch (AccessDeniedException ex)
            {
                context.Result = new ObjectResult(new
                {
                    Code = "AccessDenied",
                    Message = $"Missing permission {ex.Permission}"
                })
                {
                    StatusCode = 403,
                };
            }
            catch(ArgumentNullException ex)
            {
                context.Result = new ObjectResult(new
                {
                    Code = "RequestInvalid",
                    Message = $"Missing or invalid {ex.ParamName}"
                })
                {
                    StatusCode = 400,
                };
            }
            catch (Exception)
            {
                context.Result = new ObjectResult(new
                {
                    Code = "InternalProblem",
                    Message = "Problems with the server"
                })
                {
                    StatusCode = 500,
                };
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
