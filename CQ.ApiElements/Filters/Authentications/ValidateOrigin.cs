using CQ.ApiElements.Filters.Extensions;
using CQ.Utility;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace CQ.ApiElements.Filters.Authentications;
public class ValidateOrigin(ContextItems Item) : BaseAttribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var origin = base.GetItem(context, Item);

        if (Guard.IsNotNull(origin))
        {
            return;
        }

        context.Result = context.HttpContext.Request.CreateCQErrorResponse(
            HttpStatusCode.Unauthorized,
            "Unauthenticated",
            $"Missing header Authorization or PrivateKey");
    }
}
