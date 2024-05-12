using Microsoft.AspNetCore.Mvc.Filters;

namespace CQ.ApiElements.Filters.Authentications
{
    public class ValidateAccountAttribute : ValidateOrigin
    {
        protected override object? GetItem(AuthorizationFilterContext context)
        {
            var accountLogged = context.HttpContext.Items[ContextItems.AccountLogged];

            return accountLogged;
        }
    }
}
