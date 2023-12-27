using DataAccess.Enums;

namespace Core.DTOs;

public class TokenResponse
{
    public string Token { get; set; } = null!;
    public UserType UserType { get; set; }
}
