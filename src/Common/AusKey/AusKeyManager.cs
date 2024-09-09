using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.XPath;
using Common.Configuration;
using Microsoft.Extensions.Configuration;

namespace Common.AusKey;

public class AusKeyManager(IConfiguration configuration) : IAusKeyManager
{
    private static readonly ConcurrentDictionary<string, X509Certificate2> Certificates = new();

    public X509Certificate2 GetX509Certificate() =>
     Certificates.GetOrAdd(configuration[SettingsKey.AusKeyOrgId] ?? throw new ApplicationException(), static (x, y) =>
    {
        XPathDocument xmlDocument = new(y.FileName ?? throw new ApplicationException());
        var xPathNavigator = xmlDocument.CreateNavigator();
        XmlNamespaceManager ns = new(xPathNavigator.NameTable);
        ns.AddNamespace("store", "http://auth.abr.gov.au/credential/xsd/SBRCredentialStore");
        var xpath = $"//store:credentialStore/store:credentials/store:credential[@id='{x}']";
        var publicCertificateString = xPathNavigator.SelectSingleNode($"{xpath}/store:publicCertificate/text()", ns)?.Value;
        if (string.IsNullOrWhiteSpace(publicCertificateString))
        {
            throw new NotSupportedException("The AUSKey file is not valid. The public certificate could not be found.");
        }

        var privateKeyString = xPathNavigator.SelectSingleNode($"{xpath}/store:protectedPrivateKey/text()", ns)?.Value;
        if (string.IsNullOrWhiteSpace(privateKeyString))
        {
            throw new NotSupportedException("The AUSKey file is not valid. The private key could not be found.");
        }

        var abn = xPathNavigator.SelectSingleNode($"{xpath}/store:abn/text()", ns)?.Value;
        if (string.IsNullOrWhiteSpace(abn))
        {
            throw new NotSupportedException("The AUSKey file is not valid. The ABN could not be found.");
        }

        X509Certificate2Collection x509Certificate2Collection = new();
        x509Certificate2Collection.Import(Convert.FromBase64String(publicCertificateString));
        var x509Certificate2 = x509Certificate2Collection.FirstOrDefault(c => c.SubjectName.Name != null && c.SubjectName.Name.Contains(abn)) ??
                            throw new NotSupportedException("The AUSKey file is not valid. The required certificate could not be found.");
        using var rsa = RSA.Create();
        rsa.ImportEncryptedPkcs8PrivateKey(y.Password, Convert.FromBase64String(privateKeyString), out _);
        x509Certificate2 = x509Certificate2.CopyWithPrivateKey(rsa);

        return x509Certificate2;
    }, (FileName: configuration[SettingsKey.AusKeyFileName], Password: configuration[SettingsKey.AusKeyPassword]));
}
