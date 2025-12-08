using Common.Configuration;
using Common.Keystore;
using Common.ServiceClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UsiClient;

public static class UsiServiceClientExtensions
{
  public static IServiceCollection AddUsiClient(this IServiceCollection services, IConfiguration configuration, out ClientMode configuredMode)
  {
    ClientMode clientMode = default;
    var mode = configuration[SettingsKey.Mode];
    if (!string.IsNullOrWhiteSpace(mode) && !Enum.TryParse(mode, true, out clientMode))
    {
      throw new ApplicationException();
    }

    if (clientMode == ClientMode.IssuedToken)
    {
      services.AddTransient<IUSIService, IssuedTokenUsiServiceClient>();
    }
    else
    {
      services.AddTransient<IUSIService, IssuerBindingUsiServiceClient>();
    }

    services
        .AddTransient<IKeystoreManager, KeystoreManager>()
        .AddMemoryCache();
    configuredMode = clientMode;
    return services;
  }
}
