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
    private const string WST2008 = "http://docs.oasis-open.org/ws-sx/ws-trust/200802";
    private const string V13 = "http://vanguard.business.gov.au/2016/03";
    private const string PARTY_SCHEME = "uri://abr.gov.au/ABN";

    public XmlElement GetAppliesToElement(Uri uri)
    {
        XmlDocument xmlDocument = new();
        using (var xmlWriter = (xmlDocument.CreateNavigator() ?? throw new ApplicationException()).AppendChild())
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

        return xmlDocument.DocumentElement ?? throw new ApplicationException();
    }

    public XmlElement GetLifeTimeElement(TimeSpan timeSpan)
    {
        var now = DateTime.UtcNow;
        XmlDocument xmlDocument = new();
        using (var xmlWriter = (xmlDocument.CreateNavigator() ?? throw new ApplicationException()).AppendChild())
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

        return xmlDocument.DocumentElement ?? throw new ApplicationException();
    }

    public Claims GetRequiredClaimTypes() => new("http://schemas.xmlsoap.org/ws/2005/05/identity",
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

    public XmlElement GetActAsElement(string firstPartyAbn, string secondPartyAbn)
    {
        XmlDocument xmlDocument = new();
        using (var xmlWriter = (xmlDocument.CreateNavigator() ?? throw new ApplicationException()).AppendChild())
        {
            xmlWriter.WriteStartElement("ActAs", WST2008);
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
            xmlWriter.WriteAttributeString("Scheme", V13, PARTY_SCHEME);
            xmlWriter.WriteAttributeString("Value", V13, firstPartyAbn);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("SecondParty", V13);
            xmlWriter.WriteAttributeString("Scheme", V13, PARTY_SCHEME);
            xmlWriter.WriteAttributeString("Value", V13, secondPartyAbn);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.Flush();
        }

        return xmlDocument.DocumentElement ?? throw new ApplicationException();
    }
}
