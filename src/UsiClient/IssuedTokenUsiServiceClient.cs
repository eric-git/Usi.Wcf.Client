using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Federation;
using System.ServiceModel.Security;
using Common.AusKey;
using Common.Configuration;
using Common.Federation;
using Common.ServiceClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UsiClient;

public class IssuedTokenUsiServiceClient(
    IAusKeyManager ausKeyManager,
    IWsMessageHelper wsMessageHelper,
    IConfiguration configuration,
    ILogger<IUSIService> logger) : BaseUsiServiceClient(logger)
{
    private static SecurityToken? s_securityToken;

    protected override IUSIService GetChannel()
    {
        var now = DateTime.UtcNow;
        WSTrustTokenParameters wsTrustTokenParameters;
        if (s_securityToken == null || s_securityToken.ValidFrom > now || s_securityToken.ValidTo <= now)
        {
            WS2007HttpBinding ws2007HttpBinding = new(SecurityMode.TransportWithMessageCredential);
            ws2007HttpBinding.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.Basic256Sha256;
            ws2007HttpBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
            ws2007HttpBinding.Security.Message.EstablishSecurityContext = false;
            ws2007HttpBinding.Security.Message.NegotiateServiceCredential = false;
            wsTrustTokenParameters = WSTrustTokenParameters.CreateWS2007FederationTokenParameters(ws2007HttpBinding, new EndpointAddress(configuration[SettingsKey.AtoStsEndpoint]));
            wsTrustTokenParameters.KeyType = SecurityKeyType.SymmetricKey;
            wsTrustTokenParameters.Claims = wsMessageHelper.GetRequiredClaimTypes();
            wsTrustTokenParameters.CacheIssuedTokens = false;
            if (TimeSpan.TryParse(configuration[SettingsKey.TokenLifeTime], out TimeSpan timeSpan))
            {
                wsTrustTokenParameters.AdditionalRequestParameters.Add(wsMessageHelper.GetLifeTimeElement(timeSpan));
            }

            ClientCredentials clientCredentials = new()
            {
                ClientCertificate =
                {
                    Certificate = ausKeyManager.GetX509Certificate()
                }
            };
            WSTrustChannelClientCredentials wsTrustChannelClientCredentials = new(clientCredentials);
            var securityTokenManager = wsTrustChannelClientCredentials.CreateSecurityTokenManager();
            SecurityTokenRequirement securityTokenRequirement = new()
            {
                TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0"
            };
            const string prefix = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement";
            securityTokenRequirement.Properties[$"{prefix}/IssuedSecurityTokenParameters"] = wsTrustTokenParameters;
            if (!Uri.TryCreate(configuration[SettingsKey.TokenAppliesTo], UriKind.Absolute, out var appliesToUrl))
            {
                appliesToUrl = new Uri(configuration[SettingsKey.UsiServiceEndpoint] ?? throw new InvalidOperationException());
            }

            securityTokenRequirement.Properties[$"{prefix}/TargetAddress"] = new EndpointAddress(appliesToUrl);
            securityTokenRequirement.Properties[$"{prefix}/SecurityAlgorithmSuite"] = SecurityAlgorithmSuite.Basic256Sha256;
            var securityTokenProvider = securityTokenManager.CreateSecurityTokenProvider(securityTokenRequirement);
            ((ICommunicationObject)securityTokenProvider).Open();
            Logger.LogDebug("Getting token from {endpoint} for {appliesTo}...", configuration[SettingsKey.AtoStsEndpoint], appliesToUrl);
            s_securityToken = securityTokenProvider.GetToken(TimeSpan.FromMinutes(2));
            Logger.LogDebug("Security token obtained. It's valid from {from} (UTC) to {to} (UTC) of type {name}.", s_securityToken.ValidFrom, s_securityToken.ValidTo, s_securityToken.GetType().Name);
        }

        wsTrustTokenParameters = new WSTrustTokenParameters
        {
            TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0"
        };
        WSFederationHttpBinding wsFederationHttpBinding = new(wsTrustTokenParameters)
        {
            Security =
            {
                Mode = SecurityMode.TransportWithMessageCredential,
                Message =
                {
                    ClientCredentialType = MessageCredentialType.Certificate,
                    EstablishSecurityContext = false,
                    NegotiateServiceCredential = false
                }
            }
        };
        ChannelFactory<IUSIService> channelFactory = new(wsFederationHttpBinding, new EndpointAddress(configuration[SettingsKey.UsiServiceEndpoint]));
        channelFactory.Endpoint.EndpointBehaviors.Remove(typeof(ClientCredentials));
        SamlClientCredentials samlClientCredentials = new(s_securityToken)
        {
            ClientCertificate =
            {
                Certificate = ausKeyManager.GetX509Certificate()
            }
        };
        channelFactory.Endpoint.EndpointBehaviors.Add(samlClientCredentials);
        return channelFactory.CreateChannel();
    }
}
