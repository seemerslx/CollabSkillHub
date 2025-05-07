using DataAccess.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class AppDbContext : IdentityDbContext<User>
{
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Contractor> Contractors { get; set; } = null!;
    public DbSet<Work> Works { get; set; } = null!;
    public DbSet<Request> Requests { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<ContractorPaymentInfo> ContractorPaymentInfos { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Work>()
            .HasOne(w => w.Customer)
            .WithMany(c => c.Works)
            .HasForeignKey(w => w.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Work>()
            .HasOne(w => w.Contractor)
            .WithMany(c => c.Works)
            .HasForeignKey(w => w.ContractorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Chat>()
            .HasOne(w => w.Customer)
            .WithMany(c => c.Chats)
            .HasForeignKey(w => w.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Chat>()
            .HasOne(w => w.Contractor)
            .WithMany(c => c.Chats)
            .HasForeignKey(w => w.ContractorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Request>()
            .HasOne(w => w.Customer)
            .WithMany(c => c.Requests)
            .HasForeignKey(w => w.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Request>()
            .HasOne(w => w.Contractor)
            .WithMany(c => c.Requests)
            .HasForeignKey(w => w.ContractorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Review>()
            .HasOne(w => w.Contractor)
            .WithMany(c => c.Reviews)
            .HasForeignKey(w => w.ContractorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Payment>()
            .HasOne(p => p.Work)
            .WithMany()
            .HasForeignKey(p => p.WorkId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Payment>()
            .HasOne(p => p.Customer)
            .WithMany()
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Payment>()
            .HasOne(p => p.Contractor)
            .WithMany()
            .HasForeignKey(p => p.ContractorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ContractorPaymentInfo>()
            .HasOne(p => p.Contractor)
            .WithOne()
            .HasForeignKey<ContractorPaymentInfo>(p => p.ContractorId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(builder);
    }
}
