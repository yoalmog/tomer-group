using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TomerGroup.Core.Models;

namespace TomerGroup.Infrastructure.Data;

public class TomerDbContext : DbContext
{
    public TomerDbContext(DbContextOptions<TomerDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<TripDay> TripDays => Set<TripDay>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<HotelBooking> HotelBookings => Set<HotelBooking>();
    public DbSet<Transportation> Transportations => Set<Transportation>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Guide> Guides => Set<Guide>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<MessageTemplate> MessageTemplates => Set<MessageTemplate>();
    public DbSet<AIConversation> AIConversations => Set<AIConversation>();
    public DbSet<AIRequest> AIRequests => Set<AIRequest>();
    public DbSet<BrandSettings> BrandSettings => Set<BrandSettings>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<PhoneVerificationCode> PhoneVerificationCodes => Set<PhoneVerificationCode>();
    public DbSet<Destination> Destinations => Set<Destination>();
    public DbSet<PackingItem> PackingItems => Set<PackingItem>();
    public DbSet<TripMemory> TripMemories => Set<TripMemory>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<SupportTicketMessage> SupportTicketMessages => Set<SupportTicketMessage>();
    public DbSet<StaffTask> StaffTasks => Set<StaffTask>();
    public DbSet<TrekRouteEntity> TrekRoutes => Set<TrekRouteEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // String list JSON value converter
        var stringListConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

        var stringListComparer = new ValueComparer<List<string>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        var dictConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<Dictionary<string, string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>());

        var dictComparer = new ValueComparer<Dictionary<string, string>>(
            (d1, d2) => d1 != null && d2 != null && d1.Count == d2.Count && !d1.Except(d2).Any(),
            d => d.Aggregate(0, (a, p) => HashCode.Combine(a, p.Key.GetHashCode(), p.Value.GetHashCode())),
            d => new Dictionary<string, string>(d));

        // User & Customer
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasQueryFilter(u => !u.IsDeleted);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(r => r.Permissions)
                .HasConversion(stringListConverter)
                .Metadata.SetValueComparer(stringListComparer);
            entity.HasQueryFilter(r => !r.IsDeleted);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(c => c.AuthUserId);
            entity.HasIndex(c => c.Email);

