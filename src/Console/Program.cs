using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Configuration;
using Common.ServiceClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UsiClient;

var hostApplicationBuilder = Host.CreateApplicationBuilder();
hostApplicationBuilder.Services.AddUsiClient(hostApplicationBuilder.Configuration, out var clientMode);
hostApplicationBuilder.Build();

var serviceProvider = hostApplicationBuilder.Services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
Console.Clear();
var orgCode = hostApplicationBuilder.Configuration[SettingsKey.UsiOrgCode] ?? throw new ApplicationException();
logger.LogInformation("Client mode: {mode}, Organisation code: {orgCode}", clientMode, orgCode);
var usiServiceClient = serviceProvider.GetRequiredService<IUsiService>();
JsonSerializerOptions jsonSerializerOptions = new() { WriteIndented = true };
jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

Console.WriteLine();
EchoRequest echoRequest = new()
{
    OrgCode = orgCode
};
EchoResponse? echoResponse;
try
{
    echoResponse = usiServiceClient.EchoAsync(echoRequest).Result;
}
catch (AggregateException aggregateException)
{
    echoResponse = null;
    var flattened = aggregateException.Flatten();
    foreach (var item in flattened.InnerExceptions)
    {
        logger.LogError(item, item.Message);
    }
}
catch (Exception exception)
{
    echoResponse = null;
    logger.LogError(exception, exception.Message);
}

if (echoResponse is not null)
{
    Console.WriteLine(JsonSerializer.Serialize(echoResponse, jsonSerializerOptions));
}

Console.WriteLine();
FuzzySearchRequest fuzzySearchRequest = new()
{
    OrgCode = orgCode,
    FirstName = "Argentina",
    FamilyName = "Abdullah",
    DateOfBirth = DateTime.Parse("1992-09-04"),
    GenderCode = "F",
    CountryOfBirthCode = "1101"
};
FuzzySearchResponse? fuzzySearchResponse;
try
{
    fuzzySearchResponse = usiServiceClient.FuzzyAsync(fuzzySearchRequest).Result;
}
catch (AggregateException aggregateException)
{
    fuzzySearchResponse = null;
    var flattened = aggregateException.Flatten();
    foreach (var item in flattened.InnerExceptions)
    {
        logger.LogError(item, item.Message);
    }
}
catch (Exception exception)
{
    fuzzySearchResponse = null;
    logger.LogError(exception, exception.Message);
}

if (fuzzySearchResponse is not null)
{
    Console.WriteLine(JsonSerializer.Serialize(fuzzySearchResponse, jsonSerializerOptions));
}

Console.WriteLine();
Console.WriteLine("Press enter to exit...");
Console.ReadLine();
