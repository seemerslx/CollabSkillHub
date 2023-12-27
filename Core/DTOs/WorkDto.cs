namespace Core.DTOs;

public class WorkDto
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;

    public DateTime? Deadline { get; set; }
    public double? Price { get; set; }
}
