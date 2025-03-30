using BussinessObject;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class BookingHistoryRepositories : IBookingHistoryRepositories
    {
        private readonly BookingHistoryDAO _bookinghistoryDAO;

        public BookingHistoryRepositories(BookingVillaPrnContext context)
        {
            _bookinghistoryDAO = new BookingHistoryDAO(context);
        }
        public List<BookingHistory> GetAllBookingHistoryNoStatus(int idCustomer) => _bookinghistoryDAO.GetAllBookingHistoryNoStatus(idCustomer);

        public List<BookingHistory> GetAllBookingHistoryStatus(int idCustomer) => _bookinghistoryDAO.GetAllBookingHistoryStatus(idCustomer);

        public BookingHistory GetBookingHistoryById(int idBooking) => _bookinghistoryDAO.GetBookingHistoryById(idBooking);
    }
}
