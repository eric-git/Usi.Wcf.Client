using Microsoft.Extensions.Logging;

namespace Common.ServiceClient;

public abstract class BaseUsiServiceClient(ILogger<IUsiService> logger) : IUsiService
{
    protected ILogger<IUsiService> Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<EchoResponse> EchoAsync(EchoRequest request)
    {
        Logger.LogDebug("Calling {operation}...", nameof(EchoAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.EchoAsync(request);
    }

    public async Task<FuzzySearchResponse> FuzzyAsync(FuzzySearchRequest request)
    {
        Logger.LogDebug("Calling {operation}...", nameof(FuzzyAsync));
        var usiServiceClient = GetChannel();
        return await usiServiceClient.FuzzyAsync(request);
    }

    protected abstract IUsiService GetChannel();
}
