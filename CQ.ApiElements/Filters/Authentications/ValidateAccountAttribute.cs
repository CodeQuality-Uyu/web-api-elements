using CQ.ApiElements.Filters.Extensions;
using CQ.Utility;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace CQ.ApiElements.Filters.Authentications
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ValidateAccountAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var accountLogged = context.HttpContext.Items[ContextItems.AccountLogged];

            if (Guard.IsNotNull(accountLogged))
                return;

            context.Result = context.HttpContext.Request.CreateCQErrorResponse(
                HttpStatusCode.Unauthorized,
                "Unauthenticated",
                $"Missing header Authorization or PrivateKey");
        }
    }
}
