namespace OOB.Data.Entities;

public class TransactionResponse
{
    public long TR_Id { get; set; }

    public Guid TR_RequestID { get; set; }

    public int PS_Id { get; set; }

    public DateTime TR_ProcessingTime { get; set; }

    public Guid TR_ApplicationID { get; set; }

    public required string TR_Mode { get; set; }

    public Guid? TR_TransactionIndex { get; set; }

    public string? TR_Acquirer { get; set; }

    public string? TR_AcquirerCycle { get; set; }

    public ProcessingStatus ProcessingStatus { get; set; } = null!;

    public ICollection<TransactionResponseNameValue> NameValues { get; set; } = [];

    public string? TR_Command { get; set; }
    public Guid? TR_OriginalRequestID { get; set; }
}
