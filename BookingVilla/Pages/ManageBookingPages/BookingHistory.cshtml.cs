using BussinessObject;
using BookingVilla.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Repository;
using System.Text.Json;

namespace BookingVilla.Pages.ManageBookingPages
{
    public class BookingHistoryModel : PageModel
    {
        private readonly IBookingHistoryRepositories _bookingHistoryRepositories;
        private readonly ICancelBookingRepositories _cancelBookingRepositories;
        private readonly IHubContext<NewsHub> _hubContext;
        public int TotalAccounts { get; set; }
        public int NumberPage { get; set; }
        public int Index { get; set; } = 1;
        public BookingHistoryModel(IBookingHistoryRepositories bookingHistoryRepositories, ICancelBookingRepositories cancelBookingRepositories, IHubContext<NewsHub> hubContext)
        {
            _bookingHistoryRepositories = bookingHistoryRepositories;
            _cancelBookingRepositories = cancelBookingRepositories;
            _hubContext = hubContext;
        }
        public List<BookingHistory> BookingHistoryList { get; set; } = new();

        public IActionResult OnGet(int index = 1)
        {
            Index = index;
            var userInfoJson = HttpContext.Session.GetString("UserInfo");
            if (string.IsNullOrEmpty(userInfoJson))
            {
                return RedirectToPage("/Auth/Login");
            }

            var userInfo = System.Text.Json.JsonSerializer.Deserialize<UserInfo>(userInfoJson);
            if (userInfo == null)
            {
                return RedirectToPage("/Auth/Login");
            }

            int customerId = userInfo.Id;
            // Lấy danh sách lịch sử đặt phòng
            BookingHistoryList = _bookingHistoryRepositories.GetAllBookingHistoryNoStatusPagination(customerId, index);
            TotalAccounts = _bookingHistoryRepositories.GetNumberTotalBookingHistoryNoStatus(customerId);
            NumberPage = _bookingHistoryRepositories.GetNumberBookingHistoryNoStatus(customerId);
            return Page();
        }
        public IActionResult OnGetCancelBooking(int bookingId)
        {
            var userInfoJson = HttpContext.Session.GetString("UserInfo");
            if (string.IsNullOrEmpty(userInfoJson))
            {
                return new JsonResult(new { success = false, message = "User not authenticated" });
            }

            var userInfo = System.Text.Json.JsonSerializer.Deserialize<UserInfo>(userInfoJson);
            if (userInfo == null)
            {
                return new JsonResult(new { success = false, message = "Invalid user info" });
            }

            int customerId = userInfo.Id;
            // Server-side validation: kiểm tra booking tồn tại và thuộc về user
            var booking = _bookingHistoryRepositories.GetBookingHistoryById(bookingId);
            if (booking == null)
            {
                return new JsonResult(new { success = false, message = "Booking not found" });
            }

            if (booking.IdCustomer != customerId)
            {
                return new JsonResult(new { success = false, message = "You are not allowed to cancel this booking" });
            }

            // Kiểm tra đã có request hủy trước đó (không cho tạo nếu đã có pending/approved)
            if (!string.IsNullOrEmpty(booking.Status) && booking.Status != "Rejected")
            {
                return new JsonResult(new { success = false, message = "A cancel request already exists for this booking" });
            }

            // Kiểm tra thời hạn hủy: phải còn ít nhất 2 ngày trước check-in (ví dụ hôm nay 20/11, check-in 22/11)
            var currentDate = DateTime.Today;
            var checkin = booking.CheckinDate.Date;
            var dayDifference = (checkin - currentDate).TotalDays;
            if (double.IsNaN(dayDifference) || dayDifference < 2)
            {
                return new JsonResult(new { success = false, message = "Cannot cancel booking less than 2 days before check-in" });
            }

            var cancelBooking = new CancelBooking
            {
                IdBookingOnline = bookingId,
                IdCustomer = customerId,
                RequestDate = DateTime.Now,
                Status = "Pending"
            };

            bool result = _cancelBookingRepositories.AddCancelBooking(cancelBooking);
            if (result)
            {
                _hubContext.Clients.All.SendAsync("ReceiveNewCancelBooking", "Have New Cancel Booking!");
            }
            return new JsonResult(new { success = result });
        }
    }
}
