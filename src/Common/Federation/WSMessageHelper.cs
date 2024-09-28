using System.Xml;
using Microsoft.IdentityModel.Protocols.WsFed;
using Microsoft.IdentityModel.Protocols.WsTrust;

namespace Common.Federation;

public static class WsMessageHelper
{
    private const string Wsa = "http://www.w3.org/2005/08/addressing";
    private const string Wsp = "http://schemas.xmlsoap.org/ws/2004/09/policy";
    private const string Wst = "http://docs.oasis-open.org/ws-sx/ws-trust/200512";
    private const string Wsu = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
    private const string Wst2008 = "http://docs.oasis-open.org/ws-sx/ws-trust/200802";
    private const string V13 = "http://vanguard.business.gov.au/2016/03";
    private const string PartyScheme = "uri://abr.gov.au/ABN";

    public static XmlElement GetAppliesToElement(Uri uri)
    {
        var (xmlWriter, xmlDocument) = GetXmlWriter();
        using (xmlWriter)
        {
            xmlWriter.WriteStartElement("AppliesTo", Wsp);
            xmlWriter.WriteStartElement("EndpointReference", Wsa);
            xmlWriter.WriteStartElement("Address", Wsa);
            xmlWriter.WriteValue(uri);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.Flush();
        }

        return xmlDocument.DocumentElement ?? throw new ApplicationException();
    }

    public static XmlElement GetLifeTimeElement(TimeSpan timeSpan)
    {
        var now = DateTime.UtcNow;
        var (xmlWriter, xmlDocument) = GetXmlWriter();
        using (xmlWriter)
        {
            xmlWriter.WriteStartElement("Lifetime", Wst);
            xmlWriter.WriteStartElement("Created", Wsu);
            xmlWriter.WriteValue(now);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("Expires", Wsu);
            xmlWriter.WriteValue(now.Add(timeSpan));
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.Flush();
        }

        return xmlDocument.DocumentElement ?? throw new ApplicationException();
    }

    public static Claims GetRequiredClaimTypes() => new("http://schemas.xmlsoap.org/ws/2005/05/identity",
        [
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
        ]);

    public static XmlElement GetActAsElement(string firstPartyAbn, string secondPartyAbn)
    {
        var (xmlWriter, xmlDocument) = GetXmlWriter();
        using (xmlWriter)
        {
            xmlWriter.WriteStartElement("ActAs", Wst2008);
            xmlWriter.WriteStartElement("RelationshipToken", V13);
            xmlWriter.WriteAttributeString("ID", V13, Guid.NewGuid().ToString());
            xmlWriter.WriteStartElement("Relationship", V13);
            xmlWriter.WriteAttributeString("Type", V13, "OSPfor");
            xmlWriter.WriteStartElement("Attribute", V13);
            xmlWriter.WriteAttributeString("Name", V13, "SSID");
            xmlWriter.WriteAttributeString("Value", V13, "0000123400");
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("FirstParty", V13);
            xmlWriter.WriteAttributeString("Scheme", V13, PartyScheme);
            xmlWriter.WriteAttributeString("Value", V13, firstPartyAbn);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("SecondParty", V13);
            xmlWriter.WriteAttributeString("Scheme", V13, PartyScheme);
            xmlWriter.WriteAttributeString("Value", V13, secondPartyAbn);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.Flush();
        }

        return xmlDocument.DocumentElement ?? throw new ApplicationException();
    }

    private static (XmlWriter, XmlDocument) GetXmlWriter()
    {
        XmlDocument xmlDocument = new();
        var xmlNavigator = xmlDocument.CreateNavigator() ?? throw new ApplicationException();
        return (xmlNavigator.AppendChild(), xmlDocument);
    }
}
