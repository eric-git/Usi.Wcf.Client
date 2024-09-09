using Microsoft.Extensions.Logging;

namespace Common.ServiceClient;

public abstract class BaseUsiServiceClient(ILogger<IUSIService> logger) : IUSIService
{
    protected ILogger<IUSIService> Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<BulkUploadResponse> BulkUploadAsync(BulkUploadRequest request)
    {
        Logger.LogDebug("Calling {methodName}...", nameof(BulkUploadAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.BulkUploadAsync(request);
    }

    public async Task<BulkVerifyUSIResponse> BulkVerifyUSIAsync(BulkVerifyUSIRequest request)
    {
        Logger.LogDebug("Calling {methodName}...", nameof(BulkVerifyUSIAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.BulkVerifyUSIAsync(request);
    }

    public async Task<VerifyUSIResponse> VerifyUSIAsync(VerifyUSIRequest request)
    {
        Logger.LogDebug("Calling {methodName}...", nameof(VerifyUSIAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.VerifyUSIAsync(request);
    }

    public async Task<BulkUploadRetrieveResponse> BulkUploadRetrieveAsync(BulkUploadRetrieveRequest request)
    {
        Logger.LogDebug("Calling {methodName}...", nameof(BulkUploadRetrieveAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.BulkUploadRetrieveAsync(request);
    }

    public async Task<CreateUSIResponse> CreateUSIAsync(CreateUSIRequest request)
    {
        Logger.LogDebug("Calling {methodName}...", nameof(CreateUSIAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.CreateUSIAsync(request);
    }

    public async Task<GetNonDvsDocumentTypesResponse> GetNonDvsDocumentTypesAsync(GetNonDvsDocumentTypesRequest request)
    {
        Logger.LogDebug("Calling {methodName}...", nameof(GetNonDvsDocumentTypesAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.GetNonDvsDocumentTypesAsync(request);
    }

    public async Task<UpdateUSIContactDetailsResponse> UpdateUSIContactDetailsAsync(UpdateUSIContactDetailsRequest request)
    {
        Logger.LogDebug("Calling {methodName}...", nameof(UpdateUSIContactDetailsAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.UpdateUSIContactDetailsAsync(request);
    }

    public async Task<UpdateUSIPersonalDetailsResponse> UpdateUSIPersonalDetailsAsync(UpdateUSIPersonalDetailsRequest request)
    {
        Logger.LogDebug("Calling {methodName}...", nameof(UpdateUSIPersonalDetailsAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.UpdateUSIPersonalDetailsAsync(request);
    }

    public async Task<LocateUSIResponse> LocateUSIAsync(LocateUSIRequest request)
    {
        Logger.LogDebug("Calling {methodName}...", nameof(LocateUSIAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.LocateUSIAsync(request);
    }

    public async Task<GetCountriesResponse> GetCountriesAsync(GetCountriesRequest request)
    {
        Logger.LogDebug("Calling {methodName}...", nameof(GetCountriesAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.GetCountriesAsync(request);
    }

    protected abstract IUSIService GetChannel();
}
