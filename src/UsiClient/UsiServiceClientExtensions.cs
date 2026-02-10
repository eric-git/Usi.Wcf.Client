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
        ArgumentNullException.ThrowIfNull(configuration);
        if (!Enum.TryParse(configuration[SettingsKey.Mode], true, out ClientMode clientMode))
        {
            throw new InvalidOperationException($"Invalid {SettingsKey.Mode} value.");
        }

        switch (clientMode)
        {
            case ClientMode.IssuedToken:
                services.AddTransient<IUSIService, IssuedTokenUsiServiceClient>();
                break;
            case ClientMode.IssuerBinding:
                services.AddTransient<IUSIService, IssuerBindingUsiServiceClient>();
                break;
            default:
                throw new NotSupportedException($"Invalid {SettingsKey.Mode} value.");
        }

        services.AddTransient<IKeystoreManager, KeystoreManager>();
        services.AddMemoryCache();
        configuredMode = clientMode;
        return services;
    }
}