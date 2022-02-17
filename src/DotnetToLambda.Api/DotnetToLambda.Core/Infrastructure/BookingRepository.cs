using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetToLambda.Core.Exceptions;
using DotnetToLambda.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetToLambda.Core.Infrastructure
{
    public class BookingRepository : IBookingRepository
    {
        private readonly BookingContext _context;
        private readonly ILogger<BookingRepository> _logger;

        public BookingRepository(BookingContext context, ILogger<BookingRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Add(Booking booking)
        {
            this._logger.LogInformation($"Attempting to add booking, first checking if duplicate exists for {booking.OutboundFlightId} and customer {booking.CustomerId}");
            
            var existingBookingForFlight = await this._context.Bookings.FirstOrDefaultAsync(p =>
                p.OutboundFlightId.Equals(booking.OutboundFlightId) && p.CustomerId.Equals(booking.CustomerId));

            if (existingBookingForFlight != null)
            {
                this._logger.LogWarning("Duplicate request received for booking");
                
                throw new DuplicateRequestException(existingBookingForFlight);
            }
            
            this._context.Bookings.Add(booking);

            await this._context.SaveChangesAsync();
        }

        public async Task<List<Booking>> ListForCustomer(string customerId) =>
            await this._context.Bookings.Where(p => p.CustomerId.Equals(customerId)).ToListAsync();

        public async Task<Booking> Retrieve(string bookingId) =>
            await this._context.Bookings.FirstOrDefaultAsync(p => p.BookingId == bookingId);

        public async Task Update(Booking booking)
        {
            this._context.Bookings.Update(booking);

            await this._context.SaveChangesAsync();
        }
    }
}
