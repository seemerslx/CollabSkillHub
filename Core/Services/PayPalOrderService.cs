using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using DataAccess;
using DataAccess.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Services;

public class PayPalOrderService
{
    private readonly PayPalHttpClientService _payPalHttpClient;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private readonly ILogger<PayPalOrderService> _logger;

    public PayPalOrderService(
        PayPalHttpClientService payPalHttpClient,
        IConfiguration configuration,
        AppDbContext context,
        ILogger<PayPalOrderService> logger)
    {
        _payPalHttpClient = payPalHttpClient;
        _configuration = configuration;
        _context = context;
        _logger = logger;
    }

    public async Task<PayPalOrderResponse> CreatePayPalOrderAsync(int paymentId)
    {
        try
        {
            var payment = await _context.Payments
                .Include(p => p.Work)
                .Include(p => p.Customer)
                .Include(p => p.Contractor)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                _logger.LogError($"Payment not found: {paymentId}");
                return new PayPalOrderResponse { Success = false, ErrorMessage = "Payment not found" };
            }

            // Get contractor's PayPal email
            var contractorPaymentInfo = await _context.ContractorPaymentInfos
                .FirstOrDefaultAsync(p => p.ContractorId == payment.ContractorId);

            if (contractorPaymentInfo == null || string.IsNullOrEmpty(contractorPaymentInfo.PayPalEmail))
            {
                _logger.LogError($"Contractor {payment.ContractorId} has no PayPal email configured");
                return new PayPalOrderResponse
                {
                    Success = false,
                    ErrorMessage = "Contractor has not set up their payment information"
                };
            }

            // Get PayPal access token
            string accessToken = await _payPalHttpClient.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("Failed to obtain PayPal access token");
                return new PayPalOrderResponse { Success = false, ErrorMessage = "Failed to obtain PayPal access token" };
            }

            // Determine the appropriate PayPal API URL
            bool isTestMode = true;
            string apiUrl = isTestMode
                ? _configuration["PayPal:TestOrdersUrl"]
                : _configuration["PayPal:OrdersUrl"];

            // Prepare return and cancel URLs
            string baseUrl = _configuration["Application:BaseUrl"];
            string returnUrl = $"{baseUrl}/payment/success?paymentId={payment.Id}";
            string cancelUrl = $"{baseUrl}/payment/cancel?paymentId={payment.Id}";

            // Create the order request object with payee information
            var orderRequest = new PayPalCreateOrderRequest
            {
                Intent = "CAPTURE",
                PurchaseUnits = new List<PayPalOrderPurchaseUnit>
                    {
                        new PayPalOrderPurchaseUnit
                        {
                            ReferenceId = payment.Id.ToString(),
                            Description = $"Payment for work: {payment.Work.Name}",
                            CustomId = payment.Work.Id.ToString(),
                            Amount = new PayPalOrderAmount
                            {
                                CurrencyCode = "USD",
                                Value = payment.Amount.ToString("0.00", CultureInfo.InvariantCulture)
                            },
                            Payee = new PayPalOrderPayee
                            {
                                EmailAddress = contractorPaymentInfo.PayPalEmail
                            }
                        }
                    },
                ApplicationContext = new PayPalOrderApplicationContext
                {
                    ReturnUrl = returnUrl,
                    CancelUrl = cancelUrl,
                    UserAction = "PAY_NOW",
                    ShippingPreference = "NO_SHIPPING"
                }
            };

