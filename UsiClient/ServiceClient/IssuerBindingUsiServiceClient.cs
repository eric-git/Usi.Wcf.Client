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

public class IssuerBindingUsiServiceClient : BaseUsiServiceClient
{
    private readonly IAusKeyManager _ausKeyManager;
    private readonly IConfiguration _configuration;
    private readonly IWSMessageHelper _wsMessageHelper;

    public IssuerBindingUsiServiceClient(
        IAusKeyManager ausKeyManager,
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
        WS2007HttpBinding ws2007HttpBinding = new(SecurityMode.TransportWithMessageCredential);
        ws2007HttpBinding.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.Basic256Sha256;
        ws2007HttpBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
        ws2007HttpBinding.Security.Message.EstablishSecurityContext = false;
        ws2007HttpBinding.Security.Message.NegotiateServiceCredential = false;
        var wsTrustTokenParameters = WSTrustTokenParameters.CreateWS2007FederationTokenParameters(ws2007HttpBinding, new EndpointAddress(_configuration[SettingsKey.AtoStsEndpoint]));
        wsTrustTokenParameters.KeyType = SecurityKeyType.SymmetricKey;
        wsTrustTokenParameters.TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0";
        wsTrustTokenParameters.Claims = _wsMessageHelper.GetRequiredClaimTypes();
        if (TimeSpan.TryParse(_configuration[SettingsKey.TokenLifeTime], out var timeSpan))
        {
            wsTrustTokenParameters.AdditionalRequestParameters.Add(_wsMessageHelper.GetLifeTimeElement(timeSpan));
            wsTrustTokenParameters.MaxIssuedTokenCachingTime = timeSpan;
        }

        if (Uri.TryCreate(_configuration[SettingsKey.TokenAppliesTo], UriKind.Absolute, out var uri))
        {
            wsTrustTokenParameters.AdditionalRequestParameters.Add(_wsMessageHelper.GetAppliesToElement(uri));
        }

        WSFederationHttpBinding wsFederationHttpBinding = new(wsTrustTokenParameters);
        wsFederationHttpBinding.Security.Mode = SecurityMode.TransportWithMessageCredential;
        wsFederationHttpBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
        wsFederationHttpBinding.Security.Message.EstablishSecurityContext = false;
        wsFederationHttpBinding.Security.Message.NegotiateServiceCredential = false;
        ChannelFactory<IUSIService> channelFactory = new(wsFederationHttpBinding, new EndpointAddress(_configuration[SettingsKey.UsiServiceEndpoint]));
        var clientCredentials = (ClientCredentials)channelFactory.Endpoint.EndpointBehaviors[typeof(ClientCredentials)];
        clientCredentials.ClientCertificate.Certificate = _ausKeyManager.GetX509Certificate();
        return channelFactory.CreateChannel();
    }
}