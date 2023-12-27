using Core.DTOs;
using Core.Interfaces;
using Core.WorkModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLevel.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthController : Controller
{
    private IUserAuthenticationRepository AuthenticationRepository { get; }
    private IUnitOfWork WorkModel { get; }

    public AuthController(IUserAuthenticationRepository authenticationRepository, IUnitOfWork workModel)
    {
        AuthenticationRepository = authenticationRepository;
        WorkModel = workModel;
    }

    [HttpGet]
    [Authorize]
    [ActionName("verify")]
    public async Task<IActionResult> Validate()
    {
        var user = await WorkModel.Users
            .GetFirstAsync(u => u.UserName == User.Identity!.Name);

        if (user is null)
            return Unauthorized();

        return Ok(user.UserType);
    }

    [HttpPost]
    [ActionName("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
    {
        var response = await AuthenticationRepository.ValidateUserAsync(dto);

        if (!response.result || response.user == null)
            return Unauthorized("Invalid username or password");

        var token = await AuthenticationRepository.CreateTokenAsync(response.user);

        return Ok(new { token });
    }

    [HttpPost]
    [ActionName("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationDto dto)
    {
        IdentityResult result;

        switch (dto.Type)
        {
            case "contractor":
                result = await AuthenticationRepository.RegisterContractorAsync(dto);
                break;
            case "customer":
                result = await AuthenticationRepository.RegisterCustomerAsync(dto);
                break;
            default:
                return BadRequest("Invalid user type");
        }

        if (!result.Succeeded)
            return BadRequest(string.Join("\n", result.Errors.Select(e => e.Description)));

        return await Login(new UserLoginDto
        {
            Username = dto.Username,
            Password = dto.Password
        });
    }
}