            string jsonBody = JsonSerializer.Serialize(orderRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            _logger.LogInformation($"Creating PayPal order for payment {payment.Id}: {jsonBody}");

            // Send the request to PayPal
            string responseBody = await _payPalHttpClient.PostJsonAsync(apiUrl, jsonBody, accessToken);

            if (string.IsNullOrEmpty(responseBody))
            {
                _logger.LogError("Empty response from PayPal order creation");
                return new PayPalOrderResponse { Success = false, ErrorMessage = "Empty response from PayPal" };
            }

            _logger.LogInformation($"PayPal order creation response: {responseBody}");

            // Deserialize the response
            var createOrderResponse = JsonSerializer.Deserialize<PayPalCreateOrderResponse>(responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (createOrderResponse == null || string.IsNullOrEmpty(createOrderResponse.Id))
            {
                _logger.LogError("Invalid response format from PayPal");
                return new PayPalOrderResponse { Success = false, ErrorMessage = "Invalid response from PayPal" };
            }

            // Save the PayPal order ID to the payment record
            payment.PaymentProvider = "PayPal";
            payment.PaymentDetails = responseBody;
            payment.Status = PaymentStatus.Processing;

            await _context.SaveChangesAsync();

            // Extract the approval URL from the response
            string approvalUrl = createOrderResponse.Links
                .FirstOrDefault(link => link.Rel.Equals("approve", StringComparison.OrdinalIgnoreCase))?.Href;

            return new PayPalOrderResponse
            {
                Success = true,
                OrderId = createOrderResponse.Id,
                ApprovalUrl = approvalUrl
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating PayPal order for payment {paymentId}");
            return new PayPalOrderResponse
            {
                Success = false,
                ErrorMessage = $"Error creating PayPal order: {ex.Message}"
            };
        }
    }

    public async Task<PayPalCaptureResponse> CapturePaymentAsync(string orderId)
    {
        try
        {
            // Find the payment record associated with this order
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentDetails.Contains(orderId));

            if (payment == null)
            {
                _logger.LogError($"Payment not found for order: {orderId}");
                return new PayPalCaptureResponse { Success = false, ErrorMessage = "Payment not found" };
            }

            // Get PayPal access token
            string accessToken = await _payPalHttpClient.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("Failed to obtain PayPal access token for capture");
                return new PayPalCaptureResponse { Success = false, ErrorMessage = "Failed to obtain PayPal access token" };
            }

            // Determine the appropriate PayPal API URL
            bool isTestMode = true;
            string captureUrlTemplate = isTestMode
                ? _configuration["PayPal:TestCaptureUrlTemplate"]
                : _configuration["PayPal:CaptureUrlTemplate"];

            string captureUrl = string.Format(captureUrlTemplate, orderId);

            _logger.LogInformation($"Capturing PayPal payment for order {orderId}");

            // Send the capture request
            string jsonBody = "{}"; // Empty JSON object for capture request
            string responseBody = await _payPalHttpClient.PostJsonAsync(captureUrl, jsonBody, accessToken);

            if (string.IsNullOrEmpty(responseBody))
            {
                _logger.LogError("Empty response from PayPal capture request");
                return new PayPalCaptureResponse { Success = false, ErrorMessage = "Empty response from PayPal" };
            }

            _logger.LogInformation($"PayPal capture response: {responseBody}");

            // Deserialize the response
            var captureResponse = JsonSerializer.Deserialize<PayPalCaptureOrderResponse>(responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (captureResponse == null)
            {
                _logger.LogError("Invalid response format from PayPal capture");
                return new PayPalCaptureResponse { Success = false, ErrorMessage = "Invalid response from PayPal" };
            }

            // Check if capture was successful
            if (captureResponse.Status?.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Update payment record with transaction details
                payment.Status = PaymentStatus.Completed;
                payment.PaidAt = DateTime.UtcNow;
                payment.TransactionId = captureResponse.Id;
                payment.PaymentDetails = responseBody;

                await _context.SaveChangesAsync();

                // Get and update the work status if needed
                var work = await _context.Works.FindAsync(payment.WorkId);
                if (work != null)
                {
                    // You might want to add a "Paid" state to your State enum
                    // work.State = State.Paid;
                    await _context.SaveChangesAsync();
                }

                return new PayPalCaptureResponse
                {
                    Success = true,
                    TransactionId = captureResponse.Id,
                    PaymentId = payment.Id
                };
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.PaymentDetails = responseBody;
                await _context.SaveChangesAsync();

                return new PayPalCaptureResponse
                {
                    Success = false,
                    ErrorMessage = $"PayPal capture failed with status: {captureResponse.Status}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error capturing PayPal payment for order {orderId}");
            return new PayPalCaptureResponse
            {
                Success = false,
                ErrorMessage = $"Error capturing PayPal payment: {ex.Message}"
            };
        }
    }

    public async Task<RefundResponse> RefundPaymentAsync(int paymentId, double? amount = null, string reason = null)
    {
        try
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                _logger.LogError($"Payment not found for refund: {paymentId}");
                return new RefundResponse { Success = false, ErrorMessage = "Payment not found" };
            }

            if (payment.Status != PaymentStatus.Completed)
            {
                _logger.LogError($"Cannot refund payment that is not completed: {paymentId}, Status: {payment.Status}");
                return new RefundResponse { Success = false, ErrorMessage = "Payment is not in completed state" };
            }

            if (string.IsNullOrEmpty(payment.TransactionId))
            {
                _logger.LogError($"Payment has no transaction ID: {paymentId}");
                return new RefundResponse { Success = false, ErrorMessage = "Payment has no transaction ID" };
            }

            // Get PayPal access token
            string accessToken = await _payPalHttpClient.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("Failed to obtain PayPal access token for refund");
                return new RefundResponse { Success = false, ErrorMessage = "Failed to obtain PayPal access token" };
            }

            // Determine the appropriate PayPal API URL
            bool isTestMode = true;
            string refundUrlTemplate = isTestMode
                ? _configuration["PayPal:TestRefundUrlTemplate"]
                : _configuration["PayPal:RefundUrlTemplate"];

            string refundUrl = string.Format(refundUrlTemplate, payment.TransactionId);

            // Prepare refund request
            var refundRequest = new PayPalRefundRequest();

            // If amount is specified, it's a partial refund
            if (amount.HasValue && amount.Value > 0 && amount.Value < payment.Amount)
            {
                refundRequest.Amount = new PayPalRefundAmount
                {
                    Value = amount.Value.ToString("0.00", CultureInfo.InvariantCulture),
                    CurrencyCode = "USD"
                };
            }

