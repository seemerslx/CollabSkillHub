using Core.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLevel.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly PaymentService _paymentService;
    private readonly PayPalOrderService _payPalOrderService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        PaymentService paymentService,
        PayPalOrderService payPalOrderService,
        UserManager<User> userManager,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _payPalOrderService = payPalOrderService;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserPayments()
    {
        var userId = _userManager.GetUserId(User);
        var user = await _userManager.FindByIdAsync(userId);

        if (await _userManager.IsInRoleAsync(user, "Customer"))
        {
            var customerPayments = await _paymentService.GetCustomerPaymentsAsync(userId);
            return Ok(customerPayments);
        }
        else if (await _userManager.IsInRoleAsync(user, "Contractor"))
        {
            var contractorPayments = await _paymentService.GetContractorPaymentsAsync(userId);
            return Ok(contractorPayments);
        }

        return Forbid();
    }

    [HttpPost("create/{workId}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreatePayment(int workId)
    {
        try
        {
            var payment = await _paymentService.CreatePaymentAsync(workId);
            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating payment for work {workId}");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("paypal/create/{paymentId}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreatePayPalOrder(int paymentId)
    {
        try
        {
            var response = await _payPalOrderService.CreatePayPalOrderAsync(paymentId);

            if (!response.Success)
            {
                return BadRequest(new { message = response.ErrorMessage });
            }

            return Ok(new
            {
                orderId = response.OrderId,
                approvalUrl = response.ApprovalUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating PayPal order for payment {paymentId}");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("paypal/capture/{orderId}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CapturePayPalPayment(string orderId)
    {
        try
        {
            var response = await _payPalOrderService.CapturePaymentAsync(orderId);

            if (!response.Success)
            {
                return BadRequest(new { message = response.ErrorMessage });
            }

            return Ok(new
            {
                transactionId = response.TransactionId,
                paymentId = response.PaymentId,
                message = "Payment successful"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error capturing PayPal payment for order {orderId}");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("paypal/refund/{paymentId}")]
    [Authorize(Roles = "Admin")]  // Typically only admins should be able to refund
    public async Task<IActionResult> RefundPayment(int paymentId, [FromBody] RefundRequest request)
    {
        try
        {
            var response = await _payPalOrderService.RefundPaymentAsync(
                paymentId,
                request?.Amount,
                request?.Reason);

            if (!response.Success)
            {
                return BadRequest(new { message = response.ErrorMessage });
            }

            return Ok(new
            {
                refundId = response.RefundId,
                status = response.Status,
                message = "Refund initiated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error refunding payment {paymentId}");
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class RefundRequest
{
    public double? Amount { get; set; }
    public string Reason { get; set; }
}