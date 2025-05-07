using Core.DTOs;
using Core.WorkModel;
using DataAccess.Enums;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLevel.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
[Authorize(Roles = "Contractor")]
public class ContractorController : Controller
{
    private readonly IUnitOfWork _workModel;
    private readonly ILogger<ContractorController> _logger;

    public ContractorController(
        IUnitOfWork workModel,
        ILogger<ContractorController> logger)
    {
        _workModel = workModel;
        _logger = logger;
    }

    [HttpGet]
    [ActionName("home")]
    public async Task<IActionResult> Get()
    {
        var works = await _workModel.Works
            .GetAllAsync(w => w.State == State.Active && (w.Deadline > DateTime.UtcNow || w.Deadline == null));

        return Ok(new { works });
    }

    [HttpGet]
    [ActionName("me")]
    public async Task<IActionResult> GetMe()
    {
        var contractor = await _workModel.Contractors
            .GetFirstAsync(c => c.UserName == User.Identity!.Name, "Reviews.Customer");

        if (contractor is null)
            return NotFound("Contractor not found");

        return Ok(contractor);
    }

    [HttpPost]
    [ActionName("update-description")]
    public async Task<IActionResult> UpdateDescription([FromBody] string description)
    {
        var contractor = await _workModel.Contractors
            .GetFirstAsync(c => c.UserName == User.Identity!.Name);

        if (contractor is null)
            return NotFound("Contractor not found");

        contractor.Description = description;

        await _workModel.SaveChangesAsync();

        return Ok("Description updated");
    }

    [HttpPost]
    [ActionName("search")]
    public async Task<IActionResult> Search([FromBody] string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return Ok(await _workModel.Works
                .GetAllAsync(w => w.State == State.Active && (w.Deadline > DateTime.UtcNow || w.Deadline == null)));
        }

        var works = await _workModel.Works
            .GetAllAsync(w =>
                w.State == State.Active && (w.Deadline > DateTime.UtcNow || w.Deadline == null) &&
                w.Name.Contains(name));

        return Ok(works);
    }

    [HttpPost]
    [ActionName("send-request")]
    public async Task<IActionResult> SendRequest([FromBody] RequestDto dto)
    {
        var work = await _workModel.Works
            .GetFirstAsync(w => w.Id == dto.WorkId && w.State == State.Active &&
                                (w.Deadline > DateTime.UtcNow || w.Deadline == null));

        if (work is null)
            return NotFound("Work not found");

        var contractor = await _workModel.Contractors
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

        await _workModel.Requests.AddAsync(request);
        await _workModel.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("{id:int}")]
    [ActionName("mark-as-ready")]
    public async Task<IActionResult> MarkAsReady([FromRoute] int id)
    {
        var contractor = await _workModel.Contractors
            .GetFirstAsync(c => c.UserName == User.Identity!.Name);

        if (contractor is null)
            return NotFound("Customer not found");

        var chat = await _workModel.Chats
            .GetFirstAsync(c => c.Id == id && c.ContractorId == contractor.Id, "Work");

        if (chat is null)
            return NotFound("Chat not found");

        chat.Work.State = State.ReadyForReviewAndPay;

        await _workModel.SaveChangesAsync();

        return Ok("Chat closed");
    }

    [HttpGet]
    [ActionName("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var contractor = await _workModel.Contractors
            .GetFirstAsync(c => c.UserName == User.Identity!.Name);

        if (contractor == null)
        {
            return NotFound(new { message = "Contractor profile not found" });
        }

        return Ok(contractor);
    }

    [HttpGet]
    [ActionName("payment-info")]
    public async Task<IActionResult> GetPaymentInfo()
    {
        var contractor = await _workModel.Contractors
            .GetFirstAsync(c => c.UserName == User.Identity!.Name);

        if (contractor == null)
        {
            return NotFound(new { message = "Contractor profile not found" });
        }

        var paymentInfo = await _workModel.ContractorPaymentInfos
            .GetFirstAsync(p => p.ContractorId == contractor.Id);

        if (paymentInfo == null)
        {
            return Ok(null); // Return null if no payment info exists yet
        }

        return Ok(new
        {
            payPalEmail = paymentInfo.PayPalEmail,
            defaultPaymentMethod = paymentInfo.DefaultPaymentMethod,
            isPaymentInfoComplete = paymentInfo.IsPaymentInfoComplete
        });
    }

    [HttpPost]
    [ActionName("payment-info")]
    public async Task<IActionResult> SavePaymentInfo([FromBody] PaymentInfoDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var contractor = await _workModel.Contractors
            .GetFirstAsync(c => c.UserName == User.Identity!.Name);

        if (contractor == null)
        {
            return NotFound(new { message = "Contractor profile not found" });
        }

        var paymentInfo = await _workModel.ContractorPaymentInfos
            .GetFirstAsync(p => p.ContractorId == contractor.Id);

        if (paymentInfo == null)
        {
            // Create new payment info
            paymentInfo = new ContractorPaymentInfo
            {
                ContractorId = contractor.Id,
                PayPalEmail = model.PayPalEmail,
                DefaultPaymentMethod = model.DefaultPaymentMethod,
                CreatedAt = DateTime.UtcNow
            };

            await _workModel.ContractorPaymentInfos.AddAsync(paymentInfo);
        }
        else
        {
            // Update existing payment info
            paymentInfo.PayPalEmail = model.PayPalEmail;
            paymentInfo.DefaultPaymentMethod = model.DefaultPaymentMethod;
            paymentInfo.UpdatedAt = DateTime.UtcNow;

            await _workModel.ContractorPaymentInfos.UpdateAsync(paymentInfo);
        }

        await _workModel.SaveChangesAsync();
        _logger.LogInformation($"Payment info updated for contractor {contractor.Id}");

        return Ok(new { message = "Payment information saved successfully" });
    }

    [HttpGet]
    [ActionName("works")]
    public async Task<IActionResult> GetWorks()
    {
        var contractor = await _workModel.Contractors
            .GetFirstAsync(c => c.UserName == User.Identity!.Name);

        if (contractor == null)
        {
            return NotFound(new { message = "Contractor profile not found" });
        }

        var works = await _workModel.Works
            .GetAllAsync(
                w => w.ContractorId == contractor.Id,
                "Customer"
            );

        return Ok(works);
    }

    [HttpGet("{id}")]
    [ActionName("work")]
    public async Task<IActionResult> GetWork(int id)
    {
        var contractor = await _workModel.Contractors
            .GetFirstAsync(c => c.UserName == User.Identity!.Name);

        if (contractor == null)
        {
            return NotFound(new { message = "Contractor profile not found" });
        }

        var work = await _workModel.Works
            .GetFirstAsync(
                w => w.Id == id && w.ContractorId == contractor.Id,
                 "Customer"
            );

        if (work == null)
        {
            return NotFound(new { message = "Work not found" });
        }

        return Ok(work);
    }

    [HttpGet]
    [ActionName("payments")]
    public async Task<IActionResult> GetPayments()
    {
        var contractor = await _workModel.Contractors
            .GetFirstAsync(c => c.UserName == User.Identity!.Name);

        if (contractor == null)
        {
            return NotFound(new { message = "Contractor profile not found" });
        }

        var payments = await _workModel.Payments
            .GetAllAsync(
                p => p.ContractorId == contractor.Id,
                "Work", "Customer"
            );

        return Ok(payments);
    }
}

public class PaymentInfoDto
{
    public string PayPalEmail { get; set; }
    public string DefaultPaymentMethod { get; set; } = "PayPal";
}