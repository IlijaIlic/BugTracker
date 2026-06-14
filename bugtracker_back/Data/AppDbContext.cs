using System;
using System.Security.Cryptography.X509Certificates;
using bugtracker_back.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<Manager> Managers { get; set; }
    public DbSet<Tester> Testers { get; set; }
    
    public DbSet<Bug> Bugs { get; set; }
    public DbSet<Project> Projects { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options): base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Bug>()
            .HasOne(b => b.Owner)
            .WithMany()
            .HasForeignKey(b => b.OwnerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
