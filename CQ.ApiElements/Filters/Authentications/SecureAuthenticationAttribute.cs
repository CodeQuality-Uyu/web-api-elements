using CQ.ApiElements.Filters.ExceptionFilter;
using CQ.ApiElements.Filters.Extensions;
using CQ.Utility;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Security.Principal;

namespace CQ.ApiElements.Filters.Authentications;

/// <summary>
/// Authenticates the request with the Authorization header,
/// <c>&lt;scheme&gt; &lt;token&gt;</c>, accepting only the schemes in
/// <paramref name="authorizationTypes"/>. The token goes to the first
/// registered <see cref="ITokenService"/> that handles the scheme and
/// recognizes the token, so several services can share a scheme.
/// </summary>
public class SecureAuthenticationAttribute(
    object? keyItem = null,
    params AuthorizationType[] authorizationTypes)
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

            if (authorizationHeaderVaue.Count > 1 ||
                !TryParseHeader(authorizationHeaderVaue.ToString(), out var authorizationType, out var token))
            {
                BuildInvalidHeaderFormat(context);
                return;
            }

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

    /// <summary>
    /// The scheme is the first word of the header and has to be one of the
    /// schemes the endpoint accepts, matched by name (a number is not a
    /// scheme); the token is the rest.
    /// </summary>
    private bool TryParseHeader(
        string header,
        out AuthorizationType authorizationType,
        out string token)
    {
        authorizationType = default;
        token = string.Empty;

        var value = header.Trim();
        var separator = value.IndexOf(' ');
        if (separator <= 0)
        {
            return false;
        }

        var scheme = value[..separator];
        token = value[(separator + 1)..].Trim();

        return Enum.TryParse(scheme, ignoreCase: true, out authorizationType) &&
            string.Equals(authorizationType.ToString(), scheme, StringComparison.OrdinalIgnoreCase) &&
            authorizationTypes.Contains(authorizationType) &&
            token.Length > 0;
    }

    private async Task HandleAuthenticationAsync(
        AuthorizationType authorizationType,
        string token,
        AuthorizationFilterContext context)
    {
        var tokenServices = context
            .GetService<IEnumerable<ITokenService>>()
            .Where(t => t.AuthorizationTypeHandled == authorizationType)
            .ToList();

        if (tokenServices.Count == 0)
        {
            throw new InvalidOperationException("No token service found for authorization type " + authorizationType);
        }

        var tokenService = await GetIssuerOrDefaultAsync(
            token,
            tokenServices)
            .ConfigureAwait(false);
        if (tokenService == null)
        {
            BuildInvalidHeaderFormat(context);
            return;
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
    /// <summary>
    /// The first service of the scheme that recognizes the token. Null when
    /// none does.
    /// </summary>
    private async Task<ITokenService?> GetIssuerOrDefaultAsync(
        string token,
        List<ITokenService> tokenServices)
    {
        foreach (var tokenService in tokenServices)
        {
            var isValid = await IsFormatOfHeaderValidAsync(
                token,
                tokenService)
                .ConfigureAwait(false);

            if (isValid)
            {
                return tokenService;
            }
        }

        return null;
    }

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
