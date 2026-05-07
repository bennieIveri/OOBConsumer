namespace OOB.Models;

public class VJsonRequest
{
    public string Version { get; set; } = string.Empty;

    public string Direction { get; set; } = string.Empty;

    public JsonTransactionElement Transaction { get; set; } = new();
}

public class JsonTransactionElement
{
    public string ApplicationID { get; set; } = string.Empty;

    public string Command { get; set; } = string.Empty;

    public string Mode { get; set; } = string.Empty;

    public string RequestID { get; set; } = string.Empty;

    public JsonResultElement Result { get; set; } = new();

    public string? DeviceSerialNumber { get; set; }

    public string? DeviceMake { get; set; }

    public string? MerchantTrace { get; set; }

    public string? Amount { get; set; }

    public string? AuthorisationCode { get; set; }

    public string? Currency { get; set; }

    public string? ExpiryDate { get; set; }

    public string? MerchantReference { get; set; }

    public string? Terminal { get; set; }

    public string? TransactionIndex { get; set; }

    public string? EMV_ResponseCode { get; set; }

    public string? EMV_IssuerAuthenticationData { get; set; }

    public string? MerchantName { get; set; }

    public string? MerchantUSN { get; set; }

    public string? Acquirer { get; set; }

    public string? AcquirerReference { get; set; }

    public string? AcquirerDate { get; set; }

    public string? AcquirerTime { get; set; }

    public string? DisplayAmount { get; set; }

    public string? BIN { get; set; }

    public string? Association { get; set; }

    public string? CardType { get; set; }

    public string? Issuer { get; set; }

    public string? Jurisdiction { get; set; }

    public string? PAN { get; set; }

    public string? PANMode { get; set; }

    public string? ReconReference { get; set; }

    public string? CardHolderPresence { get; set; }

    public string? MerchantAddress { get; set; }

    public string? MerchantCity { get; set; }

    public string? MerchantCountryCode { get; set; }

    public string? MerchantCountry { get; set; }

    public string? DistributorName { get; set; }

    public string? AcquirerTrace { get; set; }
}

public class JsonResultElement
{
    public int Status { get; set; }

    public int Code { get; set; }

    public string? Description { get; set; }

    public string? AppServer { get; set; }

    public string? DBServer { get; set; }

    public string? Gateway { get; set; }

    public string? AcquirerCode { get; set; }

    public string? AcquirerDescription { get; set; }
}
