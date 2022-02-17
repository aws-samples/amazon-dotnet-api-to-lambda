using DotnetToLambda.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DotnetToLambda.Core.Infrastructure
{
    public class BookingContext : DbContext
    {
        private readonly DatabaseConnection _connection;

        public BookingContext(DatabaseConnection connection)
        {
            this._connection = connection;
        }
        
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(this._connection.ToString());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Booking>().ToTable("Bookings");

            modelBuilder.Entity<Booking>()
                .HasKey(b => b.BookingId).HasName("PK_BookingId");
            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.CustomerId)
                .HasName("UX_Booking_CustomerId");
        }
    }
}