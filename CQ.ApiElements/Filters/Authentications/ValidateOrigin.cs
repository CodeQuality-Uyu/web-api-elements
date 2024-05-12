using CQ.ApiElements.Filters.Extensions;
using CQ.Utility;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace CQ.ApiElements.Filters.Authentications
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public abstract class ValidateOrigin : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var origin = GetItem(context);

            if (Guard.IsNotNull(origin))
            {
                return;
            }

            context.Result = context.HttpContext.Request.CreateCQErrorResponse(
                HttpStatusCode.Unauthorized,
                "Unauthenticated",
                $"Missing header Authorization or PrivateKey");
        }

        protected abstract object? GetItem(AuthorizationFilterContext context);
    }
}
