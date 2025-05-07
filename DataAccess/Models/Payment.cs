using System.ComponentModel.DataAnnotations;
using DataAccess.Enums;

namespace DataAccess.Models;

public class Payment
{
    public int Id { get; set; }

    public int WorkId { get; set; }
    public Work Work { get; set; } = null!;

    [MaxLength(450)]
    public string CustomerId { get; set; } = null!;
    public Customer Customer { get; set; } = null!;

    [MaxLength(450)]
    public string ContractorId { get; set; } = null!;
    public Contractor Contractor { get; set; } = null!;

    public double Amount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PaidAt { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [MaxLength(256)]
    public string? TransactionId { get; set; }

    [MaxLength(256)]
    public string? PaymentProvider { get; set; } // "PayPal", "Stripe", etc.

    // For storing any additional payment details as JSON
    public string? PaymentDetails { get; set; }
}