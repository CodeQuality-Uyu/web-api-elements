namespace CQ.ApiElements;

public interface ITokenService
{
    string AuthorizationTypeHandled { get; }

    Task<string> CreateAsync(object item);

    Task<bool> IsValidAsync(string value);

    Task<object?> GetOrDefaultAsync(string value);
}