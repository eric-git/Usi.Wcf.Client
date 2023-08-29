using System.Security.Cryptography.X509Certificates;

namespace Common;

public interface IAusKeyManager
{
    X509Certificate2 GetX509Certificate();
}