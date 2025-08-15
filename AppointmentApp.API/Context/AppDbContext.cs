using AppointmentApp.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppointmentApp.API.Context;

public class AppDbContext:DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<CoachProfile> CoachProfiles => Set<CoachProfile>();
    
    public DbSet<Service> Services => Set<Service>();
    
    public DbSet<AvailabilityRule> AvailabilityRules => Set<AvailabilityRule>();
    
    public DbSet<Notification> Notifications => Set<Notification>();
    
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<User>(e => {
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).IsRequired();
            e.Property(x => x.PasswordHash).IsRequired();
        });

        mb.Entity<Role>(e => { e.HasIndex(x => x.Name).IsUnique(); });

        mb.Entity<UserRole>(e => {
            e.HasKey(x => new { x.UserId, x.RoleId });
            e.HasOne(x => x.User).WithMany(u => u.UserRoles).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Role).WithMany(r => r.UserRoles).HasForeignKey(x => x.RoleId);
        });

        mb.Entity<CoachProfile>(e => {
            e.HasIndex(x => x.UserId).IsUnique();
        });

        // Seed roller
        mb.Entity<Role>().HasData(
            new Role { Id = 1, Name = "student" },
            new Role { Id = 2, Name = "coach" },
            new Role { Id = 3, Name = "admin" }
        );
        
        mb.Entity<Service>(e => {
            e.Property(x => x.Name).IsRequired();
            e.HasIndex(x => new { x.CoachId, x.Name }).IsUnique();
        });

        mb.Entity<AvailabilityRule>(e => {
            e.Property(x => x.Days).HasColumnType("int[]"); // DayOfWeek = int
        });
        
        // Booking ilişkileri
        mb.Entity<Booking>(e =>
        {
            e.HasOne(b => b.Coach)
                .WithMany()
                .HasForeignKey(b => b.CoachId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(b => b.Student)
                .WithMany()
                .HasForeignKey(b => b.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sık kullanılan sorgular için indeksler (coach ve zaman aralığına göre)
            e.HasIndex(b => new { b.CoachId, b.StartsAt });
            e.HasIndex(b => new { b.StudentId, b.StartsAt });
        });
        
        mb.Entity<Notification>(e =>
        {
            e.HasOne(n => n.RecipientUser)
                .WithMany()
                .HasForeignKey(n => n.RecipientUserId)
                .OnDelete(DeleteBehavior.Cascade); // kullanıcı silinirse bildirimi de sil
        });
    }
}