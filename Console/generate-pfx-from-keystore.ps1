<#
    .SYNOPSIS
    Generates a pfx file from the AUSKey file.

    .DESCRIPTION
    Generates a pfx certificate file from the AUSKey file based on the provided orgaisation credential name.
    The pfx file will be located under the same folder of this module. The file name is the credential name.

    .PARAMETER file
    Specifies the path to the AUSKey file, the default is .\keystore-usi.xml.

    .PARAMETER name
    Specifies the organisation credential name, the default is USIMachine.

    .PARAMETER password
    Specifies the password of the AUSKey file, the default is Password1!.

    .INPUTS
    None. You cannot pipe objects to this module.

    .OUTPUTS
    A pfx file contains a private key.

    .EXAMPLE
    .\generate-pfx-from-keystore.ps1

    .EXAMPLE
    .\generate-pfx-from-keystore.ps1 -file .\test.xml -name OrgOne -password Password123!

    .LINK
    https://github.com/eric-git/Usi.Wcf.Client
#>
param (
    [string] $file = '.\keystore-usi.xml',
    [string] $name = 'USIMachine',
    [string] $password = 'Password1!'
)
$ErrorActionPreference = 'Stop'
$file = Resolve-Path $file
$xmlDocument = [System.Xml.XPath.XPathDocument]::new($file)
$xPathNavigator = $xmlDocument.CreateNavigator()
$ns = [System.Xml.XmlNamespaceManager]::new($xPathNavigator.NameTable)
$ns.AddNamespace('store', 'http://auth.abr.gov.au/credential/xsd/SBRCredentialStore')
$xpath = "//store:credentialStore/store:credentials/store:credential[store:name1[text()='$name']]"
$publicCertificateString = $xPathNavigator.SelectSingleNode("$xpath/store:publicCertificate/text()", $ns).Value
if ([System.String]::IsNullOrWhiteSpace($publicCertificateString)) {
    throw [System.NotSupportedException]::new('The AUSKey file is not valid. The public certificate could not be found.')
}

$privateKeyString = $xPathNavigator.SelectSingleNode("$xpath/store:protectedPrivateKey/text()", $ns).Value
if ([System.String]::IsNullOrWhiteSpace($privateKeyString)) {
    throw [System.NotSupportedException]::new('The AUSKey file is not valid. The private key could not be found.')
}

$abn = $xPathNavigator.SelectSingleNode("$xpath/store:abn/text()", $ns).Value
if ([System.String]::IsNullOrWhiteSpace($abn)) {
    throw [System.NotSupportedException]::new('The AUSKey file is not valid. The ABN could not be found.')
}

$x509Certificate2Collection = [System.Security.Cryptography.X509Certificates.X509Certificate2Collection]::new()
$x509Certificate2Collection.Import([System.Convert]::FromBase64String($publicCertificateString))
$x509Certificate2 = [System.Linq.Enumerable]::FirstOrDefault($x509Certificate2Collection, [Func[System.Security.Cryptography.X509Certificates.X509Certificate2, bool]] { `
            param($x) $x.SubjectName.Name -ne $null -and $x.SubjectName.Name.Contains($abn) })
if ($null -eq $x509Certificate2) {
    throw [System.NotSupportedException]::new('The AUSKey file is not valid. The required certificate could not be found.')
}

$rsa = [System.Security.Cryptography.RSA]::Create()
$bytesRead = 0
$rsa.ImportEncryptedPkcs8PrivateKey($password, [System.Convert]::FromBase64String($privateKeyString), [ref] $bytesRead)
$x509Certificate2 = [System.Security.Cryptography.X509Certificates.RSACertificateExtensions]::CopyWithPrivateKey($x509Certificate2, $rsa)
$data = $x509Certificate2.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Pfx, $password)
$pfxFileName = "$pwd\$name.pfx"
[System.IO.File]::WriteAllBytes($pfxFileName, $data)

Write-Host "File $pfxFileName has been successfully generated."