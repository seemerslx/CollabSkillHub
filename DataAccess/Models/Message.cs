using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models;

public class Message
{
    public long Id { get; set; }

    public string Text { get; set; } = null!;
    public string Sender { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public int ChatId { get; set; }
    public Chat Chat { get; set; } = null!;
}
