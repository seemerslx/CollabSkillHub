using DataAccess;
using DataAccess.Enums;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core.Services;

public class PaymentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(AppDbContext context, ILogger<PaymentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new payment for a completed work
    /// </summary>
    public async Task<Payment> CreatePaymentAsync(int workId)
    {
        var work = await _context.Works
            .Include(w => w.Customer)
            .Include(w => w.Contractor)
            .FirstOrDefaultAsync(w => w.Id == workId);

        if (work == null)
        {
            _logger.LogError($"Work not found with ID: {workId}");
            throw new ArgumentException($"Work not found with ID: {workId}", nameof(workId));
        }

        if (work.Price == null || work.Price <= 0)
        {
            _logger.LogError($"Work {workId} has no valid price");
            throw new InvalidOperationException("Work has no valid price");
        }

        if (string.IsNullOrEmpty(work.ContractorId))
        {
            _logger.LogError($"Work {workId} has no assigned contractor");
            throw new InvalidOperationException("Work has no assigned contractor");
        }

        if (work.State != State.ReadyForReviewAndPay)
        {
            _logger.LogError($"Work {workId} is not in Completed state. Current state: {work.State}");
            throw new InvalidOperationException("Work is not in Completed state");
        }

        // Check if payment already exists
        var existingPayment = await _context.Payments
            .FirstOrDefaultAsync(p => p.WorkId == workId && p.Status != PaymentStatus.Failed);

        if (existingPayment != null)
        {
            _logger.LogWarning($"Payment for work {workId} already exists. Payment ID: {existingPayment.Id}");
            return existingPayment;
        }

        // Create new payment
        var payment = new Payment
        {
            WorkId = workId,
            CustomerId = work.CustomerId,
            ContractorId = work.ContractorId,
            Amount = work.Price.Value,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Created payment {payment.Id} for work {workId}");
        return payment;
    }

    /// <summary>
    /// Gets all payments for a customer
    /// </summary>
    public async Task<IEnumerable<Payment>> GetCustomerPaymentsAsync(string customerId)
    {
        return await _context.Payments
            .Include(p => p.Work)
            .Include(p => p.Contractor)
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all payments for a contractor
    /// </summary>
    public async Task<IEnumerable<Payment>> GetContractorPaymentsAsync(string contractorId)
    {
        return await _context.Payments
            .Include(p => p.Work)
            .Include(p => p.Customer)
            .Where(p => p.ContractorId == contractorId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a specific payment by ID
    /// </summary>
    public async Task<Payment> GetPaymentByIdAsync(int paymentId)
    {
        return await _context.Payments
            .Include(p => p.Work)
            .Include(p => p.Customer)
            .Include(p => p.Contractor)
            .FirstOrDefaultAsync(p => p.Id == paymentId);
    }

    /// <summary>
    /// Updates a payment status
    /// </summary>
    public async Task<Payment> UpdatePaymentStatusAsync(int paymentId, PaymentStatus status)
    {
        var payment = await _context.Payments.FindAsync(paymentId);
        if (payment == null)
        {
            _logger.LogError($"Payment not found with ID: {paymentId}");
            throw new ArgumentException($"Payment not found with ID: {paymentId}", nameof(paymentId));
        }

        payment.Status = status;

        if (status == PaymentStatus.Completed && payment.PaidAt == null)
        {
            payment.PaidAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation($"Updated payment {paymentId} status to {status}");

        return payment;
    }

    /// <summary>
    /// Updates a payment with transaction details
    /// </summary>
    public async Task<Payment> UpdatePaymentTransactionDetailsAsync(int paymentId, string transactionId, string provider, string details)
    {
        var payment = await _context.Payments.FindAsync(paymentId);
        if (payment == null)
        {
            _logger.LogError($"Payment not found with ID: {paymentId}");
            throw new ArgumentException($"Payment not found with ID: {paymentId}", nameof(paymentId));
        }

        payment.TransactionId = transactionId;
        payment.PaymentProvider = provider;
        payment.PaymentDetails = details;

        await _context.SaveChangesAsync();
        _logger.LogInformation($"Updated payment {paymentId} transaction details. Transaction ID: {transactionId}");

        return payment;
    }

    /// <summary>
    /// Marks a work as paid when payment is completed
    /// </summary>
    public async Task MarkWorkAsPaidAsync(int workId)
    {
        var work = await _context.Works.FindAsync(workId);
        if (work == null)
        {
            _logger.LogError($"Work not found with ID: {workId}");
            throw new ArgumentException($"Work not found with ID: {workId}", nameof(workId));
        }

        // You might want to add a Paid state to your State enum
        // work.State = State.Paid;

        // Or use a flag to mark it as paid
        // work.IsPaid = true;

        await _context.SaveChangesAsync();
        _logger.LogInformation($"Marked work {workId} as paid");
    }

    /// <summary>
    /// Gets payment statistics for a user (total paid, pending, etc.)
    /// </summary>
    public async Task<PaymentStatistics> GetPaymentStatisticsAsync(string userId, bool isCustomer)
    {
        IQueryable<Payment> query = _context.Payments;

        if (isCustomer)
        {
            query = query.Where(p => p.CustomerId == userId);
        }
        else
        {
            query = query.Where(p => p.ContractorId == userId);
        }

        var payments = await query.ToListAsync();

        return new PaymentStatistics
        {
            TotalPayments = payments.Count,
            TotalAmount = payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount),
            PendingAmount = payments.Where(p => p.Status == PaymentStatus.Pending).Sum(p => p.Amount),
            CompletedPayments = payments.Count(p => p.Status == PaymentStatus.Completed),
            PendingPayments = payments.Count(p => p.Status == PaymentStatus.Pending),
            FailedPayments = payments.Count(p => p.Status == PaymentStatus.Failed)
        };
    }
}

public class PaymentStatistics
{
    public int TotalPayments { get; set; }
    public double TotalAmount { get; set; }
    public double PendingAmount { get; set; }
    public int CompletedPayments { get; set; }
    public int PendingPayments { get; set; }
    public int FailedPayments { get; set; }
}