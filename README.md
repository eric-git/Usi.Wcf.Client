# .NET Sample Application for USI Services

A sample .NET application demonstrating how modern .NET applications consume the **USI WCF Web Services**.

---

## 🔑 Authentication Modes

Two authentication modes are supported:

-   **IssuedToken**
    Manually call the STS service to obtain a security token.
-   **IssuerBinding**
    Use nested configuration; the built‑in channel automatically calls the STS service to obtain the token.

See [appsettings.json](./src/Console/appsettings.json) and [launchSettings.json](./src/Console/Properties/launchSettings.json) for configuration details.

---

## 🧪 Testing Accounts

Two test accounts are available:

-   **VA1802** → Example for _ActAs_ delegation
    -   First party: `11000002568`
    -   Second party: `96312011219`
-   **VA1803** → Example that suits most simple cases

See [keystore-usi.xml](./src/Console/keystore-usi.xml) for machine account settings.

---

## ⚙️ Operations

This sample demonstrates the following WCF operations:

-   `http://usi.gov.au/2022/ws/GetCountries` → `GetCountriesAsync`
-   `http://usi.gov.au/2022/ws/BulkVerifyUSI` → `BulkVerifyUSIAsync`

---

## 📦 Prerequisites

-   Latest .NET LTS SDK installed
-   Visual Studio 2022+ or Visual Studio Code
-   Access to USI test endpoints
-   Valid test account credentials

---

## ▶️ Running the Sample

You can run the application in **debug mode** using Visual Studio or Visual Studio Code, or directly from the CLI.

### PowerShell

```powershell
$env:DOTNET_ENVIRONMENT = "Development"
dotnet run --project ./src/Console
```

### Bash

```bash
export DOTNET_ENVIRONMENT=Development
dotnet run --project ./src/Console
```

---

## 🐛 Issues & Support

-   Raise bugs, requests, or discussions on [GitHub Issues](../../issues).
-   For security concerns, please see [SECURITY](SECURITY.md).
-   For general support, contact **it@usi.gov.au** or see [SUPPORT](SUPPORT.md).

---

## 📚 Additional Notes

-   Configuration files (`appsettings.json`, `launchSettings.json`, `keystore-usi.xml`) define authentication and runtime settings.
-   The sample is intended for **development and testing only**; do not use test accounts in production.
