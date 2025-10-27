
namespace CQ.ApiElements.Filters.Authentications;

public sealed class BearerAuthenticationAttribute()
    : SecureAuthenticationAttribute(authorizationTypes: "Bearer")
{
}
