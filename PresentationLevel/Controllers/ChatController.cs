using Core.WorkModel;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLevel.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : Controller
{
    public ChatController(IUnitOfWork workModel)
    {
        WorkModel = workModel;
    }

    private IUnitOfWork WorkModel { get; }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var chats = await WorkModel.Chats
            .GetAllAsync(null);

        var show = User.IsInRole("Customer");

        return Ok(new { chats = chats.OrderBy(c => c.IsArchived), show });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get([FromRoute] int id)
    {
        var chat = await WorkModel.Chats.GetFirstAsync(c => c.Id == id, "Messages", "Work");

        // Determine the user role
        string userRole = "";
        if (User.IsInRole("Customer"))
        {
            userRole = "customer";
        }
        else if (User.IsInRole("Contractor"))
        {
            userRole = "contractor";
        }

        if (chat is null)
            return NotFound("Chat not found");

        return Ok(new ChatDTO
        {
            Chat = chat,
            UserRole = userRole 
        });
    }
}

public class ChatDTO
{
    public Chat Chat { get; set; }
    public string UserRole { get; set; }
}