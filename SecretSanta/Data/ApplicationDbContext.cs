using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SecretSanta.Models;
using SQLitePCL;

namespace SecretSanta.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<Participant> Participants { get; set; } = null!;

    public DbSet<Assignment> Assignments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Participant>()
        .HasOne(p => p.Partner)
        .WithMany()
        .HasForeignKey(p => p.PartnerId)
        .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Participant>()
        .HasIndex(p => new { p.EventId, p.Email })
        .IsUnique();

        builder.Entity<Assignment>()
        .HasOne(a => a.Giver)
        .WithMany()
        .HasForeignKey(a => a.GiverId)
        .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Assignment>()
        .HasOne(a => a.Receiver)
        .WithMany()
        .HasForeignKey(a => a.ReceiverId)
        .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Assignment>()
        .HasIndex(a => new { a.EventId, a.Round, a.GiverId })
        .IsUnique();

        builder.Entity<Assignment>()
        .HasIndex(a => new { a.EventId, a.Round, a.ReceiverId })
        .IsUnique();

    }

}
