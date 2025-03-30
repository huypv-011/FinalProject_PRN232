using BussinessObject;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Repository;
using System.Text.Json;

namespace BookingVilla.Pages.ManageBookingPages
{
    public class BookingHistoryModel : PageModel
    {
        private readonly IBookingHistoryRepositories _bookingHistoryRepositories;
        private readonly ICancelBookingRepositories _cancelBookingRepositories;
        public BookingHistoryModel(IBookingHistoryRepositories bookingHistoryRepositories, ICancelBookingRepositories cancelBookingRepositories)
        {
            _bookingHistoryRepositories = bookingHistoryRepositories;
            _cancelBookingRepositories = cancelBookingRepositories;
        }
        public List<BookingHistory> BookingHistoryList { get; set; }

        public IActionResult OnGet()
        {
            var userInfoJson = HttpContext.Session.GetString("UserInfo");
            using JsonDocument doc = JsonDocument.Parse(userInfoJson);
            int customerId = doc.RootElement.GetProperty("IdCustomer").GetInt32();
            // Lấy danh sách lịch sử đặt phòng
            BookingHistoryList = _bookingHistoryRepositories.GetAllBookingHistoryNoStatus(customerId);
            return Page();
        }
        public IActionResult OnGetCancelBooking(int bookingId)
        {
            var userInfoJson = HttpContext.Session.GetString("UserInfo");
            using JsonDocument doc = JsonDocument.Parse(userInfoJson);
            int customerId = doc.RootElement.GetProperty("IdCustomer").GetInt32();
            var cancelBooking = new CancelBooking
            {
                IdBookingOnline = bookingId,
                IdCustomer = customerId,  // Lấy từ session hoặc user info
                RequestDate = DateTime.Now,
                Status = "Pending"
            };

            bool result = _cancelBookingRepositories.AddCancelBooking(cancelBooking);

            return new JsonResult(new { success = result });
        }
    }
}
