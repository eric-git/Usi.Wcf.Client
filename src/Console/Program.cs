using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Configuration;
using Common.Logging;
using Common.ServiceClient;
using Console.Properties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UsiClient;
using SystemConsole = System.Console;

var hostApplicationBuilder = Host.CreateApplicationBuilder();
hostApplicationBuilder.Configuration
                      .SetBasePath(AppContext.BaseDirectory)
                      .AddJsonFile("appsettings.json", false, true)
                      .AddEnvironmentVariables();
hostApplicationBuilder.Services.AddUsiClient(hostApplicationBuilder.Configuration, out var clientMode);
hostApplicationBuilder.Build();

var serviceProvider = hostApplicationBuilder.Services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
SystemConsole.Clear();
var orgCode = hostApplicationBuilder.Configuration[SettingsKey.UsiOrgCode] ?? throw new InvalidOperationException($"{SettingsKey.UsiOrgCode} required.");
Log.ClientModeInfo(logger, clientMode, orgCode);
var usiServiceClient = serviceProvider.GetRequiredService<IUSIService>();
JsonSerializerOptions jsonSerializerOptions = new()
{
    WriteIndented = true
};
jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
const int maxRecords = 3;

Log.InvokingCountries(logger, nameof(IUSIService.GetCountriesAsync), maxRecords);
GetCountriesRequest getCountriesRequest = new()
{
    GetCountries = new GetCountriesType
    {
        OrgCode = orgCode
    }
};
var getCountriesResponse = await usiServiceClient.GetCountriesAsync(getCountriesRequest).ConfigureAwait(false);
SystemConsole.WriteLine();
SystemConsole.WriteLine(JsonSerializer.Serialize(getCountriesResponse.GetCountriesResponse1.Countries.Take(maxRecords), jsonSerializerOptions));
SystemConsole.WriteLine();

Log.InvokingBulkVerify(logger, nameof(IUSIService.BulkVerifyUSIAsync), maxRecords);
VerificationType[] records =
[
    new()
    {
        RecordId = 1,
        USI = "XNY5NV9WG9",
        DateOfBirth = DateTime.Parse("2022-06-07", CultureInfo.CurrentCulture),
        Items = ["Amy"],
        ItemsElementName = [ItemsChoiceType1.SingleName]
    },
    new()
    {
        RecordId = 2,
        USI = "HQ9HHNJC3J",
        DateOfBirth = DateTime.Parse("1986-04-22", CultureInfo.CurrentCulture),
        Items = ["BERT", "ZYWIEC"],
        ItemsElementName = [ItemsChoiceType1.FirstName, ItemsChoiceType1.FamilyName]
    },
    new()
    {
        RecordId = 3,
        USI = "XNY5NV9WG8",
        DateOfBirth = DateTime.Parse("2022-06-07", CultureInfo.CurrentCulture),
        Items = ["Amy"],
        ItemsElementName = [ItemsChoiceType1.SingleName]
    }
];
BulkVerifyUSIRequest bulkVerifyUsiRequest = new()
{
    BulkVerifyUSI = new BulkVerifyUSIType
    {
        OrgCode = orgCode,
        NoOfVerifications = records.Length,
        Verifications = records
    }
};
var bulkVerifyUsiResponse = await usiServiceClient.BulkVerifyUSIAsync(bulkVerifyUsiRequest).ConfigureAwait(false);
SystemConsole.WriteLine();
SystemConsole.WriteLine(JsonSerializer.Serialize(bulkVerifyUsiResponse.BulkVerifyUSIResponse1.VerificationResponses.Take(maxRecords), jsonSerializerOptions));
SystemConsole.WriteLine();

SystemConsole.WriteLine(Resources.ExitMessage);
SystemConsole.ReadLine();