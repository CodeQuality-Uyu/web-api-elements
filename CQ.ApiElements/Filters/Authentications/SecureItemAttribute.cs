using CQ.ApiElements.Filters.ExceptionFilter;
using CQ.ApiElements.Filters.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace CQ.ApiElements.Filters.Authentications;
public class SecureItemAttribute(ContextItem Item)
    : BaseAttribute,
    IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        try
        {
            context.GetItem(Item);
        }
        catch (Exception ex)
        {
            var error = new ErrorResponse(
                HttpStatusCode.Unauthorized,
                "Unauthenticated",
                "Item not saved",
                string.Empty,
                "Missing item in context related to token sent",
                ex);

            context.Result = BuildResponse(error);
        }
    }
}
