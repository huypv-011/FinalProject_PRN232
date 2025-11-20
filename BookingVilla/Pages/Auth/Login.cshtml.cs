using System;
using BookingVilla.DTOs;
using BookingVilla.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Repository;

namespace BookingVilla.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IAccountRepositories _accountRepo;
            private readonly IJwtService _jwtService;

        public LoginModel(IAccountRepositories accountRepo, IJwtService jwtService)
        {
            _accountRepo = accountRepo;
            _jwtService = jwtService;
        }

        [BindProperty]
        public string Username { get; set; }
        [BindProperty]
        public string Password { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost()
        {
            var user = _accountRepo.CheckAccount(Username, Password);
            if (user == null)
            {
                TempData["Error"] = "Username or password invalid!";
                return Page();
            }

            if (!user.Status)
            {
                TempData["Error"] = "This account was locked";
                return Page();
            }

            HttpContext.Session.SetString("UserId", user.IdAccount.ToString());
            HttpContext.Session.SetString("Role", user.Role);

                var userInfo = BuildUserInfo(user);
                HttpContext.Session.SetString("UserInfo", System.Text.Json.JsonSerializer.Serialize(userInfo));

            var token = _jwtService.GenerateToken(user, userInfo);
            HttpContext.Session.SetString("AccessToken", token);

            return user.Role == "ad" ? RedirectToPage("/ManageVillaPages/ManageVilla") : RedirectToPage("/Index");
        }

        private UserInfo BuildUserInfo(BussinessObject.Account user)
        {
            if (user.Role == "us" || user.Role == "ad")
            {
                var customerInfo = _accountRepo.GetCustomer(user.IdAccount)
                                   ?? throw new InvalidOperationException("Customer info not found.");

                return new UserInfo
                {
                    Id = customerInfo.IdCustomer,
                    Name = customerInfo.Name ?? string.Empty,
                    Address = customerInfo.Address,
                    Email = customerInfo.Email,
                    Phone = customerInfo.Phone,
                    Avatar = customerInfo.Avatar,
                    Role = user.Role ?? string.Empty,
                    UserType = "Customer"
                };
            }

            var employeeInfo = _accountRepo.GetEmployee(user.IdAccount)
                               ?? throw new InvalidOperationException("Employee info not found.");

            return new UserInfo
            {
                Id = employeeInfo.IdEmployee,
                Name = employeeInfo.Name ?? string.Empty,
                Address = employeeInfo.Address,
                Email = employeeInfo.Email,
                Phone = employeeInfo.Phone,
                Avatar = employeeInfo.Avatar,
                Salary = employeeInfo.Salary,
                Role = user.Role ?? string.Empty,
                UserType = "Employee"
            };
        }
    }
}

