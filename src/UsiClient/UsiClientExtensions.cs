using Common.AusKey;
using Common.Configuration;
using Common.Federation;
using Common.ServiceClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UsiClient;

public static class UsiClientExtensions
{
    public static IServiceCollection AddUsiClient(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddTransient<IAusKeyManager, AusKeyManager>()
            .AddTransient<IWsMessageHelper, WsMessageHelper>();
        if (Enum.TryParse(configuration[SettingsKey.Mode], true, out ClientMode clientMode) && clientMode == ClientMode.IssuedToken)
        {
            services.AddTransient<IUSIService, IssuedTokenUsiServiceClient>();
        }
        else
        {
            services.AddTransient<IUSIService, IssuerBindingUsiServiceClient>();
        }

        return services;
    }
}
