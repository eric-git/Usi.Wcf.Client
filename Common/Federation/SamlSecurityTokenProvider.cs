using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;

namespace Common.Federation;

public class SamlSecurityTokenProvider : SecurityTokenProvider
{
    private readonly SecurityToken _securityToken;

    public SamlSecurityTokenProvider(SecurityToken securityToken)
    {
        _securityToken = securityToken;
    }

    protected override SecurityToken GetTokenCore(TimeSpan timeout)
    {
        return _securityToken;
    }
}