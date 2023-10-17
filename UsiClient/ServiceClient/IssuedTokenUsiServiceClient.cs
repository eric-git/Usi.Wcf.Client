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

public class IssuedTokenUsiServiceClient : BaseUsiServiceClient
{
    private static SecurityToken SecurityToken;
    private readonly IAusKeyManager _ausKeyManager;
    private readonly IConfiguration _configuration;
    private readonly IWSMessageHelper _wsMessageHelper;

    public IssuedTokenUsiServiceClient(IAusKeyManager ausKeyManager,
        IWSMessageHelper wsMessageHelper,
        IConfiguration configuration,
        ILogger<IUSIService> logger) : base(logger)
    {
        _ausKeyManager = ausKeyManager ?? throw new ArgumentNullException(nameof(ausKeyManager));
        _wsMessageHelper = wsMessageHelper ?? throw new ArgumentNullException(nameof(wsMessageHelper));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    protected override IUSIService GetChannel()
    {
        var now = DateTime.UtcNow;
        WSTrustTokenParameters wsTrustTokenParameters;
        if (SecurityToken == null || SecurityToken.ValidFrom > now || SecurityToken.ValidTo <= now)
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
                appliesToUrl = new Uri(_configuration[SettingsKey.UsiServiceEndpoint]);
            }

            securityTokenRequirement.Properties[$"{prefix}/TargetAddress"] = new EndpointAddress(appliesToUrl);
            securityTokenRequirement.Properties[$"{prefix}/SecurityAlgorithmSuite"] = SecurityAlgorithmSuite.Basic256Sha256;
            var securityTokenProvider = securityTokenManager.CreateSecurityTokenProvider(securityTokenRequirement);
            ((ICommunicationObject)securityTokenProvider).Open();
            Logger.LogInformation("Getting token from {0} for {1}...", _configuration[SettingsKey.AtoStsEndpoint], appliesToUrl);
            SecurityToken = securityTokenProvider.GetToken(TimeSpan.FromMinutes(2));
            Logger.LogInformation("Security token obatined. It's valid from {0} to {1} of type {2}.", SecurityToken.ValidFrom, SecurityToken.ValidTo, SecurityToken.GetType().Name);
        }

        wsTrustTokenParameters = new WSTrustTokenParameters
        {
            TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0"
        };
        WSFederationHttpBinding wsFederationHttpBinding = new(wsTrustTokenParameters);
        wsFederationHttpBinding.Security.Mode = SecurityMode.TransportWithMessageCredential;
        wsFederationHttpBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
        wsFederationHttpBinding.Security.Message.EstablishSecurityContext = false;
        wsFederationHttpBinding.Security.Message.NegotiateServiceCredential = false;
        ChannelFactory<IUSIService> channelFactory = new(wsFederationHttpBinding, new EndpointAddress(_configuration[SettingsKey.UsiServiceEndpoint]));
        channelFactory.Endpoint.EndpointBehaviors.Remove(typeof(ClientCredentials));
        SamlClientCredentials samlClientCredentials = new(SecurityToken)
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