            entity.HasOne(c => c.User)
                .WithOne(u => u.CustomerProfile)
                .HasForeignKey<Customer>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(c => !c.IsDeleted);
        });

        // Booking
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasIndex(b => b.BookingCode).IsUnique();
            entity.Property(b => b.TotalAmount).HasPrecision(18, 2);
            entity.Property(b => b.PaidAmount).HasPrecision(18, 2);

            entity.HasOne(b => b.Customer)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Trip)
                .WithMany(t => t.Bookings)
                .HasForeignKey(b => b.TripId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasQueryFilter(b => !b.IsDeleted);
        });

        // Trip, TripDay, Activity
        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasIndex(t => t.TripCode).IsUnique();
            entity.Property(t => t.TotalRevenue).HasPrecision(18, 2);
            entity.Property(t => t.TotalCost).HasPrecision(18, 2);

            entity.HasOne(t => t.Customer)
                .WithMany(c => c.Trips)
                .HasForeignKey(t => t.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(t => !t.IsDeleted);
        });

        modelBuilder.Entity<TripDay>(entity =>
        {
            entity.HasOne(td => td.Trip)
                .WithMany(t => t.Days)
                .HasForeignKey(td => td.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(td => !td.IsDeleted);
        });

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasOne(a => a.TripDay)
                .WithMany(td => td.Activities)
                .HasForeignKey(a => a.TripDayId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Driver)
                .WithMany()
                .HasForeignKey(a => a.DriverId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(a => a.Guide)
                .WithMany(g => g.Activities)
                .HasForeignKey(a => a.GuideId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasQueryFilter(a => !a.IsDeleted);
        });

        // Tour
        modelBuilder.Entity<Tour>(entity =>
        {
            entity.Property(t => t.AdultPrice).HasPrecision(18, 2);
            entity.Property(t => t.ChildPrice).HasPrecision(18, 2);
            entity.Property(t => t.PrivatePrice).HasPrecision(18, 2);
            entity.Property(t => t.AgencyCost).HasPrecision(18, 2);

            entity.Property(t => t.ImageUrls).HasConversion(stringListConverter).Metadata.SetValueComparer(stringListComparer);
            entity.Property(t => t.IncludedServices).HasConversion(stringListConverter).Metadata.SetValueComparer(stringListComparer);
            entity.Property(t => t.ExcludedServices).HasConversion(stringListConverter).Metadata.SetValueComparer(stringListComparer);
            entity.Property(t => t.LocationsVisited).HasConversion(stringListConverter).Metadata.SetValueComparer(stringListComparer);

            entity.HasQueryFilter(t => !t.IsDeleted);
        });

        // Hotel & HotelBooking
        modelBuilder.Entity<Hotel>(entity =>
        {
            entity.Property(h => h.RoomTypes).HasConversion(stringListConverter).Metadata.SetValueComparer(stringListComparer);
            entity.Property(h => h.PhotoUrls).HasConversion(stringListConverter).Metadata.SetValueComparer(stringListComparer);
            entity.HasQueryFilter(h => !h.IsDeleted);
        });

        modelBuilder.Entity<HotelBooking>(entity =>
        {
            entity.Property(hb => hb.AgencyCost).HasPrecision(18, 2);
            entity.Property(hb => hb.SellingPrice).HasPrecision(18, 2);

            entity.HasOne(hb => hb.Customer)
                .WithMany()
                .HasForeignKey(hb => hb.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(hb => !hb.IsDeleted);
        });

        // Guide
        modelBuilder.Entity<Guide>(entity =>
        {
            entity.Property(g => g.Languages).HasConversion(stringListConverter).Metadata.SetValueComparer(stringListComparer);
            entity.HasQueryFilter(g => !g.IsDeleted);
        });

        // Transportation, Vehicle, Driver
        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasQueryFilter(d => !d.IsDeleted);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasOne(v => v.AssignedDriver)
                .WithMany(d => d.Vehicles)
                .HasForeignKey(v => v.AssignedDriverId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasQueryFilter(v => !v.IsDeleted);
        });

        modelBuilder.Entity<Transportation>(entity =>
        {
            entity.HasOne(t => t.Driver)
                .WithMany(d => d.Transportations)
                .HasForeignKey(t => t.DriverId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.Booking)
                .WithMany(b => b.Transportations)
                .HasForeignKey(t => t.BookingId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.Customer)
                .WithMany()
                .HasForeignKey(t => t.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasQueryFilter(t => !t.IsDeleted);
        });

        // Payment & Expense
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(p => p.Amount).HasPrecision(18, 2);
            entity.HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Customer)
                .WithMany(c => c.Payments)
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Document
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasOne(d => d.Customer)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(d => !d.IsDeleted);
        });

        // BrandSettings
        modelBuilder.Entity<BrandSettings>(entity =>
        {
            entity.Property(bs => bs.SocialLinks)
                .HasConversion(dictConverter)
                .Metadata.SetValueComparer(dictComparer);
        });

        // Experience & Traveler Entities
        modelBuilder.Entity<PackingItem>(entity =>
        {
            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        modelBuilder.Entity<TripMemory>(entity =>
        {
            entity.HasQueryFilter(m => !m.IsDeleted);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.Property(r => r.PhotoUrls)
                .HasConversion(stringListConverter)
                .Metadata.SetValueComparer(stringListComparer);
            entity.HasQueryFilter(r => !r.IsDeleted);
        });

        // Admin Operations Entities
        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.HasOne(t => t.Customer)
                .WithMany()
                .HasForeignKey(t => t.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Trip)
                .WithMany()
                .HasForeignKey(t => t.TripId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.AssignedStaffUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedStaffUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(t => t.Messages)
                .WithOne(m => m.SupportTicket)
                .HasForeignKey(m => m.SupportTicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(t => !t.IsDeleted);
        });

        modelBuilder.Entity<SupportTicketMessage>(entity =>
        {
            entity.HasOne(m => m.SenderUser)
                .WithMany()
                .HasForeignKey(m => m.SenderUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasQueryFilter(m => !m.IsDeleted);
        });

        modelBuilder.Entity<StaffTask>(entity =>
        {
            entity.HasOne(st => st.Customer)
                .WithMany()
                .HasForeignKey(st => st.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(st => st.Trip)
                .WithMany()
                .HasForeignKey(st => st.TripId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(st => st.AssignedStaffUser)
                .WithMany()
                .HasForeignKey(st => st.AssignedStaffUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(st => st.CreatedByUser)
                .WithMany()
                .HasForeignKey(st => st.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasQueryFilter(st => !st.IsDeleted);
        });

        modelBuilder.Entity<TrekRouteEntity>(entity =>
        {
            entity.HasOne(tr => tr.Tour)
                .WithMany()
                .HasForeignKey(tr => tr.TourId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasQueryFilter(tr => !tr.IsDeleted);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                if (entry.Entity.UpdatedAt == default)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

