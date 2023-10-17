using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel.Description;

namespace Common.Federation;

public class SamlClientCredentials : ClientCredentials
{
    public SamlClientCredentials(SecurityToken securityToken)
    {
        ProofToken = securityToken;
    }

    protected SamlClientCredentials(SamlClientCredentials other) : base(other)
    {
        ProofToken = other.ProofToken;
    }

    public SecurityToken ProofToken { get; set; }

    protected override ClientCredentials CloneCore()
    {
        return new SamlClientCredentials(this);
    }

    public override SecurityTokenManager CreateSecurityTokenManager()
    {
        return new SamlSecurityTokenManager(this);
    }
}