            if (!string.IsNullOrEmpty(reason))
            {
                refundRequest.NoteToPayer = reason;
            }

            string jsonBody = JsonSerializer.Serialize(refundRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            _logger.LogInformation($"Refunding PayPal payment {paymentId}, Transaction: {payment.TransactionId}: {jsonBody}");

            // Send the refund request
            string responseBody = await _payPalHttpClient.PostJsonAsync(refundUrl, jsonBody, accessToken);

            if (string.IsNullOrEmpty(responseBody))
            {
                _logger.LogError("Empty response from PayPal refund request");
                return new RefundResponse { Success = false, ErrorMessage = "Empty response from PayPal" };
            }

            _logger.LogInformation($"PayPal refund response: {responseBody}");

            // Deserialize the response
            var refundResponse = JsonSerializer.Deserialize<PayPalRefundResponse>(responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (refundResponse == null || string.IsNullOrEmpty(refundResponse.Id))
            {
                _logger.LogError("Invalid response format from PayPal refund");
                return new RefundResponse { Success = false, ErrorMessage = "Invalid response from PayPal" };
            }

            // Check if refund was successful
            if (refundResponse.Status?.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase) == true ||
                refundResponse.Status?.Equals("PENDING", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Update payment status to refunded
                payment.Status = PaymentStatus.Refunded;
                payment.PaymentDetails = JsonSerializer.Serialize(new
                {
                    OriginalPayment = payment.PaymentDetails,
                    RefundResponse = responseBody
                });

                await _context.SaveChangesAsync();

                return new RefundResponse
                {
                    Success = true,
                    RefundId = refundResponse.Id,
                    Status = refundResponse.Status
                };
            }
            else
            {
                return new RefundResponse
                {
                    Success = false,
                    ErrorMessage = $"PayPal refund failed with status: {refundResponse.Status}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error refunding PayPal payment {paymentId}");
            return new RefundResponse
            {
                Success = false,
                ErrorMessage = $"Error refunding PayPal payment: {ex.Message}"
            };
        }
    }
}

#region Request and Response Models

public class PayPalOrderResponse
{
    public bool Success { get; set; }
    public string OrderId { get; set; }
    public string ApprovalUrl { get; set; }
    public string ErrorMessage { get; set; }
}

public class PayPalCaptureResponse
{
    public bool Success { get; set; }
    public string TransactionId { get; set; }
    public int PaymentId { get; set; }
    public string ErrorMessage { get; set; }
}

public class RefundResponse
{
    public bool Success { get; set; }
    public string RefundId { get; set; }
    public string Status { get; set; }
    public string ErrorMessage { get; set; }
}

public class PayPalCreateOrderRequest
{
    [JsonPropertyName("intent")]
    public string Intent { get; set; }

    [JsonPropertyName("purchase_units")]
    public List<PayPalOrderPurchaseUnit> PurchaseUnits { get; set; }

    [JsonPropertyName("application_context")]
    public PayPalOrderApplicationContext ApplicationContext { get; set; }
}

public class PayPalOrderPurchaseUnit
{
    [JsonPropertyName("reference_id")]
    public string ReferenceId { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("custom_id")]
    public string CustomId { get; set; }

    [JsonPropertyName("amount")]
    public PayPalOrderAmount Amount { get; set; }

    [JsonPropertyName("payee")]
    public PayPalOrderPayee Payee { get; set; }
}

public class PayPalOrderPayee
{
    [JsonPropertyName("email_address")]
    public string EmailAddress { get; set; }

    [JsonPropertyName("merchant_id")]
    public string MerchantId { get; set; }
}

public class PayPalOrderAmount
{
    [JsonPropertyName("currency_code")]
    public string CurrencyCode { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}

public class PayPalOrderApplicationContext
{
    [JsonPropertyName("return_url")]
    public string ReturnUrl { get; set; }

    [JsonPropertyName("cancel_url")]
    public string CancelUrl { get; set; }

    [JsonPropertyName("user_action")]
    public string UserAction { get; set; }

    [JsonPropertyName("shipping_preference")]
    public string ShippingPreference { get; set; }
}
public class PayPalCreateOrderResponse
{
    public string Id { get; set; }
    public string Status { get; set; }
    public List<PayPalLink> Links { get; set; }
}

public class PayPalLink
{
    public string Href { get; set; }
    public string Rel { get; set; }
    public string Method { get; set; }
}

public class PayPalCaptureOrderResponse
{
    public string Id { get; set; }
    public string Status { get; set; }
    public List<PayPalLink> Links { get; set; }
}

public class PayPalRefundRequest
{
    public PayPalRefundAmount Amount { get; set; }
    public string NoteToPayer { get; set; }
}

public class PayPalRefundAmount
{
    public string Value { get; set; }
    public string CurrencyCode { get; set; }
}

public class PayPalRefundResponse
{
    public string Id { get; set; }
    public string Status { get; set; }
    public List<PayPalLink> Links { get; set; }
}

#endregion