using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models;

public class Chat
{
    public int Id { get; set; }

    [MaxLength(50)] public string Name { get; set; } = null!;
    public string? Description { get; set; }

    [MaxLength(450)] public string CustomerId { get; set; } = null!;
    public Customer Customer { get; set; } = null!;

    [MaxLength(450)] public string ContractorId { get; set; } = null!;
    public Contractor Contractor { get; set; } = null!;

    public required int WorkId { get; set; }
    public Work Work { get; set; } = null!;

    public bool IsArchived { get; set; }

    public ICollection<Message> Messages { get; set; } = null!;
}
