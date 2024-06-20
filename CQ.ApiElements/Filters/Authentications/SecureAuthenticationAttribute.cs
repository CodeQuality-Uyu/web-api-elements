using CQ.ApiElements.Filters.Exceptions;
using CQ.ApiElements.Filters.Extensions;
using CQ.Exceptions;
using CQ.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace CQ.ApiElements.Filters.Authentications;
public abstract class SecureAuthenticationAttribute : BaseAttribute, IAsyncAuthorizationFilter
{
    public virtual async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var authorizationHeader = context.HttpContext.Request.Headers["Authorization"];
        var privateKeyHeader = context.HttpContext.Request.Headers["PrivateKey"];

        if (Guard.IsNotNullOrEmpty(authorizationHeader))
        {
            await HandleAuthenticationAsync(
                "Authorization",
                authorizationHeader,
                ContextItems.AccountLogged,
                context)
                .ConfigureAwait(false);

            return;
        }

        if (Guard.IsNotNullOrEmpty(privateKeyHeader))
        {
            await HandleAuthenticationAsync(
                "PrivateKey",
                privateKeyHeader,
                ContextItems.ClientSystemLogged,
                context)
                .ConfigureAwait(false);

            return;
        }

        context.Result = BuildMissingHeaderResponse(context);
    }

    private async Task HandleAuthenticationAsync(
        string header,
        string headerValue,
        ContextItems item,
        AuthorizationFilterContext context)
    {
        try
        {
            if (RequestIsLogged(item, context))
            {
                return;
            }

            await AssertHeaderFormatAsync(header, headerValue, context).ConfigureAwait(false);

            var request = await AssertGetRequestAsync(header, headerValue, context).ConfigureAwait(false);

            base.SetItem(context, item, request);
        }
        catch (MissingRequiredHeaderException ex)
        {
            context.Result = BuildMissingHeaderResponse(ex, context);
        }
        catch (InvalidHeaderException ex)
        {
            context.Result = BuildInvalidHeaderFormatResponse(ex, context);
        }
        catch (ExpiredHeaderException ex)
        {
            context.Result = BuildExpiredHeaderResponse(ex, context);
        }
        catch (AccessDeniedException ex)
        {
            context.Result = BuildUnauthorizedResponse(ex, context);
        }
        catch (ArgumentNullException ex)
        {
            context.Result = BuldInvalidArgumentResponse(ex, context);
        }
        catch (Exception)
        {
            context.Result = BuildGenericResponse(context);
        }
    }

    #region Assert header
    private bool RequestIsLogged(
        ContextItems item,
        AuthorizationFilterContext context)
    {
        return base.GetItem(context, item) != null;
    }

    private async Task AssertHeaderFormatAsync(
        string header,
        string headerValue,
        AuthorizationFilterContext context)
    {
        bool isFormatValid;
        try
        {
            isFormatValid = await IsFormatOfHeaderValidAsync(header, headerValue, context).ConfigureAwait(false);
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

    protected virtual Task<bool> IsFormatOfHeaderValidAsync(string header, string headerValue, AuthorizationFilterContext context)
    {
        return Task.FromResult(true);
    }
    #endregion

    #region Get request
    private async Task<object> AssertGetRequestAsync(string header, string headerValue, AuthorizationFilterContext context)
    {
        object item;
        try
        {
            item = await GetRequestByHeaderAsync(header, headerValue, context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new ExpiredHeaderException(header, headerValue, ex);
        }

        if (Guard.IsNull(item))
        {
            throw new ExpiredHeaderException(header, headerValue);
        }

        return item;
    }

    protected abstract Task<object> GetRequestByHeaderAsync(string header, string headerValue, AuthorizationFilterContext context);
    #endregion

    #region Build responses
    protected virtual IActionResult BuildMissingHeaderResponse(
        MissingRequiredHeaderException exception,
        AuthorizationFilterContext context)
    {
        return context.HttpContext.Request.CreateCQErrorResponse(
            HttpStatusCode.Unauthorized,
            "Unauthenticated",
            $"Missing header {exception.Header}");
    }

    protected virtual IActionResult BuildMissingHeaderResponse(AuthorizationFilterContext context)
    {
        return context.HttpContext.Request.CreateCQErrorResponse(
            HttpStatusCode.Unauthorized,
            "Unauthenticated",
            $"Missing header Authorization or PrivateKey");
    }

    protected virtual IActionResult BuildInvalidHeaderFormatResponse(
        InvalidHeaderException ex,
        AuthorizationFilterContext context)
    {
        return context.HttpContext.Request.CreateCQErrorResponse(
            HttpStatusCode.Forbidden,
            "InvalidHeaderFormat",
            $"Invalid format of {ex.Header}");
    }

    protected virtual IActionResult BuildExpiredHeaderResponse(
        ExpiredHeaderException ex,
        AuthorizationFilterContext context)
    {
        return context.HttpContext.Request.CreateCQErrorResponse(
            HttpStatusCode.Unauthorized,
            "ExpiredHeader",
            $"Header {ex.Header} is expired");
    }

    protected virtual IActionResult BuildUnauthorizedResponse(AccessDeniedException ex, AuthorizationFilterContext context)
    {
        return context.HttpContext.Request.CreateCQErrorResponse(
            HttpStatusCode.Forbidden,
            "AccessDenied",
            $"Missing permission {ex.Permission}");
    }

    protected virtual IActionResult BuldInvalidArgumentResponse(ArgumentNullException ex, AuthorizationFilterContext context)
    {
        return context.HttpContext.Request.CreateCQErrorResponse(
            HttpStatusCode.BadRequest,
            "RequestInvalid",
            $"Missing or invalid {ex.ParamName}");
    }

    protected virtual IActionResult BuildGenericResponse(AuthorizationFilterContext context)
    {
        return context.HttpContext.Request.CreateCQErrorResponse(
            HttpStatusCode.InternalServerError,
            "InternalProblem",
            "Problems with the server");
    }
    #endregion
}
