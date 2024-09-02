using CQ.ApiElements.Filters.ExceptionFilter;
using CQ.ApiElements.Filters.Exceptions;
using CQ.ApiElements.Filters.Extensions;
using CQ.Utility;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Security.Principal;

namespace CQ.ApiElements.Filters.Authentications;
public abstract class SecureAuthenticationAttribute
    : BaseAttribute,
    IAsyncAuthorizationFilter
{
    private static IDictionary<Type, ErrorResponse>? _errors;

    private static IDictionary<Type, ErrorResponse> Errors
    {
        get
        {
            _errors ??= new Dictionary<Type, ErrorResponse>
            {
                {
                    typeof(MissingRequiredHeaderException),
                    new DynamicErrorResponse<MissingRequiredHeaderException>(
                        HttpStatusCode.Unauthorized,
                        "Unauthenticated",
                        (ex, context) => $"Missing header {ex.Header}"
                    )
                    {
                        Description = "To use the endpoint a value must be send in header Authorization or PrivateKey"
                    }
                },
                {
                    typeof(InvalidHeaderException),
                    new DynamicErrorResponse<InvalidHeaderException>(
                        HttpStatusCode.Forbidden,
                    "InvalidHeaderFormat",
                    (ex, context) => $"Invalid format of {ex.Header}"
                        )
                },
                {
                    typeof(ExpiredHeaderException),
                    new DynamicErrorResponse<ExpiredHeaderException>(
                        HttpStatusCode.Unauthorized,
                    "ExpiredHeader",
                    (ex, context) => $"Header {ex.Header} is expired"
                        )
                },
                {
                    typeof(ArgumentNullException),
                    new DynamicErrorResponse<ArgumentNullException>(
                    HttpStatusCode.BadRequest,
                    "RequestInvalid",
                    (ex, context) => $"Missing or invalid {ex.ParamName}"
                        )
                }
            };

            return _errors;
        }
    }

    public virtual async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        try
        {
            var isFakeAuthActive = IsFakeAuthActive(context);
            if (isFakeAuthActive)
            {
                return;
            }

            var authorizationHeader = context.HttpContext.Request.Headers[HeaderNames.Authorization];
            var privateKeyHeader = context.HttpContext.Request.Headers["PrivateKey"];

            if (Guard.IsNullOrEmpty(authorizationHeader) &&
                Guard.IsNullOrEmpty(privateKeyHeader))
            {
                MissingRequiredHeaderException.Throw($"{HeaderNames.Authorization} or PrivateKey");
            }

            if (Guard.IsNotNullOrEmpty(authorizationHeader))
            {
                await HandleAuthenticationAsync(
                    HeaderNames.Authorization,
                    authorizationHeader,
                    ContextItems.AccountLogged,
                    context)
                    .ConfigureAwait(false);

                return;
            }

            await HandleAuthenticationAsync(
                "PrivateKey",
                privateKeyHeader,
                ContextItems.ClientSystemLogged,
                context)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var exceptionContext = new ExceptionThrownContext(
                context,
                ex,
                string.Empty,
                string.Empty);

            context.Result = BuildErrorResponse(
                Errors,
                exceptionContext);
        }
    }

    private bool IsFakeAuthActive(AuthorizationFilterContext context)
    {
        var itemRequested = GetFakeAuth(context);

        if (Guard.IsNull(itemRequested))
        {
            return false;
        }

        context.SetItem(
            ContextItems.AccountLogged,
            itemRequested!);

        return true;
    }

    private object? GetFakeAuth(AuthorizationFilterContext context)
    {
        object? fakeAccount;
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

    private async Task HandleAuthenticationAsync(
        string header,
        string headerValue,
        ContextItems item,
        AuthorizationFilterContext context)
    {
        if (ItemIsLogged(item, context))
        {
            return;
        }

        await AssertHeaderFormatAsync(
            header,
            headerValue,
            context)
            .ConfigureAwait(false);

        var itemRequested = await AssertGetItemAsync(
           header,
           headerValue,
           context)
           .ConfigureAwait(false);

        context.SetItem(
            item,
            itemRequested);
    }

    #region Assert header
    private static bool ItemIsLogged(
        ContextItems item,
        AuthorizationFilterContext context)
    {
        return context.GetItemOrDefault(item) != null;
    }

    private async Task AssertHeaderFormatAsync(
        string header,
        string headerValue,
        AuthorizationFilterContext context)
    {
        bool isFormatValid;
        try
        {
            isFormatValid = await IsFormatOfHeaderValidAsync(
                header,
                headerValue,
                context)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new InvalidHeaderException(header, headerValue, ex);
        }

        if (!isFormatValid)
        {
            throw new InvalidHeaderException(header, headerValue);
        }
    }

    protected virtual Task<bool> IsFormatOfHeaderValidAsync(
        string header,
        string headerValue,
        AuthorizationFilterContext context)
    {
        return Task.FromResult(true);
    }
    #endregion

    #region Get request
    private async Task<object> AssertGetItemAsync(
        string header,
        string headerValue,
        AuthorizationFilterContext context)
    {
        try
        {
            var item = await GetItemByHeaderAsync(
                header,
                headerValue,
                context)
                .ConfigureAwait(false);

            return item;
        }
        catch (Exception ex)
        {
            throw new ExpiredHeaderException(
                header,
                headerValue,
                ex);
        }
    }

    protected abstract Task<object> GetItemByHeaderAsync(
        string header,
        string headerValue,
        AuthorizationFilterContext context);
    #endregion
}
