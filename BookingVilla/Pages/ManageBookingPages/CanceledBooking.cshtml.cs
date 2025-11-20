using BussinessObject;
using BookingVilla.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Repository;
using System.Text.Json;

namespace BookingVilla.Pages.ManageBookingPages
{
    public class CanceledBookingModel : PageModel
    {
        private readonly IBookingHistoryRepositories _bookingHistoryRepositories;

        public List<BookingHistory> BookingHistoryList { get; set; }

        public CanceledBookingModel(IBookingHistoryRepositories bookingHistoryRepositories)
        {
            _bookingHistoryRepositories = bookingHistoryRepositories;
        }
        public IActionResult OnGet()
        {
            var userInfoJson = HttpContext.Session.GetString("UserInfo");
            if (string.IsNullOrEmpty(userInfoJson))
            {
                return RedirectToPage("/Auth/Login");
            }

            var userInfo = JsonSerializer.Deserialize<UserInfo>(userInfoJson);
            if (userInfo == null)
            {
                return RedirectToPage("/Auth/Login");
            }

            int customerId = userInfo.Id;
            BookingHistoryList = _bookingHistoryRepositories.GetAllBookingHistoryStatus(customerId);
            return Page();
        }
    }
}
