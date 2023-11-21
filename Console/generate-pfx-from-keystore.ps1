# Generate pfx file from AUSKey file
param (
    [string] $auskeyFile = 'keystore-usi.xml',
    [string] $name = 'USIMachine',
    [string] $password = 'Password1!'
)
$ErrorActionPreference = 'Stop'
$auskeyFile = "$pwd\$auskeyFile"
$xmlDocument = [System.Xml.XPath.XPathDocument]::new($auskeyFile)
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

Write-Host "Generated $pfxFileName"