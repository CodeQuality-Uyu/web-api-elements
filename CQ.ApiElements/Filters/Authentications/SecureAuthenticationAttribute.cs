using CQ.ApiElements.Filters.ExceptionFilter;
using CQ.ApiElements.Filters.Extensions;
using CQ.AuthProvider.Abstractions;
using CQ.Utility;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Security.Principal;

namespace CQ.ApiElements.Filters.Authentications;

public abstract class SecureAuthenticationAttribute(params string[] _authorizationTypes)
    : BaseAttribute,
    IAsyncAuthorizationFilter
{
    public virtual async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        try
        {
            var authorizationHeaderVaue = context.HttpContext.Request.Headers[HeaderNames.Authorization];

            if (IsFakeAuthActive(context) && Guard.IsNullOrEmpty(authorizationHeaderVaue))
            {
                return;
            }

            if (Guard.IsNullOrEmpty(authorizationHeaderVaue))
            {
                var errorResponse = new ErrorResponse(
                    HttpStatusCode.Unauthorized,
                    "Unauthenticated",
                    "Missing Authorization header",
                    string.Empty,
                    "The endpoint is protected with authorization (needs to be sent Authorization header)",
                    null
                    );

                context.Result = BuildResponse(errorResponse);
                return;
            }

            var uniqueToken = authorizationHeaderVaue.ToString();

            var authorizationType = _authorizationTypes.FirstOrDefault(a => uniqueToken.Contains(a, StringComparison.OrdinalIgnoreCase));
            if (Guard.IsNullOrEmpty(authorizationType) || authorizationHeaderVaue.Count > 1)
            {
                BuildInvalidHeaderFormat(context);
                return;
            }

            var token = uniqueToken
                .Replace(authorizationType, string.Empty)
                .Trim();

            await HandleAuthenticationAsync(
                authorizationType,
                token,
                context)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var error = BuildUnexpectedErrorResponse(ex);
            var response = BuildResponse(error);
            context.Result = response;
        }
    }

    #region Fake Authenticatino
    private bool IsFakeAuthActive(AuthorizationFilterContext context)
    {
        var itemRequested = GetFakeAuthOrDefault(context);

        if (Guard.IsNull(itemRequested))
        {
            return false;
        }

        context.SetItem(
            ContextItem.AccountLogged,
            itemRequested!);

        return true;
    }

    private object? GetFakeAuthOrDefault(AuthorizationFilterContext context)
    {
        IPrincipal? fakeAccount;
        try
        {
            fakeAccount = context.GetService<IPrincipal>();
        }
        catch (Exception)
        {
            fakeAccount = null;
        }

        return fakeAccount;
    }
    #endregion

    private static void BuildInvalidHeaderFormat(AuthorizationFilterContext context)
    {
        var errorResponse = new ErrorResponse(
                    HttpStatusCode.Forbidden,
                    "InvalidHeaderFormat",
                    "Invalid Authorization Header",
                    string.Empty,
                    "The value of Authorization header is incorrect for the authorization type setted for the endpoint",
                    null
                    );

        context.Result = BuildResponse(errorResponse);
    }

    private async Task HandleAuthenticationAsync(
        string authorizationType,
        string token,
        AuthorizationFilterContext context)
    {
        var isValid = await IsFormatOfHeaderValidAsync(
            authorizationType,
            token,
            context)
            .ConfigureAwait(false);
        if (!isValid)
        {
            BuildInvalidHeaderFormat(context);
        }

        var itemRequested = await GetItemAsync(
            authorizationType,
           token,
           context)
           .ConfigureAwait(false);
        if (itemRequested.error != null)
        {
            var errorResponse = new ErrorResponse(
                HttpStatusCode.Unauthorized,
                "AuthorizationExpired",
                "Authorization is expired",
                string.Empty,
                "The authorization expired",
                itemRequested.error
                );

            context.Result = BuildResponse(errorResponse);
            return;
        }

        context.SetItem(
            ContextItem.AccountLogged,
            itemRequested.item);
    }


    #region Assert header
    protected virtual async Task<bool> IsFormatOfHeaderValidAsync(
        string authorizationType,
        string token,
        AuthorizationFilterContext context)
    {
        var tokenServices = context.GetService<IEnumerable<ITokenService>>();

        var tokenService = tokenServices.First(t => string.Equals(t.AuthorizationTypeHandled, authorizationType, StringComparison.OrdinalIgnoreCase));

        var isValidToken = await tokenService
            .IsValidAsync(token)
            .ConfigureAwait(false);

        return isValidToken;
    }
    #endregion

    private async Task<(object? item, Exception? error)> GetItemAsync(
        string authorizationType,
        string token,
        AuthorizationFilterContext context)
    {
        try
        {
            var itemLoggedServices = context.GetService<IEnumerable<IItemLoggedService>>();

            var itemLoggedService = itemLoggedServices.First(i => string.Equals(i.AuthorizationTypeHandled, authorizationType, StringComparison.OrdinalIgnoreCase));

            var itemLogged = await itemLoggedService
                .GetByHeaderAsync(token)
                .ConfigureAwait(false);

            return (itemLogged, null);
        }
        catch (Exception ex)
        {
            return (null, ex);
        }
    }
}
