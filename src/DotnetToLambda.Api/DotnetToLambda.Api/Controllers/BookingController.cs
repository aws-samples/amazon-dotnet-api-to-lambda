using System;
using System.Text.Json;
using System.Threading.Tasks;
using DotnetToLambda.Core.Exceptions;
using DotnetToLambda.Core.ViewModels;
using DotnetToLambda.Core.Models;
using DotnetToLambda.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DotnetToLambda.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly ILogger<BookingController> _logger;
        private readonly IBookingRepository _bookingRepository;
        private readonly ICustomerService _customerService;

        public BookingController(ILogger<BookingController> logger,
            IBookingRepository bookingRepository,
            ICustomerService customerService)
        {
            this._logger = logger;
            this._bookingRepository = bookingRepository;
            this._customerService = customerService;
        }

        /// <summary>
        /// HTTP GET endpoint to list all bookings for a customer.
        /// </summary>
        /// <param name="customerId">The customer id to list for.</param>
        /// <returns>All <see cref="Booking"/> for the given customer.</returns>
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> ListForCustomer(string customerId)
        {
            this._logger.LogInformation($"Received request to list bookings for {customerId}");

            return this.Ok(await this._bookingRepository.ListForCustomer(customerId));
        }

        /// <summary>
        /// HTTP GET endpoint to view a specific booking.
        /// </summary>
        /// <param name="bookingId">The booking to retrieve.</param>
        /// <returns>The booking.</returns>
        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> Retrieve(string bookingId)
        {
            this._logger.LogInformation($"Received request for booking {bookingId}");

            var booking = await this._bookingRepository.Retrieve(bookingId);

            if (booking == null)
            {
                return this.NotFound();
            }
            
            return this.Ok(booking);
        }

        /// <summary>
        /// HTTP POST endpoint to reserve a booking.
        /// </summary>
        /// <param name="request">The <see cref="ReserveBookingDTO"/> containing details of the reservation request.</param>
        /// <returns>The created <see cref="Booking"/>.</returns>
        [HttpPost("reserve")]
        public async Task<IActionResult> Reserve ([FromBody] ReserveBookingDTO request)
        {
            this._logger.LogInformation("Received request to reserve a new booking:");

            this._logger.LogInformation(JsonSerializer.Serialize(request));

            if (!await this._customerService.CustomerExists(request.CustomerId))
            {
                this._logger.LogWarning($"Customer {request.CustomerId} does not exist in the customer service");
                return this.BadRequest();
            }

            var booking = Booking.Create(Guid.NewGuid().ToString(), request.CustomerId, request.OutboundFlightId, request.ChargeId);

            this._logger.LogInformation($"Booking created with Id ${booking.BookingId}");

            try
            {
                await this._bookingRepository.Add(booking);
            }
            catch (DuplicateRequestException ex)
            {
                return this.Ok(ex.DuplicateBooking);
            }

            this._logger.LogInformation("Booking added");

            return this.Ok(booking);
        }

        /// <summary>
        /// HTTP POST endpoint to confirm a booking.
        /// </summary>
        /// <param name="request">The <see cref="ConfirmBookingDTO"/> containing details of the reservation to be confirmed.</param>
        /// <returns>The created <see cref="Booking"/>.</returns>
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmBooking ([FromBody] ConfirmBookingDTO request)
        {
            this._logger.LogInformation("Received request to confirm a booking:");

            this._logger.LogInformation(JsonSerializer.Serialize(request));

            var existingBooking = await this._bookingRepository.Retrieve(request.BookingId);

            if (existingBooking == null)
            {
                return this.NotFound();
            }

            this._logger.LogInformation($"Found booking, confirmed");

            existingBooking.Confirm();

            await this._bookingRepository.Update(existingBooking);

            this._logger.LogInformation("Booking updated");

            return this.Ok(existingBooking);
        }

        /// <summary>
        /// HTTP PUT endpoint to cancel a booking.
        /// </summary>
        /// <param name="request">The <see cref="CancelBookingDTO"/> containing details of the reservation to be cancelled.</param>
        /// <returns>The created <see cref="Booking"/>.</returns>
        [HttpPut("cancel")]
        public async Task<IActionResult> CancelBooking ([FromBody] CancelBookingDTO request)
        {
            this._logger.LogInformation("Received request to cancel a booking:");

            this._logger.LogInformation(JsonSerializer.Serialize(request));

            var existingBooking = await this._bookingRepository.Retrieve(request.BookingId);

            if (existingBooking == null)
            {
                return this.NotFound();
            }

            this._logger.LogInformation($"Found booking, cancelling");

            existingBooking.Cancel(request.Reason);

            await this._bookingRepository.Update(existingBooking);

            this._logger.LogInformation("Booking updated");

            return this.Ok(existingBooking);
        }
    }
}
