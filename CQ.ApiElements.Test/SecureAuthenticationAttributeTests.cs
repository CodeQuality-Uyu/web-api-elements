using System.Net;
using CQ.ApiElements;
using CQ.ApiElements.Filters.Authentications;
using CQ.ApiElements.Filters.ExceptionFilter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CQ.ApiElements.Test;

/// <summary>
/// El esquema es la primera palabra del header y tiene que estar entre los que acepta el
/// endpoint. Varios servicios pueden manejar el mismo esquema: el token va al primero que lo
/// reconoce.
/// </summary>
[TestClass]
public sealed class SecureAuthenticationAttributeTests
{
    private static readonly SecureAuthenticationAttribute BearerOrSubscription =
        new(null, AuthorizationType.Bearer, AuthorizationType.Subscription);

    [TestMethod]
    public async Task The_token_goes_to_the_service_of_the_scheme_that_recognizes_it()
    {
        var opaque = new FakeTokenService(AuthorizationType.Bearer, token => token.StartsWith("opaque"), "opaque-account");
        var jwt = new FakeTokenService(AuthorizationType.Bearer, token => token.StartsWith("jwt"), "jwt-account");

        var context = await AuthenticateAsync(BearerOrSubscription, "Bearer jwt-token", opaque, jwt);

        Assert.IsNull(context.Result);
        Assert.AreEqual("jwt-account", context.HttpContext.Items[ContextItem.AccountLogged]);
        Assert.AreEqual(0, opaque.GetCalls);
    }

    [TestMethod]
    public async Task The_scheme_is_matched_ignoring_case()
    {
        var bearer = new FakeTokenService(AuthorizationType.Bearer, _ => true, "account");

        var context = await AuthenticateAsync(BearerOrSubscription, "bearer token", bearer);

        Assert.IsNull(context.Result);
        Assert.AreEqual("account", context.HttpContext.Items[ContextItem.AccountLogged]);
    }

    [TestMethod]
    public async Task A_service_of_another_scheme_is_never_asked()
    {
        var subscription = new FakeTokenService(AuthorizationType.Subscription, _ => true, "subscription");
        var bearer = new FakeTokenService(AuthorizationType.Bearer, _ => true, "account");

        var context = await AuthenticateAsync(BearerOrSubscription, "Bearer token", subscription, bearer);

        Assert.AreEqual("account", context.HttpContext.Items[ContextItem.AccountLogged]);
        Assert.AreEqual(0, subscription.ValidateCalls);
    }

    [DataTestMethod]
    [DataRow("Key token", DisplayName = "esquema que el endpoint no acepta")]
    [DataRow("Basic token", DisplayName = "esquema desconocido")]
    [DataRow("0 token", DisplayName = "un número no es un esquema")]
    [DataRow("Bearer", DisplayName = "sin token")]
    [DataRow("token-Bearer", DisplayName = "el esquema no es la primera palabra")]
    public async Task An_unusable_header_is_an_invalid_header_format(string header)
    {
        var bearer = new FakeTokenService(AuthorizationType.Bearer, _ => true, "account");

        var context = await AuthenticateAsync(BearerOrSubscription, header, bearer);

        AssertError(context, HttpStatusCode.Forbidden, "InvalidHeaderFormat");
        Assert.AreEqual(0, bearer.ValidateCalls);
    }

    [TestMethod]
    public async Task A_token_no_service_recognizes_is_rejected_without_looking_it_up()
    {
        var bearer = new FakeTokenService(AuthorizationType.Bearer, _ => false, "account");

        var context = await AuthenticateAsync(BearerOrSubscription, "Bearer token", bearer);

        AssertError(context, HttpStatusCode.Forbidden, "InvalidHeaderFormat");
        Assert.AreEqual(0, bearer.GetCalls);
    }

    [TestMethod]
    public async Task A_recognized_token_without_an_item_is_expired()
    {
        var bearer = new FakeTokenService(AuthorizationType.Bearer, _ => true, item: null);

        var context = await AuthenticateAsync(BearerOrSubscription, "Bearer token", bearer);

        AssertError(context, HttpStatusCode.Unauthorized, "AuthorizationExpired");
    }

    [TestMethod]
    public async Task A_missing_header_is_unauthenticated()
    {
        var bearer = new FakeTokenService(AuthorizationType.Bearer, _ => true, "account");

        var context = await AuthenticateAsync(BearerOrSubscription, header: null, bearer);

        AssertError(context, HttpStatusCode.Unauthorized, "Unauthenticated");
    }

    [TestMethod]
    public async Task The_bearer_attribute_only_accepts_bearer()
    {
        var subscription = new FakeTokenService(AuthorizationType.Subscription, _ => true, "subscription");

        var context = await AuthenticateAsync(new BearerAuthenticationAttribute(), "Subscription key", subscription);

        AssertError(context, HttpStatusCode.Forbidden, "InvalidHeaderFormat");
    }

    private static async Task<AuthorizationFilterContext> AuthenticateAsync(
        SecureAuthenticationAttribute attribute,
        string? header,
        params ITokenService[] tokenServices)
    {
        var services = new ServiceCollection();
        foreach (var tokenService in tokenServices)
        {
            services.AddSingleton(tokenService);
        }

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        if (header != null)
        {
            httpContext.Request.Headers["Authorization"] = header;
        }

        var context = new AuthorizationFilterContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            []);

        await attribute.OnAuthorizationAsync(context);

        return context;
    }

    private static void AssertError(
        AuthorizationFilterContext context,
        HttpStatusCode statusCode,
        string code)
    {
        var result = context.Result as ObjectResult;
        Assert.IsNotNull(result, "Se esperaba una respuesta de error");
        Assert.AreEqual((int)statusCode, result.StatusCode);
        Assert.AreEqual(code, ((ErrorResponse)result.Value!).Code);
    }

    private sealed class FakeTokenService(
        AuthorizationType authorizationType,
        Func<string, bool> recognizes,
        object? item)
        : ITokenService
    {
        public int ValidateCalls { get; private set; }

        public int GetCalls { get; private set; }

        public AuthorizationType AuthorizationTypeHandled => authorizationType;

        public Task<string> CreateAsync(object item) => throw new NotSupportedException();

        public Task<bool> IsValidAsync(string value)
        {
            ValidateCalls++;

            return Task.FromResult(recognizes(value));
        }

        public Task<object?> GetOrDefaultAsync(string value)
        {
            GetCalls++;

            return Task.FromResult(item);
        }
    }
}
