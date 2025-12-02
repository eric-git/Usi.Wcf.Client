# .NET Sample Application for USI Services

A sample .NET application demonstrating how modern .NET apps can consume the **USI WCF Web Service**.

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

---

## ⚙️ Operations

This sample demonstrates the following WCF operations:

-   `http://usi.gov.au/2022/ws/GetCountries` → `GetCountriesAsync`
-   `http://usi.gov.au/2022/ws/BulkVerifyUSI` → `BulkVerifyUSIAsync`

---

## 📦 Prerequisites

-   Latest .NET SDK LTS installed
-   Visual Studio 2022+ or VS Code

---

## ▶️ Running the Sample

```sh
dotnet build
dotnet run --project ./src/Console
```
