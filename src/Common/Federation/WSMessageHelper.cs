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
    private const string WsIdentityBaseNamespace = "http://schemas.xmlsoap.org/ws/2005/05/identity";
    private const string V13 = "http://vanguard.business.gov.au/2016/03";
    private const string VanguardIdentityBaseNamespace = "http://vanguard.ebusiness.gov.au/2008/06/identity/claims";
    private const string PartyScheme = "uri://abr.gov.au/ABN";
    private const string MachineCredentialClaim = "ABR_Device";

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

        return xmlDocument.DocumentElement ?? throw new InvalidOperationException("XML document has no root element.");
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

        return xmlDocument.DocumentElement ?? throw new InvalidOperationException("XML document has no root element.");
    }

    public static Claims GetRequiredClaimTypes(string abn)
    {
        return new Claims(WsIdentityBaseNamespace,
        [
            new ClaimType
            {
                IsOptional = false,
                Uri = $"{VanguardIdentityBaseNamespace}/abn",
                Value = abn
            },
            new ClaimType
            {
                IsOptional = false,
                Uri = $"{VanguardIdentityBaseNamespace}/credentialtype",
                Value = MachineCredentialClaim
            }
        ]);
    }

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

        return xmlDocument.DocumentElement ?? throw new InvalidOperationException("XML document has no root element.");
    }

    private static (XmlWriter, XmlDocument) GetXmlWriter()
    {
        XmlDocument xmlDocument = new();
        var xmlNavigator = xmlDocument.CreateNavigator() ?? throw new InvalidOperationException("Failed to create an XPathNavigator for the XML document.");
        return (xmlNavigator.AppendChild(), xmlDocument);
    }
}