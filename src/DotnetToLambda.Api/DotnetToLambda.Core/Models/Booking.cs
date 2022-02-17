using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetToLambda.Core.Models
{
    public class Booking
    {
        private Booking()
        {
            this.Status = BookingStatus.Reserved;
        }

        public static Booking Create(string bookingId, string customerId, string outboundFlightId, string chargeId)
        {
            return new Booking()
            {
                BookingId = bookingId,
                CustomerId = customerId,
                OutboundFlightId = outboundFlightId,
                ChargeId = chargeId,
            };
        }

        public string OutboundFlightId { get; private set; }

        public string CustomerId { get; private set; }

        public string ChargeId { get; private set; }

        public string BookingId { get; private set; }
        
        public BookingStatus Status { get; private set; }
        
        public DateTime? ConfirmedOn { get; private set; }
        
        public string CancellationReason { get; private set; }

        public void Confirm()
        {
            if (this.Status == BookingStatus.Cancelled)
            {
                return;
            }
            
            this.Status = BookingStatus.Confirmed;
            this.ConfirmedOn = DateTime.Now;
        }

        public void Cancel(string reason)
        {
            this.Status = BookingStatus.Cancelled;
            this.ConfirmedOn = DateTime.Now;
            this.CancellationReason = reason;
        }
    }
}
