using Microsoft.Extensions.Logging;

namespace Common.ServiceClient;

public abstract class BaseUsiServiceClient(ILogger<IUSIService> logger) : IUSIService
{
    protected ILogger<IUSIService> Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<BulkUploadResponse> BulkUploadAsync(BulkUploadRequest request)
    {
        Logger.LogInformation("Calling {0}...", nameof(BulkUploadAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.BulkUploadAsync(request);
    }

    public async Task<BulkVerifyUSIResponse> BulkVerifyUSIAsync(BulkVerifyUSIRequest request)
    {
        Logger.LogInformation("Calling {0}...", nameof(BulkVerifyUSIAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.BulkVerifyUSIAsync(request);
    }

    public async Task<VerifyUSIResponse> VerifyUSIAsync(VerifyUSIRequest request)
    {
        Logger.LogInformation("Calling {0}...", nameof(VerifyUSIAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.VerifyUSIAsync(request);
    }

    public async Task<BulkUploadRetrieveResponse> BulkUploadRetrieveAsync(BulkUploadRetrieveRequest request)
    {
        Logger.LogInformation("Calling {0}...", nameof(BulkUploadRetrieveAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.BulkUploadRetrieveAsync(request);
    }

    public async Task<CreateUSIResponse> CreateUSIAsync(CreateUSIRequest request)
    {
        Logger.LogInformation("Calling {0}...", nameof(CreateUSIAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.CreateUSIAsync(request);
    }

    public async Task<GetNonDvsDocumentTypesResponse> GetNonDvsDocumentTypesAsync(GetNonDvsDocumentTypesRequest request)
    {
        Logger.LogInformation("Calling {0}...", nameof(GetNonDvsDocumentTypesAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.GetNonDvsDocumentTypesAsync(request);
    }

    public async Task<UpdateUSIContactDetailsResponse> UpdateUSIContactDetailsAsync(UpdateUSIContactDetailsRequest request)
    {
        Logger.LogInformation("Calling {0}...", nameof(UpdateUSIContactDetailsAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.UpdateUSIContactDetailsAsync(request);
    }

    public async Task<UpdateUSIPersonalDetailsResponse> UpdateUSIPersonalDetailsAsync(UpdateUSIPersonalDetailsRequest request)
    {
        Logger.LogInformation("Calling {0}...", nameof(UpdateUSIPersonalDetailsAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.UpdateUSIPersonalDetailsAsync(request);
    }

    public async Task<LocateUSIResponse> LocateUSIAsync(LocateUSIRequest request)
    {
        Logger.LogInformation("Calling {0}...", nameof(LocateUSIAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.LocateUSIAsync(request);
    }

    public async Task<GetCountriesResponse> GetCountriesAsync(GetCountriesRequest request)
    {
        Logger.LogInformation("Calling {0}...", nameof(GetCountriesAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.GetCountriesAsync(request);
    }

    protected abstract IUSIService GetChannel();
}