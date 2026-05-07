using System.Xml.Serialization;

namespace OOB.Models;

[XmlRoot("V_XML")]
public class VXmlRequest
{
    [XmlAttribute("Version")]
    public string Version { get; set; } = string.Empty;

    [XmlAttribute("Direction")]
    public string Direction { get; set; } = string.Empty;

    [XmlElement("Transaction")]
    public TransactionElement Transaction { get; set; } = new();
}

public class TransactionElement
{
    [XmlAttribute("ApplicationID")]
    public string ApplicationID { get; set; } = string.Empty;

    [XmlAttribute("Command")]
    public string Command { get; set; } = string.Empty;

    [XmlAttribute("Mode")]
    public string Mode { get; set; } = string.Empty;

    [XmlAttribute("RequestID")]
    public string RequestID { get; set; } = string.Empty;

    [XmlElement("Result")]
    public ResultElement Result { get; set; } = new();

    [XmlElement("DeviceSerialNumber")]
    public string? DeviceSerialNumber { get; set; }

    [XmlElement("DeviceMake")]
    public string? DeviceMake { get; set; }

    [XmlElement("MerchantTrace")]
    public string? MerchantTrace { get; set; }

    [XmlElement("Amount")]
    public string? Amount { get; set; }

    [XmlElement("AuthorisationCode")]
    public string? AuthorisationCode { get; set; }

    [XmlElement("Currency")]
    public string? Currency { get; set; }

    [XmlElement("ExpiryDate")]
    public string? ExpiryDate { get; set; }

    [XmlElement("MerchantReference")]
    public string? MerchantReference { get; set; }

    [XmlElement("Terminal")]
    public string? Terminal { get; set; }

    [XmlElement("TransactionIndex")]
    public string? TransactionIndex { get; set; }

    [XmlElement("EMV_ResponseCode")]
    public string? EMV_ResponseCode { get; set; }

    [XmlElement("EMV_IssuerAuthenticationData")]
    public string? EMV_IssuerAuthenticationData { get; set; }

    [XmlElement("MerchantName")]
    public string? MerchantName { get; set; }

    [XmlElement("MerchantUSN")]
    public string? MerchantUSN { get; set; }

    [XmlElement("Acquirer")]
    public string? Acquirer { get; set; }

    [XmlElement("AcquirerReference")]
    public string? AcquirerReference { get; set; }

    [XmlElement("AcquirerDate")]
    public string? AcquirerDate { get; set; }

    [XmlElement("AcquirerTime")]
    public string? AcquirerTime { get; set; }

    [XmlElement("DisplayAmount")]
    public string? DisplayAmount { get; set; }

    [XmlElement("BIN")]
    public string? BIN { get; set; }

    [XmlElement("Association")]
    public string? Association { get; set; }

    [XmlElement("CardType")]
    public string? CardType { get; set; }

    [XmlElement("Issuer")]
    public string? Issuer { get; set; }

    [XmlElement("Jurisdiction")]
    public string? Jurisdiction { get; set; }

    [XmlElement("PAN")]
    public string? PAN { get; set; }

    [XmlElement("PANMode")]
    public string? PANMode { get; set; }

    [XmlElement("ReconReference")]
    public string? ReconReference { get; set; }

    [XmlElement("CardHolderPresence")]
    public string? CardHolderPresence { get; set; }

    [XmlElement("MerchantAddress")]
    public string? MerchantAddress { get; set; }

    [XmlElement("MerchantCity")]
    public string? MerchantCity { get; set; }

    [XmlElement("MerchantCountryCode")]
    public string? MerchantCountryCode { get; set; }

    [XmlElement("MerchantCountry")]
    public string? MerchantCountry { get; set; }

    [XmlElement("DistributorName")]
    public string? DistributorName { get; set; }

    [XmlElement("AcquirerTrace")]
    public string? AcquirerTrace { get; set; }
}

public class ResultElement
{
    [XmlAttribute("Status")]
    public int Status { get; set; }

    [XmlAttribute("Code")]
    public int Code { get; set; }

    [XmlAttribute("Description")]
    public string? Description { get; set; }

    [XmlAttribute("AppServer")]
    public string? AppServer { get; set; }

    [XmlAttribute("DBServer")]
    public string? DBServer { get; set; }

    [XmlAttribute("Gateway")]
    public string? Gateway { get; set; }

    [XmlAttribute("AcquirerCode")]
    public string? AcquirerCode { get; set; }

    [XmlAttribute("AcquirerDescription")]
    public string? AcquirerDescription { get; set; }
}
