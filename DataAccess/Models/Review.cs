namespace DataAccess.Models;

public class Review
{
    public int Id { get; set; }

    public int Stars { get; set; }
    public string Comment { get; set; } = null!;
    public DateTime Date { get; set; }

    public string ContractorId { get; set; } = null!;
    public Contractor Contractor { get; set; } = null!;

    public string CustomerId { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
}
