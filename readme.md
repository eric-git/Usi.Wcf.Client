# Sample .net 8 client app for USI Web services

This is an example for modern .net apps to consume USI WCF Web service.

## Authentication modes

There are 2 modes supported:

-   IssuedToken: manually call the STS service to obtain the security token
-   IssuerBinding: use nested configuration, the built-in channel calls the STS service to obtain the security token

See **appsettings.json** and **launchSettings.json** for details. The "IssuerBinding" mode is recommended.

## Operations

This example invokes the following WCF operations for demo

-   http://usi.gov.au/2022/ws/GetCountries (GetCountriesAsync)
-   http://usi.gov.au/2022/ws/BulkVerifyUSI (BulkVerifyUSIAsync)
