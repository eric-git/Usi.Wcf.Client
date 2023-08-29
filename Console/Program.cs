using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UsiClient;
using UsiClient.IssuerBinding;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false)
    .Build();
ServiceCollection serviceCollection = new();
var serviceProvider = serviceCollection
    .AddSingleton<IConfiguration>(configuration)
    .AddTransient<IAusKeyManager, AusKeyManager>()
    .AddTransient<IWSMessageHelper, WSMessageHelper>()
    //.AddTransient<IUSIService, UsiClient.IssuedToken.UsiServiceClient>()
    .AddTransient<IUSIService, UsiServiceClient>()
    .AddLogging(config => { config.AddSimpleConsole(); })
    .BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
Console.Clear();
logger.LogInformation("Running...");
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