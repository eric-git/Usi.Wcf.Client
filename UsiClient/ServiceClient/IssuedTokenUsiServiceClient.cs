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

namespace UsiClient.ServiceClient;

public class IssuedTokenUsiServiceClient(IAusKeyManager ausKeyManager,
        IWsMessageHelper wsMessageHelper,
        IConfiguration configuration,
        ILogger<IUSIService> logger)
    : BaseUsiServiceClient(logger)
{
    private static SecurityToken _securityToken;
    private readonly IAusKeyManager _ausKeyManager = ausKeyManager ?? throw new ArgumentNullException(nameof(ausKeyManager));
    private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly IWsMessageHelper _wsMessageHelper = wsMessageHelper ?? throw new ArgumentNullException(nameof(wsMessageHelper));

    protected override IUSIService GetChannel()
    {
        var now = DateTime.UtcNow;
        WSTrustTokenParameters wsTrustTokenParameters;
        if (_securityToken == null || _securityToken.ValidFrom > now || _securityToken.ValidTo <= now)
        {
            WS2007HttpBinding ws2007HttpBinding = new(SecurityMode.TransportWithMessageCredential);
            ws2007HttpBinding.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.Basic256Sha256;
            ws2007HttpBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
            ws2007HttpBinding.Security.Message.EstablishSecurityContext = false;
            ws2007HttpBinding.Security.Message.NegotiateServiceCredential = false;
            wsTrustTokenParameters = WSTrustTokenParameters.CreateWS2007FederationTokenParameters(ws2007HttpBinding, new EndpointAddress(_configuration[SettingsKey.AtoStsEndpoint]));
            wsTrustTokenParameters.KeyType = SecurityKeyType.SymmetricKey;
            wsTrustTokenParameters.Claims = _wsMessageHelper.GetRequiredClaimTypes();
            wsTrustTokenParameters.CacheIssuedTokens = false;
            if (TimeSpan.TryParse(_configuration[SettingsKey.TokenLifeTime], out var timeSpan))
            {
                wsTrustTokenParameters.AdditionalRequestParameters.Add(_wsMessageHelper.GetLifeTimeElement(timeSpan));
            }

            ClientCredentials clientCredentials = new()
            {
                ClientCertificate =
                {
                    Certificate = _ausKeyManager.GetX509Certificate()
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
            if (!Uri.TryCreate(_configuration[SettingsKey.TokenAppliesTo], UriKind.Absolute, out var appliesToUrl))
            {
                appliesToUrl = new Uri(_configuration[SettingsKey.UsiServiceEndpoint] ?? throw new InvalidOperationException());
            }

            securityTokenRequirement.Properties[$"{prefix}/TargetAddress"] = new EndpointAddress(appliesToUrl);
            securityTokenRequirement.Properties[$"{prefix}/SecurityAlgorithmSuite"] = SecurityAlgorithmSuite.Basic256Sha256;
            var securityTokenProvider = securityTokenManager.CreateSecurityTokenProvider(securityTokenRequirement);
            ((ICommunicationObject)securityTokenProvider).Open();
            Logger.LogInformation("Getting token from {0} for {1}...", _configuration[SettingsKey.AtoStsEndpoint], appliesToUrl);
            _securityToken = securityTokenProvider.GetToken(TimeSpan.FromMinutes(2));
            Logger.LogInformation("Security token obtained. It's valid from {0} to {1} of type {2}.", _securityToken.ValidFrom, _securityToken.ValidTo, _securityToken.GetType().Name);
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
        ChannelFactory<IUSIService> channelFactory = new(wsFederationHttpBinding, new EndpointAddress(_configuration[SettingsKey.UsiServiceEndpoint]));
        channelFactory.Endpoint.EndpointBehaviors.Remove(typeof(ClientCredentials));
        SamlClientCredentials samlClientCredentials = new(_securityToken)
        {
            ClientCertificate =
            {
                Certificate = _ausKeyManager.GetX509Certificate()
            }
        };
        channelFactory.Endpoint.EndpointBehaviors.Add(samlClientCredentials);
        return channelFactory.CreateChannel();
    }
}