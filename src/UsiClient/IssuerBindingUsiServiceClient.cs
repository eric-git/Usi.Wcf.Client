using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Federation;
using System.ServiceModel.Security;
using Common.Configuration;
using Common.Federation;
using Common.Keystore;
using Common.Logging;
using Common.ServiceClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UsiClient;

public class IssuerBindingUsiServiceClient(
    IKeystoreManager keystoreManager,
    IConfiguration configuration,
    ILogger<IUSIService> logger) : BaseUsiServiceClient(logger)
{
    protected override IUSIService GetChannel()
    {
        var (abn, certificate) = keystoreManager.GetX509CertificateData();
        WS2007HttpBinding ws2007HttpBinding = new(SecurityMode.TransportWithMessageCredential);
        ws2007HttpBinding.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.Basic256Sha256;
        ws2007HttpBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
        ws2007HttpBinding.Security.Message.EstablishSecurityContext = false;
        ws2007HttpBinding.Security.Message.NegotiateServiceCredential = false;
        var wsTrustTokenParameters = WSTrustTokenParameters.CreateWS2007FederationTokenParameters(ws2007HttpBinding, new EndpointAddress(configuration[SettingsKey.AtoStsEndpoint]));
        wsTrustTokenParameters.KeyType = SecurityKeyType.SymmetricKey;
        wsTrustTokenParameters.TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0";
        wsTrustTokenParameters.Claims = WsMessageHelper.GetRequiredClaimTypes();
        if (TimeSpan.TryParse(configuration[SettingsKey.TokenLifeTime], out var timeSpan))
        {
            wsTrustTokenParameters.AdditionalRequestParameters.Add(WsMessageHelper.GetLifeTimeElement(timeSpan));
            wsTrustTokenParameters.MaxIssuedTokenCachingTime = timeSpan;
        }

        var actAs = configuration[SettingsKey.ActAs];
        if (!string.IsNullOrWhiteSpace(actAs))
        {
            wsTrustTokenParameters.AdditionalRequestParameters.Add(WsMessageHelper.GetActAsElement(abn, actAs));
        }

        if (Uri.TryCreate(configuration[SettingsKey.TokenAppliesTo], UriKind.Absolute, out var uri))
        {
            wsTrustTokenParameters.AdditionalRequestParameters.Add(WsMessageHelper.GetAppliesToElement(uri));
        }

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
        var clientCredentials = (ClientCredentials)channelFactory.Endpoint.EndpointBehaviors[typeof(ClientCredentials)];
        clientCredentials.ClientCertificate.Certificate = certificate;
        channelFactory.Endpoint.EndpointBehaviors.Add(new UsiServiceClientEndpointBehavior(Logger));
        return channelFactory.CreateChannel();
    }
}
