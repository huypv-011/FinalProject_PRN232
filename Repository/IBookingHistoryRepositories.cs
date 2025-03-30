using BussinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IBookingHistoryRepositories
    {
        List<BookingHistory> GetAllBookingHistoryStatus(int idCustomer);
        List<BookingHistory> GetAllBookingHistoryNoStatus(int idCustomer);
        BookingHistory GetBookingHistoryById(int idBooking);
    }
}
