using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Federation;
using System.ServiceModel.Security;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.WsFed;
using Microsoft.IdentityModel.Protocols.WsTrust;

namespace UsiClient;

public class UsiServiceClient : IUSIService
{
    private readonly IAusKeyManager _ausKeyManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IUSIService> _logger;
    private readonly IWSMessageHelper _wsMeessageHelper;

    public UsiServiceClient(
        IAusKeyManager ausKeyManager,
        IWSMessageHelper wsMessageHelper,
        IConfiguration configuration,
        ILogger<IUSIService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _ausKeyManager = ausKeyManager ?? throw new ArgumentNullException(nameof(ausKeyManager));
        _wsMeessageHelper = wsMessageHelper ?? throw new ArgumentNullException(nameof(wsMessageHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BulkUploadResponse> BulkUploadAsync(BulkUploadRequest request)
    {
        _logger.LogInformation("Calling {0}...", nameof(BulkUploadAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.BulkUploadAsync(request);
    }

    public async Task<BulkVerifyUSIResponse> BulkVerifyUSIAsync(BulkVerifyUSIRequest request)
    {
        _logger.LogInformation("Calling {0}...", nameof(BulkVerifyUSIAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.BulkVerifyUSIAsync(request);
    }

    public async Task<VerifyUSIResponse> VerifyUSIAsync(VerifyUSIRequest request)
    {
        _logger.LogInformation("Calling {0}...", nameof(VerifyUSIAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.VerifyUSIAsync(request);
    }

    public async Task<BulkUploadRetrieveResponse> BulkUploadRetrieveAsync(BulkUploadRetrieveRequest request)
    {
        _logger.LogInformation("Calling {0}...", nameof(BulkUploadRetrieveAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.BulkUploadRetrieveAsync(request);
    }

    public async Task<CreateUSIResponse> CreateUSIAsync(CreateUSIRequest request)
    {
        _logger.LogInformation("Calling {0}...", nameof(CreateUSIAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.CreateUSIAsync(request);
    }

    public async Task<GetNonDvsDocumentTypesResponse> GetNonDvsDocumentTypesAsync(GetNonDvsDocumentTypesRequest request)
    {
        _logger.LogInformation("Calling {0}...", nameof(GetNonDvsDocumentTypesAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.GetNonDvsDocumentTypesAsync(request);
    }

    public async Task<UpdateUSIContactDetailsResponse> UpdateUSIContactDetailsAsync(UpdateUSIContactDetailsRequest request)
    {
        _logger.LogInformation("Calling {0}...", nameof(UpdateUSIContactDetailsAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.UpdateUSIContactDetailsAsync(request);
    }

    public async Task<UpdateUSIPersonalDetailsResponse> UpdateUSIPersonalDetailsAsync(UpdateUSIPersonalDetailsRequest request)
    {
        _logger.LogInformation("Calling {0}...", nameof(UpdateUSIPersonalDetailsAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.UpdateUSIPersonalDetailsAsync(request);
    }

    public async Task<LocateUSIResponse> LocateUSIAsync(LocateUSIRequest request)
    {
        _logger.LogInformation("Calling {0}...", nameof(LocateUSIAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.LocateUSIAsync(request);
    }

    public async Task<GetCountriesResponse> GetCountriesAsync(GetCountriesRequest request)
    {
        _logger.LogInformation("Calling {0}...", nameof(GetCountriesAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.GetCountriesAsync(request);
    }

    private IUSIService GetChannel()
    {
        _logger.LogInformation("Setting up USI service channel...");

        // ATO service binding
        WS2007HttpBinding ws2007HttpBinding = new(SecurityMode.TransportWithMessageCredential);
        ws2007HttpBinding.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.Basic256Sha256;
        ws2007HttpBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
        ws2007HttpBinding.Security.Message.EstablishSecurityContext = false;
        ws2007HttpBinding.Security.Message.NegotiateServiceCredential = false;
        var wsTrustTokenParameters = WSTrustTokenParameters.CreateWS2007FederationTokenParameters(ws2007HttpBinding, new EndpointAddress(_configuration[SettingsKey.AtoStsEndpoint]));
        wsTrustTokenParameters.KeyType = SecurityKeyType.SymmetricKey;
        wsTrustTokenParameters.TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0";
        wsTrustTokenParameters.Claims = new Claims("http://schemas.xmlsoap.org/ws/2005/05/identity", new[]
        {
            new ClaimType { IsOptional = false, Uri = "http://vanguard.ebusiness.gov.au/2008/06/identity/claims/abn", Value = "abn" },
            new ClaimType { IsOptional = false, Uri = "http://vanguard.ebusiness.gov.au/2008/06/identity/claims/credentialtype", Value = "D" }
        });

        if (TimeSpan.TryParse(_configuration[SettingsKey.TokenLifeTime], out var timeSpan))
        {
            var now = DateTime.UtcNow;
            Lifetime lifetime = new(now, now.Add(timeSpan));
            wsTrustTokenParameters.AdditionalRequestParameters.Add(_wsMeessageHelper.GetLifeTimeElement(lifetime));
        }

        var appliesToUrl = _configuration[SettingsKey.TokenAppliesTo];
        if (!string.IsNullOrWhiteSpace(appliesToUrl))
        {
            EndpointAddress endpointAddress = new(appliesToUrl);
            wsTrustTokenParameters.AdditionalRequestParameters.Add(_wsMeessageHelper.GetAppliesToElement(endpointAddress));
        }

        // USI service binding
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