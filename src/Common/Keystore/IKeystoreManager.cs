using System.Security.Cryptography.X509Certificates;

namespace Common.Keystore;

public interface IKeystoreManager
{
  (string, X509Certificate2) GetX509CertificateData();
}
