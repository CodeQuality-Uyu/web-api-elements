using CQ.ApiElements.Filters.ExceptionFilter;
using CQ.ApiElements.Filters.Exceptions;
using CQ.ApiElements.Filters.Extensions;
using CQ.AuthProvider.Abstractions;
using CQ.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System;
using System.Net;
using System.Security.Principal;

namespace CQ.ApiElements.Filters.Authentications;

public abstract class SecureAuthenticationAttribute
    <TokenService, ItemLoggedService>(
    params string[] AuthorizationTypes)
    : BaseAttribute,
    IAsyncAuthorizationFilter
    where TokenService : class, ITokenService
    where ItemLoggedService : class, IItemLoggedService
{
    public virtual async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        try
        {
            var authorizationHeader = context.HttpContext.Request.Headers[HeaderNames.Authorization];

            var isFakeAuthActive = IsFakeAuthActive(context);
            if (isFakeAuthActive && Guard.IsNullOrEmpty(authorizationHeader))
            {
                return;
            }

            if (Guard.IsNullOrEmpty(authorizationHeader))
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

            var isValid = AuthorizationTypes.Any(a => authorizationHeader.Contains(a));
            if (!isValid)
            {
                BuildInvalidHeaderFormat(context);
                return;
            }

            await HandleAuthenticationAsync(authorizationHeader, context)
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
        string headerValue,
        AuthorizationFilterContext context)
    {
        var isValid = await IsFormatOfHeaderValidAsync(
                headerValue,
                context)
                .ConfigureAwait(false);
        if (!isValid)
        {
            BuildInvalidHeaderFormat(context);
        }

        var itemRequested = await GetItemAsync(
           headerValue,
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
        string headerValue,
        AuthorizationFilterContext context)
    {
        var tokenService = context.GetService<TokenService>();

        var isValidToken = await tokenService
            .IsValidAsync(headerValue)
            .ConfigureAwait(false);

        return isValidToken;
    }
    #endregion

    private async Task<(object? item, Exception? error)> GetItemAsync(
        string headerValue,
        AuthorizationFilterContext context)
    {
        try
        {
            var itemLoggedService = context.GetService<ItemLoggedService>();

            var itemLogged = await itemLoggedService
                .GetByHeaderAsync(headerValue)
                .ConfigureAwait(false);

            return (itemLogged, null);
        }
        catch (Exception ex)
        {
            return (null, ex);
        }
    }
}
