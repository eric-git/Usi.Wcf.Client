using System.Xml;
using Microsoft.IdentityModel.Protocols.WsTrust;

namespace UsiClient;

public interface IWSMessageHelper
{
    XmlElement GetLifeTimeElement(TimeSpan timeSpan);

    XmlElement GetAppliesToElement(Uri endpointAddress);

    Claims GetRequiredClaimTypes();
}