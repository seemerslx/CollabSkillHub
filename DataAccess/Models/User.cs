using System.ComponentModel.DataAnnotations;
using DataAccess.Enums;
using Microsoft.AspNetCore.Identity;

namespace DataAccess.Models;

public class User : IdentityUser
{
    [MaxLength(50)] public string FirstName { get; set; } = null!;
    [MaxLength(50)] public string LastName { get; set; } = null!;

    public string FullName => $"{FirstName} {LastName}";

    public UserType UserType { get; set; }

    public ICollection<Work> Works { get; set; } = new List<Work>();
    public ICollection<Chat> Chats { get; set; } = new List<Chat>();
    public ICollection<Request> Requests { get; set; } = new List<Request>();
}
