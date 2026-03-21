using System.ServiceModel;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace Common.ServiceClient;

public abstract class BaseUsiServiceClient(ILogger<IUSIService> logger) : IUSIService
{
    private readonly Lock _sync = new();

    protected ILogger<IUSIService> Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));

    private ChannelFactory<IUSIService> ChannelFactory
    {
        get
        {
            if (field is not null)
            {
                return field;
            }

            lock (_sync)
            {
                return field = CreateChannelFactory();
            }
        }
    }

    public async Task<BulkUploadResponse> BulkUploadAsync(BulkUploadRequest request)
    {
        Log.OperationDebug(Logger, nameof(BulkUploadAsync));
        var usiServiceClient = ChannelFactory.CreateChannel();
        return await usiServiceClient.BulkUploadAsync(request).ConfigureAwait(false);
    }

    public async Task<BulkVerifyUSIResponse> BulkVerifyUSIAsync(BulkVerifyUSIRequest request)
    {
        Log.OperationDebug(Logger, nameof(BulkVerifyUSIAsync));
        var usiServiceClient = ChannelFactory.CreateChannel();
        return await usiServiceClient.BulkVerifyUSIAsync(request).ConfigureAwait(false);
    }

    public async Task<VerifyUSIResponse> VerifyUSIAsync(VerifyUSIRequest request)
    {
        Log.OperationDebug(Logger, nameof(VerifyUSIAsync));
        var usiServiceClient = ChannelFactory.CreateChannel();
        return await usiServiceClient.VerifyUSIAsync(request).ConfigureAwait(false);
    }

    public async Task<BulkUploadRetrieveResponse> BulkUploadRetrieveAsync(BulkUploadRetrieveRequest request)
    {
        Log.OperationDebug(Logger, nameof(BulkUploadRetrieveAsync));
        var usiServiceClient = ChannelFactory.CreateChannel();
        return await usiServiceClient.BulkUploadRetrieveAsync(request).ConfigureAwait(false);
    }

    public async Task<CreateUSIResponse> CreateUSIAsync(CreateUSIRequest request)
    {
        Log.OperationDebug(Logger, nameof(CreateUSIAsync));
        var usiServiceClient = ChannelFactory.CreateChannel();
        return await usiServiceClient.CreateUSIAsync(request).ConfigureAwait(false);
    }

    public async Task<GetNonDvsDocumentTypesResponse> GetNonDvsDocumentTypesAsync(GetNonDvsDocumentTypesRequest request)
    {
        Log.OperationDebug(Logger, nameof(GetNonDvsDocumentTypesAsync));
        var usiServiceClient = ChannelFactory.CreateChannel();
        return await usiServiceClient.GetNonDvsDocumentTypesAsync(request).ConfigureAwait(false);
    }

    public async Task<UpdateUSIContactDetailsResponse> UpdateUSIContactDetailsAsync(UpdateUSIContactDetailsRequest request)
    {
        Log.OperationDebug(Logger, nameof(UpdateUSIContactDetailsAsync));
        var usiServiceClient = ChannelFactory.CreateChannel();
        return await usiServiceClient.UpdateUSIContactDetailsAsync(request).ConfigureAwait(false);
    }

    public async Task<UpdateUSIPersonalDetailsResponse> UpdateUSIPersonalDetailsAsync(UpdateUSIPersonalDetailsRequest request)
    {
        Log.OperationDebug(Logger, nameof(UpdateUSIPersonalDetailsAsync));
        var usiServiceClient = ChannelFactory.CreateChannel();
        return await usiServiceClient.UpdateUSIPersonalDetailsAsync(request).ConfigureAwait(false);
    }

    public async Task<LocateUSIResponse> LocateUSIAsync(LocateUSIRequest request)
    {
        Log.OperationDebug(Logger, nameof(LocateUSIAsync));
        var usiServiceClient = ChannelFactory.CreateChannel();
        return await usiServiceClient.LocateUSIAsync(request).ConfigureAwait(false);
    }

    public async Task<GetCountriesResponse> GetCountriesAsync(GetCountriesRequest request)
    {
        Log.OperationDebug(Logger, nameof(GetCountriesAsync));
        var usiServiceClient = ChannelFactory.CreateChannel();
        return await usiServiceClient.GetCountriesAsync(request).ConfigureAwait(false);
    }

    protected abstract ChannelFactory<IUSIService> CreateChannelFactory();
}