using System.IdentityModel.Selectors;
using System.ServiceModel;

namespace Common.Federation;

public class SamlSecurityTokenManager : ClientCredentialsSecurityTokenManager
{
    private readonly SamlClientCredentials _samlClientCredentials;

    public SamlSecurityTokenManager(SamlClientCredentials samlClientCredentials)
        : base(samlClientCredentials)
    {
        _samlClientCredentials = samlClientCredentials;
    }

    public override SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement tokenRequirement)
    {
        return new SamlSecurityTokenProvider(_samlClientCredentials.ProofToken);
    }
}