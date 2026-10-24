using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SecretSanta.Models;
using SQLitePCL;

namespace SecretSanta.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<Participant> Participants { get; set; } = null!;

    public DbSet<Assignment> Assignments { get; set; } = null!;
    public DbSet<WishlistItem> WishlistItems { get; set; } = null!;
    public DbSet<ParticipantInvitation> ParticipantInvitations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Event>()
        .HasOne(e => e.Creator)
        .WithMany()
        .HasForeignKey(e => e.CreatorId)
        .OnDelete(DeleteBehavior.Restrict);

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

        builder.Entity<WishlistItem>()
            .HasOne(w => w.Participant)
            .WithMany(p => p.WishlistItems)
            .HasForeignKey(w => w.ParticipantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ParticipantInvitation>()
            .HasOne(i => i.Participant)
            .WithMany()
            .HasForeignKey(i => i.ParticipantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ParticipantInvitation>()
            .HasIndex(i => i.TokenHash)
            .IsUnique();

        builder.Entity<ParticipantInvitation>()
            .Property(i => i.TokenHash)
            .HasMaxLength(64)
            .IsRequired();

    }

}