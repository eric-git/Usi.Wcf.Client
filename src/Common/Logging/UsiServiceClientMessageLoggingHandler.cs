using Microsoft.Extensions.Logging;

namespace Common.Logging;

public class UsiServiceClientMessageLoggingHandler(HttpMessageHandler innerHandler, ILogger logger) : DelegatingHandler(innerHandler)
{
  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
  {
    if (request.Content is not null)
    {
      var requestContent = await request.Content.ReadAsStringAsync(cancellationToken);
      logger.LogDebug("Request:{newLine}{content}", Environment.NewLine, requestContent);
    }

    var response = await base.SendAsync(request, cancellationToken);
    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
    logger.LogDebug("Response:{newLine}{content}", Environment.NewLine, responseContent);
    return response;
  }
}
