using Microsoft.AspNetCore.Mvc.Filters;

namespace CQ.ApiElements.Filters.Authentications
{
    public sealed class ValidateClientSystemAttribute : ValidateOrigin
    {
        protected override object? GetItem(AuthorizationFilterContext context)
        {
            var clientSystemLogged = context.HttpContext.Items[ContextItems.ClientSystemLogged];

            return clientSystemLogged;
        }
    }
}
