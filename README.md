# Sample .NET 10 Client App for USI Web Services

This is an example for modern .NET apps to consume USI WCF Web service.

---

## Authentication Modes

Two modes are supported:

-   **IssuedToken**: manually call the STS service to obtain the security token.
-   **IssuerBinding**: use nested configuration; the built‑in channel calls the STS service to obtain the security token.

See [appsettings.json](./src/Console/appsettings.json) and [launchSettings.json](./src/Console/Properties/launchSettings.json) for details.

---

## Testing Accounts

Two testing accounts are provided:

-   **VA1802**: example for "ActAs"
    -   First party: `11000002568`
    -   Second party: `96312011219`
-   **VA1803**: example that suits most simple cases.

---

## Operations

This example invokes the following WCF operations:

-   `http://usi.gov.au/2022/ws/GetCountries` → `GetCountriesAsync`
-   `http://usi.gov.au/2022/ws/BulkVerifyUSI` → `BulkVerifyUSIAsync`

---

## Prerequisites

-   .NET 10 SDK installed
-   Visual Studio 2022 or VS Code
-   Access to USI test endpoints
-   Valid test account credentials

---

## Running the Sample

```bash
dotnet build
dotnet run --project ./src/Console
```
