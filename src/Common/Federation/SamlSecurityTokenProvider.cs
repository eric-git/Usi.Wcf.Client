using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;

namespace Common.Federation;

public class SamlSecurityTokenProvider(SecurityToken securityToken) : SecurityTokenProvider
{
  protected override SecurityToken GetTokenCore(TimeSpan timeout) => securityToken;
}
