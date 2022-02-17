using System;
using DotnetToLambda.Core.Models;

namespace DotnetToLambda.Core.Exceptions
{
    public class DuplicateRequestException : Exception
    {
        public DuplicateRequestException(Booking b) : base()
        {
            this.DuplicateBooking = b;
        }
        
        public Booking DuplicateBooking { get; private set; }
    }
}