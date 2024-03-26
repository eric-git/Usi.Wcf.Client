using System.Xml;
using Microsoft.IdentityModel.Protocols.WsFed;
using Microsoft.IdentityModel.Protocols.WsTrust;

namespace Common.Federation;

public class WsMessageHelper : IWsMessageHelper
{
    private const string WSA = "http://www.w3.org/2005/08/addressing";
    private const string WSP = "http://schemas.xmlsoap.org/ws/2004/09/policy";
    private const string WST = "http://docs.oasis-open.org/ws-sx/ws-trust/200512";
    private const string WSU = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";

    public XmlElement GetAppliesToElement(Uri uri)
    {
        XmlDocument xmlDocument = new();
        using (var xmlWriter = xmlDocument.CreateNavigator().AppendChild())
        {
            xmlWriter.WriteStartElement("AppliesTo", WSP);
            xmlWriter.WriteStartElement("EndpointReference", WSA);
            xmlWriter.WriteStartElement("Address", WSA);
            xmlWriter.WriteValue(uri);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.Flush();
        }

        return xmlDocument.DocumentElement;
    }

    public XmlElement GetLifeTimeElement(TimeSpan timeSpan)
    {
        var now = DateTime.UtcNow;
        XmlDocument xmlDocument = new();
        using (var xmlWriter = xmlDocument.CreateNavigator().AppendChild())
        {
            xmlWriter.WriteStartElement("Lifetime", WST);
            xmlWriter.WriteStartElement("Created", WSU);
            xmlWriter.WriteValue(now);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("Expires", WSU);
            xmlWriter.WriteValue(now.Add(timeSpan));
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.Flush();
        }

        return xmlDocument.DocumentElement;
    }

    public Claims GetRequiredClaimTypes()
    {
        return new Claims("http://schemas.xmlsoap.org/ws/2005/05/identity", new[]
        {
            new ClaimType
            {
                IsOptional = false,
                Uri = "http://vanguard.ebusiness.gov.au/2008/06/identity/claims/abn",
                Value = "abn"
            },
            new ClaimType
            {
                IsOptional = false,
                Uri = "http://vanguard.ebusiness.gov.au/2008/06/identity/claims/credentialtype",
                Value = "D"
            }
        });
    }
}