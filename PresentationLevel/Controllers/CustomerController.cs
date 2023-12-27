using Core.DTOs;
using Core.Interfaces;
using Core.WorkModel;
using DataAccess.Enums;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLevel.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin, Customer")]
public class CustomerController : Controller
{
    public CustomerController(IUnitOfWork workModel, IChatService chatService)
    {
        WorkModel = workModel;
        ChatService = chatService;
    }

    private IUnitOfWork WorkModel { get; }
    private IChatService ChatService { get; }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var customer = await WorkModel.Customers
            .GetFirstAsync(c => c.UserName == User.Identity!.Name, "Works");

        if (customer is null)
            return NotFound("Customer not found");

        var groupWorks = customer.Works
            .GroupBy(w => w.State)
            .Select(g => new
            {
                State = g.Key,
                Works = g.Select(w => new
                {
                    w.Id,
                    w.Name,
                    w.Description,
                    w.Deadline,
                    w.Price,
                    w.State
                })
            });

        customer.Works = new List<Work>();

        return Ok(new { customer, groupWorks });
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] WorkDto workDto)
    {
        var customer = await WorkModel.Customers
            .GetFirstAsync(c => c.UserName == User.Identity!.Name);

        if (customer is null)
            return NotFound("Customer not found");

        if (workDto.Name.Length > 256)
            return BadRequest("Name is too long (max 256 characters)");

        if (workDto.Description.Length > 500)
            return BadRequest("Description is too long (max 500 characters)");

        if (workDto.Deadline is not null && workDto.Deadline < DateTime.Now)
            return BadRequest("Deadline is in the past");

        var work = new Work
        {
            Name = workDto.Name,
            Description = workDto.Description,
            CustomerId = customer.Id,
            Deadline = workDto.Deadline,
            State = State.Active,
            Price = workDto.Price
        };

        await WorkModel.Works.AddAsync(work);
        await WorkModel.SaveChangesAsync();

        return Ok(work);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] WorkDto workDto)
    {
        var customer = await WorkModel.Customers
            .GetFirstAsync(c => c.UserName == User.Identity!.Name);

        if (customer is null)
            return NotFound("Customer not found");

        var work = await WorkModel.Works
            .GetFirstAsync(w => w.Id == id && w.CustomerId == customer.Id);

        if (work is null)
            return NotFound("Work not found");

        if (workDto.Name.Length > 256)
            return BadRequest("Name is too long (max 256 characters)");

        if (workDto.Description.Length > 500)
            return BadRequest("Description is too long (max 500 characters)");

        if (workDto.Deadline is not null && workDto.Deadline < DateTime.Now)
            return BadRequest("Deadline is in the past");

        work.Name = workDto.Name;
        work.Description = workDto.Description;
        work.Deadline = workDto.Deadline;
        work.Price = workDto.Price;

        await WorkModel.Works.UpdateAsync(work);
        await WorkModel.SaveChangesAsync();

        return Ok(work);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var customer = await WorkModel.Customers
            .GetFirstAsync(c => c.UserName == User.Identity!.Name);

        if (customer is null)
            return NotFound("Customer not found");

        var work = await WorkModel.Works
            .GetFirstAsync(w => w.Id == id && w.CustomerId == customer.Id);

        if (work is null)
            return NotFound("Work not found");

        if (work.State != State.Active)
            return BadRequest("Work is not in active state");

        await WorkModel.Works.DeleteAsync(work);
        await WorkModel.SaveChangesAsync();

        return Ok(work);
    }

    [HttpGet("work/{id:int}")]
    public async Task<IActionResult> GetWork([FromRoute] int id)
    {
        var customer = await WorkModel.Customers
            .GetFirstAsync(c => c.UserName == User.Identity!.Name);

        if (customer is null)
            return NotFound("Customer not found");

        var work = await WorkModel.Works
            .GetFirstAsync(w => w.Id == id && w.CustomerId == customer.Id);

        if (work is null)
            return NotFound("Work not found");

        return Ok(work);
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetRequests()
    {
        var customer = await WorkModel.Customers
            .GetFirstAsync(c => c.UserName == User.Identity!.Name);

        if (customer is null)
            return NotFound("Customer not found");

        var requests = await WorkModel.Requests
            .GetAllAsync(r => r.Work!.CustomerId == customer.Id, "Work", "Contractor");

        return Ok(requests);
    }

    [HttpPost("requests/{id:int}/{accept:bool}")]
    public async Task<IActionResult> PostRequest([FromRoute] int id, [FromRoute] bool accept)
    {
        var customer = await WorkModel.Customers
            .GetFirstAsync(c => c.UserName == User.Identity!.Name);

        if (customer is null)
            return NotFound("Customer not found");

        var request = await WorkModel.Requests
            .GetFirstAsync(r => r.Id == id && r.Work!.CustomerId == customer.Id, "Work");

        if (request is null)
            return NotFound("Request not found");

        if (request.Work!.State != State.Active && accept)
            return BadRequest("Work is not in active state");

        if (request.State != RequestState.Pending && accept)
            return BadRequest("Request is not in pending state");

        if (accept)
        {
            request.State = RequestState.Accepted;
            request.Work.State = State.Inprogress;

            await ChatService.CreateChatAsync(request.Work.Name, request.Work.CustomerId,
                request.ContractorId, request.WorkId);
        }
        else
        {
            request.State = RequestState.Rejected;
        }

        await WorkModel.Requests.UpdateAsync(request);
        await WorkModel.SaveChangesAsync();

        var requests = await WorkModel.Requests
            .GetAllAsync(r => r.Work!.CustomerId == customer.Id, "Work", "Contractor");

        return Ok(requests);
    }

    [HttpPost]
    [Route("close-chat/{id:int}")]
    public async Task<IActionResult> CloseChat([FromRoute] int id)
    {
        var customer = await WorkModel.Customers
            .GetFirstAsync(c => c.UserName == User.Identity!.Name);

        if (customer is null)
            return NotFound("Customer not found");

        var chat = await WorkModel.Chats
            .GetFirstAsync(c => c.Id == id && c.CustomerId == customer.Id, "Work");

        if (chat is null)
            return NotFound("Chat not found");

        chat.Work.State = State.Completed;
        chat.IsArchived = true;

        await WorkModel.SaveChangesAsync();

        return Ok("Chat closed");
    }
}
