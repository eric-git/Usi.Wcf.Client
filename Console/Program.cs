using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using UsiClient;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false)
    .Build();
ServiceCollection serviceCollection = new();
serviceCollection
    .AddSingleton<IConfiguration>(configuration)
    .AddTransient<IAusKeyManager, AusKeyManager>()
    .AddTransient<IWSMessageHelper, WSMessageHelper>()
    .AddLogging(config => { config.AddSimpleConsole(); });
if (!Enum.TryParse<ClientMode>(configuration[SettingsKey.Mode], out var clientMode) || clientMode == ClientMode.IssuedToken)
{
    serviceCollection.AddTransient<IUSIService, UsiClient.IssuedToken.UsiServiceClient>();
}
else
{
    serviceCollection.AddTransient<IUSIService, UsiClient.IssuerBinding.UsiServiceClient>();
}

var serviceProvider = serviceCollection.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
Console.Clear();
logger.LogInformation("Running on {0} mode...", clientMode);
try
{
    var usiServiceClient = serviceProvider.GetRequiredService<IUSIService>();
    GetCountriesRequest getCountriesRequest = new()
    {
        GetCountries = new GetCountriesType
        {
            OrgCode = configuration[SettingsKey.UsiOrgCode]
        }
    };
    var getCountriesResponse = usiServiceClient.GetCountriesAsync(getCountriesRequest).Result;
    foreach (var country in getCountriesResponse.GetCountriesResponse1.Countries.Take(10))
    {
        Console.WriteLine(country.Name);
    }
}
catch (Exception exception)
{
    logger.LogError(exception.GetBaseException().Message);
}

Console.WriteLine("Press enter to continue...");
Console.ReadLine();