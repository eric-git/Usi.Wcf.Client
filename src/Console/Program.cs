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
var usiServiceClient = serviceProvider.GetRequiredService<IUSIService>();
JsonSerializerOptions jsonSerializerOptions = new() { WriteIndented = true };
jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
const int maxRecords = 3;

logger.LogInformation("Invoking {operation}, the top {maxRecords} country data records will be displayed...", nameof(IUSIService.GetCountriesAsync), maxRecords);
GetCountriesRequest getCountriesRequest = new()
{
    GetCountries = new GetCountriesType { OrgCode = orgCode }
};
var getCountriesResponse = usiServiceClient.GetCountriesAsync(getCountriesRequest).Result;
Console.WriteLine();
Console.WriteLine(JsonSerializer.Serialize(getCountriesResponse.GetCountriesResponse1.Countries.Take(maxRecords), jsonSerializerOptions));
Console.WriteLine();

logger.LogInformation("Invoking {operation}, the top {maxRecords} USI verification data records will be displayed...", nameof(IUSIService.BulkVerifyUSIAsync), maxRecords);
VerificationType[] records =
[
    new()
    {
        RecordId = 1,
        USI = "XNY5NV9WG9",
        DateOfBirth = DateTime.Parse("2022-06-07"),
        Items = ["Amy"],
        ItemsElementName = [ItemsChoiceType1.SingleName]
    },
    new()
    {
        RecordId = 2,
        USI = "HQ9HHNJC3J",
        DateOfBirth = DateTime.Parse("1986-04-22"),
        Items = ["BERT", "ZYWIEC"],
        ItemsElementName = [ItemsChoiceType1.FirstName, ItemsChoiceType1.FamilyName]
    },
    new()
    {
        RecordId = 3,
        USI = "XNY5NV9WG8",
        DateOfBirth = DateTime.Parse("2022-06-07"),
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
var bulkVerifyUsiResponse = usiServiceClient.BulkVerifyUSIAsync(bulkVerifyUsiRequest).Result;
Console.WriteLine();
Console.WriteLine(JsonSerializer.Serialize(bulkVerifyUsiResponse.BulkVerifyUSIResponse1.VerificationResponses.Take(maxRecords), jsonSerializerOptions));
Console.WriteLine();
