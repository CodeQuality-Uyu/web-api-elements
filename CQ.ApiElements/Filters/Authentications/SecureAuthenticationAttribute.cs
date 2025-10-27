using CQ.ApiElements.Filters.ExceptionFilter;
using CQ.ApiElements.Filters.Extensions;
using CQ.Utility;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Security.Principal;

namespace CQ.ApiElements.Filters.Authentications;

public class SecureAuthenticationAttribute(
    object? keyItem = null,
    params object[] authorizationTypes)
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

            var authorizationType = authorizationTypes.FirstOrDefault(a => uniqueToken.Contains(a.ToString(), StringComparison.OrdinalIgnoreCase))?.ToString();
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
        var tokenServices = context.GetService<IEnumerable<ITokenService>>();

        var tokenService = tokenServices.FirstOrDefault(t => string.Equals(t.AuthorizationTypeHandled, authorizationType, StringComparison.OrdinalIgnoreCase));

        if (tokenService == null)
        {
            throw new InvalidOperationException("No token service found for authorization type " + authorizationType);
        }

        var isValid = await IsFormatOfHeaderValidAsync(
            token,
            tokenService)
            .ConfigureAwait(false);
        if (!isValid)
        {
            BuildInvalidHeaderFormat(context);
        }

        var itemRequested = await GetItemOrDefaultAsync(
           token,
           tokenService)
           .ConfigureAwait(false);
        if (itemRequested == null)
        {
            var errorResponse = new ErrorResponse(
                HttpStatusCode.Unauthorized,
                "AuthorizationExpired",
                "Authorization is expired",
                string.Empty,
                "The authorization expired",
                null);

            context.Result = BuildResponse(errorResponse);
            return;
        }

        if (Guard.IsNull(keyItem))
        {
            context.SetItem(
                ContextItem.AccountLogged,
                itemRequested);
            return;
        }
        else
        {
            context.SetItem(
                keyItem!,
                itemRequested);
        }
    }


    #region Assert header
    protected virtual async Task<bool> IsFormatOfHeaderValidAsync(
        string token,
        ITokenService tokenService)
    {
        var isValidToken = await tokenService
            .IsValidAsync(token)
            .ConfigureAwait(false);

        return isValidToken;
    }
    #endregion

    private async Task<object?> GetItemOrDefaultAsync(
        string token,
        ITokenService tokenService)
    {
        var itemLogged = await tokenService
            .GetOrDefaultAsync(token)
            .ConfigureAwait(false);

        return itemLogged;
    }
}
