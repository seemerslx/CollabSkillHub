namespace DataAccess.Models;

public class Customer : User
{
    public ICollection<Review> Reviews { get; set; } = null!;
}
