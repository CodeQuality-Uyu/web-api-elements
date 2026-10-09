namespace CQ.ApiElements;

public interface ITokenService
{
    AuthorizationType AuthorizationTypeHandled { get; }

    Task<string> CreateAsync(object item);

    /// <summary>
    /// Whether <paramref name="value"/> has the format this service issues.
    /// Several services may handle the same scheme: the authentication filter
    /// hands the token to the first one that recognizes it.
    /// </summary>
    Task<bool> IsValidAsync(string value);

    Task<object?> GetOrDefaultAsync(string value);
}
