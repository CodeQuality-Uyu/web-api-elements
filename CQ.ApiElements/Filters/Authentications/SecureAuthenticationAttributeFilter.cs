using CQ.ApiElements.Filters.Exceptions;
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
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters.Authentications
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public abstract class SecureAuthenticationAttributeFilter : Attribute, IAuthorizationFilter
    {
        public virtual void OnAuthorization(AuthorizationFilterContext context)
        {
            var authorizationHeader = context.HttpContext.Request.Headers["Authorization"];
            var privateKeyHeader = context.HttpContext.Request.Headers["PrivateKey"];

            if (Guard.IsNotNullOrEmpty(authorizationHeader))
            {
                HandleAuthentication(
                    "Authorization",
                    authorizationHeader,
                    ContextItems.AccountLogged,
                    context);

                return;
            }

            if (Guard.IsNotNullOrEmpty(privateKeyHeader))
            {
                HandleAuthentication(
                    "PrivateKey",
                    privateKeyHeader,
                    ContextItems.ClientSystemLogged,
                    context);

                return;
            }

            context.Result = BuildMissingHeaderResponse(context);
        }

        private void HandleAuthentication(
            string header,
            string headerValue,
            ContextItems item,
            AuthorizationFilterContext context)
        {
            try
            {
                if (RequestIsLogged(item, context))
                    return;

                AssertHeaderFormat(header, headerValue, context);

                var request = AssertGetRequest(header, headerValue, context);

                context.HttpContext.Items[item] = request;
            }
            catch (InvalidHeaderException ex)
            {
                context.Result = BuildInvalidHeaderFormatResponse(ex, header, context);
            }
            catch (ExpiredHeaderException ex)
            {
                context.Result = BuildExpiredHeaderResponse(ex, header, context);
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

        #region Assert header
        private bool RequestIsLogged(
            ContextItems item,
            AuthorizationFilterContext context)
        {
            return context.HttpContext.Items[item] != null;
        }

        private void AssertHeaderFormat(
            string header,
            string headerValue,
            AuthorizationFilterContext context)
        {
            bool isFormatValid;
            try
            {
                isFormatValid = IsFormatOfHeaderValid(header, headerValue, context);
            }
            catch (Exception ex)
            {
                throw new InvalidHeaderException(header, headerValue, ex);
            }

            if (!isFormatValid)
                throw new InvalidHeaderException(header, headerValue);
        }

        protected virtual bool IsFormatOfHeaderValid(string header, string headerValue, AuthorizationFilterContext context)
        {
            return true;
        }
        #endregion

        #region Get request
        private object AssertGetRequest(
            string header,
            string headerValue,
            AuthorizationFilterContext context)
        {
            object request;
            try
            {
                request = GetRequestByHeader(header, headerValue, context);
            }
            catch (Exception ex)
            {
                throw new ExpiredHeaderException(header, headerValue, ex);
            }

            if (request == null)
                throw new ExpiredHeaderException(header, headerValue);

            return request;
        }

        protected abstract object GetRequestByHeader(string header, string headerValue, AuthorizationFilterContext context);
        #endregion

        #region Build responses
        protected virtual IActionResult BuildMissingHeaderResponse(AuthorizationFilterContext context)
        {
            return context.HttpContext.Request.CreateCQErrorResponse(
                HttpStatusCode.Unauthorized,
                "Unauthenticated",
                $"Missing header Authorization or PrivateKey");
        }

        protected virtual IActionResult BuildInvalidHeaderFormatResponse(
            InvalidHeaderException ex,
            string header,
            AuthorizationFilterContext context)
        {
            return context.HttpContext.Request.CreateCQErrorResponse(
                HttpStatusCode.Forbidden,
                "InvalidHeaderFormat",
                $"Invalid format of {header}");
        }


        protected virtual IActionResult BuildExpiredHeaderResponse(
            ExpiredHeaderException ex,
            string header,
            AuthorizationFilterContext context)
        {
            return context.HttpContext.Request.CreateCQErrorResponse(
                HttpStatusCode.Unauthorized,
                "ExpiredHeader",
                $"Header {header} is expired");
        }

        protected virtual IActionResult BuildUnauthorizedResponse(AccessDeniedException ex, AuthorizationFilterContext context)
        {
            return context.HttpContext.Request.CreateCQErrorResponse(
                HttpStatusCode.Forbidden,
                "AccessDenied",
                $"Missing permission {ex.Permission}");
        }

        protected virtual IActionResult BuldInvalidArgumentResponse(ArgumentNullException ex, AuthorizationFilterContext context)
        {
            return context.HttpContext.Request.CreateCQErrorResponse(
                HttpStatusCode.BadRequest,
                "RequestInvalid",
                $"Missing or invalid {ex.ParamName}");
        }

        protected virtual IActionResult BuildGenericResponse(AuthorizationFilterContext context)
        {
            return context.HttpContext.Request.CreateCQErrorResponse(
                HttpStatusCode.InternalServerError,
                "InternalProblem",
                "Problems with the server");
        }
        #endregion

        protected virtual TService GetService<TService>(AuthorizationFilterContext context)
        {
            return context.HttpContext.RequestServices.GetRequiredService<TService>();
        }
    }
}
