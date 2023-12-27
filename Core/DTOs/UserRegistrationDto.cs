using System.ComponentModel.DataAnnotations;

namespace Core.DTOs;

public class UserRegistrationDto
{
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;

    [Required(ErrorMessage = "Username is required")]
    public string Username { get; init; } = null!;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; init; } = null!;

    public string Email { get; init; } = null!;

    public string Type { get; init; } = null!;
}
