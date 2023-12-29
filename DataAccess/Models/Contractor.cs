namespace DataAccess.Models;

public class Contractor : User
{
    public string Description { get; set; } = null!;

    public ICollection<Review> Reviews { get; set; } = null!;
}
