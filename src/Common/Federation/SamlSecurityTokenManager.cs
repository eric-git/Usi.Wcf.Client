using System.IdentityModel.Selectors;
using System.ServiceModel;

namespace Common.Federation;

public class SamlSecurityTokenManager(SamlClientCredentials samlClientCredentials)
    : ClientCredentialsSecurityTokenManager(samlClientCredentials)
{
    public override SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement tokenRequirement) => new SamlSecurityTokenProvider(samlClientCredentials.ProofToken);
}
