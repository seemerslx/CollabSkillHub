using System.ComponentModel.DataAnnotations;

namespace Core.DTOs;

public class UserLoginDto
{
    [Required(ErrorMessage = "Username is required")]
    public string? Username { get; init; }

    [Required(ErrorMessage = "Password is required")]
    public string? Password { get; init; }
}
