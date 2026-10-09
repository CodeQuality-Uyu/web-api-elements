namespace CQ.ApiElements;

/// <summary>
/// Authenticates the tokens of one <see cref="AuthorizationType"/> for
/// <see cref="Filters.Authentications.SecureAuthenticationAttribute"/>. Issuing
/// tokens is not part of it: each app does that its own way.
/// </summary>
public interface ITokenService
{
    AuthorizationType AuthorizationTypeHandled { get; }

    /// <summary>
    /// Whether <paramref name="value"/> has the format this service issues.
    /// Several services may handle the same scheme: the authentication filter
    /// hands the token to the first one that recognizes it.
    /// </summary>
    Task<bool> IsValidAsync(string value);

    Task<object?> GetOrDefaultAsync(string value);
}
