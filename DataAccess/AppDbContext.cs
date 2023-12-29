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

        base.OnModelCreating(builder);
    }
}
