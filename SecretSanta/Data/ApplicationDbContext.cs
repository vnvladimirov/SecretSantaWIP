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
    }

}
