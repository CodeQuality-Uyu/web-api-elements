
namespace CQ.ApiElements.Filters.Authentications;

public sealed class BearerAuthenticationAttribute
    : SecureAuthenticationAttribute
{
    public BearerAuthenticationAttribute() : base("Bearer")
    {
    }
}
