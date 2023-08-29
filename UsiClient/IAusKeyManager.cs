using System.Security.Cryptography.X509Certificates;

namespace UsiClient;

public interface IAusKeyManager
{
    X509Certificate2 GetX509Certificate();
}