namespace DataAccess.Models;

public class Request
{
    public int Id { get; set; }

    public int WorkId { get; set; }
    public Work? Work { get; set; }

    public string CustomerId { get; set; } = null!;
    public Customer? Customer { get; set; }

    public string ContractorId { get; set; } = null!;
    public Contractor? Contractor { get; set; }

    public RequestState State { get; set; } = RequestState.Pending;

    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum RequestState
{
    Pending,
    Accepted,
    Rejected
}
