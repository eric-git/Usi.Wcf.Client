using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.XPath;
using Common.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Common.Keystore;

public class KeystoreManager(IConfiguration configuration, IMemoryCache memoryCache) : IKeystoreManager
{
    public (string, X509Certificate2) GetX509CertificateData()
    {
        var clientCredentialId = configuration[SettingsKey.KeystoreClientCredentialId] ?? throw new ApplicationException();
        var certificateData = memoryCache.GetOrCreate($"KS-{clientCredentialId}", cacheEntry =>
        {
            var fileName = configuration[SettingsKey.KeystoreFileName] ?? throw new ApplicationException();
            var password = configuration[SettingsKey.KeystorePassword] ?? throw new ApplicationException();
            XPathDocument xmlDocument = new(Path.Combine(AppContext.BaseDirectory, fileName));
            var xPathNavigator = xmlDocument.CreateNavigator();
            XmlNamespaceManager ns = new(xPathNavigator.NameTable);
            ns.AddNamespace("store", "http://auth.abr.gov.au/credential/xsd/SBRCredentialStore");
            var xpath = $"//store:credentialStore/store:credentials/store:credential[@id='{clientCredentialId}']";
            var publicCertificateString = xPathNavigator.SelectSingleNode($"{xpath}/store:publicCertificate/text()", ns)?.Value;
            if (string.IsNullOrWhiteSpace(publicCertificateString))
            {
                throw new NotSupportedException("The keystore file is not valid. The public certificate could not be found.");
            }

            var privateKeyString = xPathNavigator.SelectSingleNode($"{xpath}/store:protectedPrivateKey/text()", ns)?.Value;
            if (string.IsNullOrWhiteSpace(privateKeyString))
            {
                throw new NotSupportedException("The keystore file is not valid. The private key could not be found.");
            }

            var abn = xPathNavigator.SelectSingleNode($"{xpath}/store:abn/text()", ns)?.Value;
            if (string.IsNullOrWhiteSpace(abn))
            {
                throw new NotSupportedException("The keystore file is not valid. The ABN could not be found.");
            }

            SignedCms signedCms = new();
            signedCms.Decode(Convert.FromBase64String(publicCertificateString));
            var x509Certificate2 = signedCms.Certificates.FirstOrDefault(c => c.SubjectName.Name.Contains(abn)) ??
                                   throw new NotSupportedException("The keystore file is not valid. The required certificate could not be found.");
            using var rsa = RSA.Create();
            rsa.ImportEncryptedPkcs8PrivateKey(password, Convert.FromBase64String(privateKeyString), out _);
            x509Certificate2 = x509Certificate2.CopyWithPrivateKey(rsa);
            cacheEntry.AbsoluteExpiration = x509Certificate2.NotAfter.AddSeconds(-30);
            return (abn, x509Certificate2);
        });

        return certificateData;
    }
}
