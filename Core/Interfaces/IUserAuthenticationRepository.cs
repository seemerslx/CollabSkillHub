using Core.DTOs;
using DataAccess.Models;
using Microsoft.AspNetCore.Identity;

namespace Core.Interfaces;

public interface IUserAuthenticationRepository
{
    Task<IdentityResult> RegisterCustomerAsync(UserRegistrationDto userRegistration);
    Task<IdentityResult> RegisterContractorAsync(UserRegistrationDto userRegistration);
    Task<IdentityResult> RegisterAdminAsync(UserRegistrationDto userRegistration);
    Task<(bool result, User? user)> ValidateUserAsync(UserLoginDto loginDto);
    Task<string> CreateTokenAsync(User user);
}
