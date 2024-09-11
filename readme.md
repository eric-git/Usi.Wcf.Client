# Sample .net 8 client app for USI Web services

This is an example for modern .net apps to consume USI WCF Web service.

## Authentication modes

There are 2 modes supported:

-   IssuedToken: manually call the STS service to obtain the security token
-   IssuerBinding: use nested configuration, the built-in channel calls the STS service to obtain the security token

See **appsettings.json** and **launchSettings.json** for details. The "IssuerBinding" mode is recommended.

## Testing accounts

There are 2 testing accounts used:

-   VA1802: this is an example for "ActAs", the first party is: 11000002568, the second is: 96312011219
-   VA1803: this is an example suits most of the simple cases

## Operations

This example invokes the following WCF operations for demo

-   http://usi.gov.au/2022/ws/GetCountries (GetCountriesAsync)
-   http://usi.gov.au/2022/ws/BulkVerifyUSI (BulkVerifyUSIAsync)
