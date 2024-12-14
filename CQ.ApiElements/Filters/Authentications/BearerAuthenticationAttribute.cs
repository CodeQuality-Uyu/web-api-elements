
using CQ.AuthProvider.Abstractions;

namespace CQ.ApiElements.Filters.Authentications
{
    public sealed class BearerAuthenticationAttribute
        : SecureAuthenticationAttribute
        <IBearerTokenService, IBearerLoggedService>
    {
        public BearerAuthenticationAttribute() : base("Bearer")
        {
        }
    }
}
