using BussinessObject;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Repository;

namespace BookingVilla.Pages.ManageServicePages
{
    public class ManageServiceModel : PageModel
    {
        private readonly IServiceRepositories _serviceRepositories;
        public ManageServiceModel(IServiceRepositories serviceRepositories)
        {
            _serviceRepositories = serviceRepositories;
        }
        public List<Service> Services { get; set; }
        public string Search { get; set; }
        public void OnGet()
        {
            Services = _serviceRepositories.GetAllService();
        }
        public IActionResult OnGetDelete(int id)
        {
            _serviceRepositories.DeleteService(id);
            bool result = true;
            TempData["SuccessMessage"] = result ? "ok" : "fail";
            return RedirectToPage("/ManageService");
        }
    }
}
