using Core.DTOs;
using Core.WorkModel;
using DataAccess.Enums;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLevel.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
[Authorize(Roles = "Contractor")]
public class ContractorController : Controller
{
    public ContractorController(IUnitOfWork workModel)
    {
        WorkModel = workModel;
    }

    private IUnitOfWork WorkModel { get; }

    [HttpGet]
    [ActionName("home")]
    public async Task<IActionResult> Get()
    {
        var works = await WorkModel.Works
            .GetAllAsync(w => w.State == State.Active && (w.Deadline > DateTime.UtcNow || w.Deadline == null));

        return Ok(new { works });
    }

    [HttpPost]
    [ActionName("search")]
    public async Task<IActionResult> Search([FromBody] string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return Ok(await WorkModel.Works
                .GetAllAsync(w => w.State == State.Active && (w.Deadline > DateTime.UtcNow || w.Deadline == null)));
        }

        var works = await WorkModel.Works
            .GetAllAsync(w =>
                w.State == State.Active && (w.Deadline > DateTime.UtcNow || w.Deadline == null) &&
                w.Name.Contains(name));

        return Ok(works);
    }

    [HttpPost]
    [ActionName("send-request")]
    public async Task<IActionResult> SendRequest([FromBody] RequestDto dto)
    {
        var work = await WorkModel.Works
            .GetFirstAsync(w => w.Id == dto.WorkId && w.State == State.Active &&
                                (w.Deadline > DateTime.UtcNow || w.Deadline == null));

        if (work is null)
            return NotFound("Work not found");

        var contractor = await WorkModel.Contractors
            .GetFirstAsync(c => c.UserName == User.Identity!.Name);

        if (contractor is null)
            return NotFound("Contractor not found");

        var request = new Request
        {
            WorkId = work.Id,
            ContractorId = contractor.Id,
            Description = dto.Description,
            CustomerId = work.CustomerId
        };

        await WorkModel.Requests.AddAsync(request);
        await WorkModel.SaveChangesAsync();

        return Ok();
    }
}
