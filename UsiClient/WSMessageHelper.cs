using System.ServiceModel;
using System.Xml;
using Microsoft.IdentityModel.Protocols.WsTrust;

namespace UsiClient;

public class WSMessageHelper : IWSMessageHelper
{
    private const string WSA = "http://www.w3.org/2005/08/addressing";
    private const string WSP = "http://schemas.xmlsoap.org/ws/2004/09/policy";
    private const string WST = "http://docs.oasis-open.org/ws-sx/ws-trust/200512";
    private const string WSU = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";

    public XmlElement GetAppliesToElement(EndpointAddress endpointAddress)
    {
        XmlDocument xmlDocument = new();
        using (var xmlWriter = xmlDocument.CreateNavigator().AppendChild())
        {
            xmlWriter.WriteStartElement("AppliesTo", WSP);
            xmlWriter.WriteStartElement("EndpointReference", WSA);
            xmlWriter.WriteElementString("Address", WSA, endpointAddress.Uri.ToString());
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.Flush();
        }

        return xmlDocument.DocumentElement;
    }

    public XmlElement GetLifeTimeElement(Lifetime lifetime)
    {
        XmlDocument xmlDocument = new();
        using (var xmlWriter = xmlDocument.CreateNavigator().AppendChild())
        {
            xmlWriter.WriteStartElement(nameof(Lifetime), WST);
            xmlWriter.WriteStartElement(nameof(Lifetime.Created), WSU);
            xmlWriter.WriteValue(lifetime.Created);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement(nameof(Lifetime.Expires), WSU);
            xmlWriter.WriteValue(lifetime.Expires);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.Flush();
        }

        return xmlDocument.DocumentElement;
    }
}