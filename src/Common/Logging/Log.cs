using Common.Configuration;
using Microsoft.Extensions.Logging;

namespace Common.Logging;

public static partial class Log
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Debug, Message = "Getting token from {Endpoint} for {AppliesTo}...")]
    public static partial void GettingToken(ILogger logger, string endpoint, Uri appliesTo);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Debug, Message = "Security token obtained. It's valid from {from} (UTC) to {to} (UTC) of type {name}.")]
    public static partial void ObtainedToken(ILogger logger, DateTime from, DateTime to, string name);

    [LoggerMessage(EventId = 2001, Level = LogLevel.Information, Message = "Client mode: {Mode}, Organisation code: {OrgCode}")]
    public static partial void ClientModeInfo(ILogger logger, ClientMode mode, string orgCode);

    [LoggerMessage(EventId = 3001, Level = LogLevel.Debug, Message = "Invoking operation {Operation}...")]
    public static partial void OperationDebug(ILogger logger, string operation);

    [LoggerMessage(EventId = 3002, Level = LogLevel.Information, Message = "Invoking {Operation}, the top {MaxRecords} country data records will be displayed...")]
    public static partial void InvokingCountries(ILogger logger, string operation, int maxRecords);

    [LoggerMessage(EventId = 3003, Level = LogLevel.Information, Message = "Invoking {Operation}, the top {MaxRecords} USI verification data records will be displayed...")]
    public static partial void InvokingBulkVerify(ILogger logger, string operation, int maxRecords);

    [LoggerMessage(EventId = 4001, Level = LogLevel.Debug, Message = "Request:\n{Content}")]
    public static partial void HttpRequestDebug(ILogger logger, string content);

    [LoggerMessage(EventId = 4002, Level = LogLevel.Debug, Message = "Response:\n{Content}")]
    public static partial void HttpResponseDebug(ILogger logger, string content);
}