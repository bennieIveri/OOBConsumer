namespace OOB.Data.Entities;

public class TransactionResponseNameValue
{
    public long TRNV_Id { get; set; }

    public long TR_Id { get; set; }

    public required string TRNV_Name { get; set; }

    public required string TRNV_Value { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public TransactionResponse TransactionResponse { get; set; } = null!;
}
