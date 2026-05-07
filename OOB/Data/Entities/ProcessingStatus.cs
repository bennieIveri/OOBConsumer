namespace OOB.Data.Entities;

public class ProcessingStatus
{
    public int PS_Id { get; set; }

    public required string PS_Description { get; set; }

    public ICollection<TransactionResponse> TransactionResponses { get; set; } = [];
}
