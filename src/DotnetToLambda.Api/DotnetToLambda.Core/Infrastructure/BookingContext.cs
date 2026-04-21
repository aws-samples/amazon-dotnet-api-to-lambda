using DotnetToLambda.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetToLambda.Core.Infrastructure;

public class BookingContext : DbContext
{
    private readonly DatabaseConnection _connection;

    public BookingContext(DatabaseConnection connection)
    {
        _connection = connection;
    }

    public DbSet<Booking> Bookings { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySQL(_connection.ToString());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>().ToTable("Bookings");

        modelBuilder.Entity<Booking>()
            .HasKey(b => b.BookingId).HasName("PK_BookingId");
        modelBuilder.Entity<Booking>()
            .HasIndex(b => b.CustomerId)
            .HasDatabaseName("UX_Booking_CustomerId");
    }
}
