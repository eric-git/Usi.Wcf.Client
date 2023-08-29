using System.ServiceModel;
using System.Xml;
using Microsoft.IdentityModel.Protocols.WsTrust;

namespace UsiClient;

public interface IWSMessageHelper
{
    XmlElement GetLifeTimeElement(Lifetime lifetime);

    XmlElement GetAppliesToElement(EndpointAddress endpointAddress);
}