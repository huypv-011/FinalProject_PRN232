using BussinessObject;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Repository;

namespace BookingVilla.Pages.ManageAccountPages
{
    public class ManageAccountModel : PageModel
    {
        private readonly IAccountRepositories _accountRepositories;
        public List<Account> AccountList { get; set; } = new();
        public int TotalAccounts { get; set; }
        public int NumberPage { get; set; }
        public int Index { get; set; } = 1;
        public string Search { get; set; }
        public ManageAccountModel(IAccountRepositories accountRepositories)
        {
            _accountRepositories = accountRepositories;
        }
        public void OnGet(int index = 1)
        {
            Index = index;

            // Lấy danh sách tài khoản
            AccountList = _accountRepositories.GetAllAccountPagination(index);

            // Tính tổng số tài khoản
            TotalAccounts = _accountRepositories.GetNumberTotalAccount();

            // Tính số trang
            NumberPage = _accountRepositories.GetNumberAccount();
        }
    }
}
