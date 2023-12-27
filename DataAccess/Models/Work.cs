using System.ComponentModel.DataAnnotations;
using DataAccess.Enums;

namespace DataAccess.Models;

public class Work
{
    public int Id { get; set; }

    [MaxLength(256)] public string Name { get; set; } = null!;
    [MaxLength(500)] public string Description { get; set; } = null!;

    public DateTime? Deadline { get; set; }
    public double? Price { get; set; }

    public State State { get; set; }

    [MaxLength(450)] public string CustomerId { get; set; } = null!;
    public Customer Customer { get; set; } = null!;

    [MaxLength(450)] public string? ContractorId { get; set; } = null!;
    public Contractor? Contractor { get; set; } = null!;

    public Chat? Chat { get; set; }
}
