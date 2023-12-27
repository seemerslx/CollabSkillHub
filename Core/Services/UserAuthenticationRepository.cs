using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.DTOs;
using Core.Interfaces;
using DataAccess.Enums;
using DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Core.Services;

public class UserAuthenticationRepository : IUserAuthenticationRepository
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserAuthenticationRepository(UserManager<User> userManager, IConfiguration configuration,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _configuration = configuration;
        _roleManager = roleManager;
    }

    public async Task<IdentityResult> RegisterCustomerAsync(UserRegistrationDto userRegistration)
    {
        var user = new Customer
        {
            UserName = userRegistration.Username,
            Email = userRegistration.Email,
            FirstName = userRegistration.FirstName,
            LastName = userRegistration.LastName,
            UserType = UserType.Customer
        };

        var existsAsync = await _roleManager.RoleExistsAsync("Customer");
        if (!existsAsync)
            await _roleManager.CreateAsync(new IdentityRole("Customer"));

        var result = await _userManager.CreateAsync(user, userRegistration.Password);
        await _userManager.AddToRoleAsync(user, "Customer");
        return result;
    }

    public async Task<IdentityResult> RegisterContractorAsync(UserRegistrationDto userRegistration)
    {
        var user = new Contractor
        {
            UserName = userRegistration.Username,
            Email = userRegistration.Email,
            FirstName = userRegistration.FirstName,
            LastName = userRegistration.LastName,
            UserType = UserType.Contractor
        };

        var existsAsync = await _roleManager.RoleExistsAsync("Contractor");
        if (!existsAsync)
            await _roleManager.CreateAsync(new IdentityRole("Contractor"));

        var result = await _userManager.CreateAsync(user, userRegistration.Password);
        await _userManager.AddToRoleAsync(user, "Contractor");
        return result;
    }

    public async Task<IdentityResult> RegisterAdminAsync(UserRegistrationDto userRegistration)
    {
        var user = new User
        {
            UserName = userRegistration.Username,
            Email = userRegistration.Email,
            FirstName = userRegistration.FirstName,
            LastName = userRegistration.LastName,
            UserType = UserType.Admin
        };

        var existsAsync = await _roleManager.RoleExistsAsync("Admin");
        if (!existsAsync)
            await _roleManager.CreateAsync(new IdentityRole("Admin"));

        var result = await _userManager.CreateAsync(user, userRegistration.Password);
        await _userManager.AddToRoleAsync(user, "Admin");
        return result;
    }

    public async Task<(bool result, User? user)> ValidateUserAsync(UserLoginDto loginDto)
    {
        var user = _userManager.FindByNameAsync(loginDto.Username!).Result;

        if (user is null)
        {
            return (false, null);
        }

        var result = await _userManager.CheckPasswordAsync(user, loginDto.Password!);

        return (result, user);
    }

    public async Task<string> CreateTokenAsync(User user)
    {
        var signingCredentials = GetSigningCredentials();
        var claims = await GetClaims(user);
        var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
        return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
    }

    private SigningCredentials GetSigningCredentials()
    {
        var jwtConfig = _configuration.GetSection("jwtConfig");
        var key = Encoding.UTF8.GetBytes(jwtConfig["Secret"]!);
        var secret = new SymmetricSecurityKey(key);
        return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
    }

    private async Task<List<Claim>> GetClaims(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName!)
        };

        var roles = await _userManager.GetRolesAsync(user);

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return claims;
    }

    private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
    {
        var jwtSettings = _configuration.GetSection("JwtConfig");
        var tokenOptions = new JwtSecurityToken
        (
            issuer: jwtSettings["validIssuer"],
            audience: jwtSettings["validAudience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["expiresIn"])),
            signingCredentials: signingCredentials
        );
        return tokenOptions;
    }
}
