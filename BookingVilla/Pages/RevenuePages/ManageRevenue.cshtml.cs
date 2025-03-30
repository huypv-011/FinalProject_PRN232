using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookingVilla.Pages.RevenuePages
{
    public class ManageRevenueModel : PageModel
    {
        public List<int> Years { get; set; } = new List<int>();
        public int YearChoosing { get; set; }
        public Dictionary<string, decimal> MonthlyRevenue { get; set; } = new Dictionary<string, decimal>();

        public ManageRevenueModel()
        {
            MonthlyRevenue = new Dictionary<string, decimal>
            {
                { "Jan", 0 }, { "Feb", 0 }, { "Mar", 0 }, { "Apr", 0 }, { "May", 0 }, { "Jun", 0 },
                { "Jul", 0 }, { "Aug", 0 }, { "Sep", 0 }, { "Oct", 0 }, { "Nov", 0 }, { "Dec", 0 }
            };
        }
    }
}
