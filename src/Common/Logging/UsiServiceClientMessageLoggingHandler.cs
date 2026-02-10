using Microsoft.Extensions.Logging;

namespace Common.Logging;

public class UsiServiceClientMessageLoggingHandler(HttpMessageHandler innerHandler, ILogger logger) : DelegatingHandler(innerHandler)
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Content is not null)
        {
            var requestContent = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            Log.HttpRequestDebug(logger, requestContent);
        }

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        Log.HttpResponseDebug(logger, responseContent);
        return response;
    }
}