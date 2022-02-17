using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotnetToLambda.Core.Models
{
    public interface IBookingRepository
    {
        Task Add(Booking booking);

        Task<List<Booking>> ListForCustomer(string customerId);

        Task<Booking> Retrieve(string bookingId);

        Task Update(Booking booking);
    }
}
