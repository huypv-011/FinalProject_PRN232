using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookingVilla.DTOs;
using BookingVilla.Services;
using Repository;
using BussinessObject;

namespace BookingVilla.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAccountRepositories _accountRepo;
        private readonly ICustomerRepositories _customerRepo;
        private readonly IJwtService _jwtService;

        public AuthController(IAccountRepositories accountRepo, ICustomerRepositories customerRepo, IJwtService jwtService)
        {
            _accountRepo = accountRepo;
            _customerRepo = customerRepo;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<LoginResponse>.ErrorResponse("Invalid request", 
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
            }

            var user = _accountRepo.CheckAccount(request.Username, request.Password);
            if (user == null)
            {
                return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse("Username or password invalid!"));
            }

            if (!user.Status)
            {
                return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse("This account was locked"));
            }

            UserInfo userInfo;
            if (user.Role == "us" || user.Role == "ad")
            {
                var customerInfo = _accountRepo.GetCustomer(user.IdAccount);
                if (customerInfo == null)
                {
                    return NotFound(ApiResponse<LoginResponse>.ErrorResponse("Customer information not found"));
                }

                userInfo = new UserInfo
                {
                    Id = customerInfo.IdCustomer,
                    Name = customerInfo.Name ?? "",
                    Address = customerInfo.Address,
                    Email = customerInfo.Email,
                    Phone = customerInfo.Phone,
                    Avatar = customerInfo.Avatar,
                    Role = user.Role ?? "",
                    UserType = "Customer"
                };
            }
            else
            {
                var employeeInfo = _accountRepo.GetEmployee(user.IdAccount);
                if (employeeInfo == null)
                {
                    return NotFound(ApiResponse<LoginResponse>.ErrorResponse("Employee information not found"));
                }

                userInfo = new UserInfo
                {
                    Id = employeeInfo.IdEmployee,
                    Name = employeeInfo.Name ?? "",
                    Address = employeeInfo.Address,
                    Email = employeeInfo.Email,
                    Phone = employeeInfo.Phone,
                    Avatar = employeeInfo.Avatar,
                    Salary = employeeInfo.Salary,
                    Role = user.Role ?? "",
                    UserType = "Employee"
                };
            }

            var token = _jwtService.GenerateToken(user, userInfo);
            var response = new LoginResponse
            {
                Token = token,
                UserInfo = userInfo,
                Message = "Login successful"
            };

            return Ok(ApiResponse<LoginResponse>.SuccessResponse(response, "Login successful"));
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
            }

            if (request.Password != request.ConfirmPassword)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Password and Confirm Password do not match!"));
            }

            // Check if username exists
            if (_accountRepo.GetUserByUsername(request.Username) != null)
            {
                return Conflict(ApiResponse<object>.ErrorResponse("Username already exists!"));
            }

            // Check if email exists
            if (_customerRepo.GetUserByEmail(request.Email) != null)
            {
                return Conflict(ApiResponse<object>.ErrorResponse("Email already exists!"));
            }

            // Create new account
            var newAccount = new Account
            {
                UserName = request.Username,
                PassWord = request.Password, // In production, hash the password
                Role = "us",
                Status = true
            };
            var accountId = _accountRepo.AddAccount(newAccount);

            // Get the created account
            var createdAccount = _accountRepo.GetUserByUsername(request.Username);
            if (createdAccount == null)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Failed to create an account!"));
            }

            // Create customer linked to the created account
            var newCustomer = new Customer
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                IdCustomerNavigation = createdAccount
            };
            _customerRepo.AddCustomer(newCustomer);

            return Ok(ApiResponse<object>.SuccessResponse(null, "Registration successful"));
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            var roleClaim = User.FindFirst("Role")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(roleClaim))
            {
                return Unauthorized(ApiResponse<UserInfo>.ErrorResponse("Invalid token"));
            }

            var userId = int.Parse(userIdClaim);
            UserInfo userInfo;

            if (roleClaim == "us" || roleClaim == "ad")
            {
                var customerInfo = _accountRepo.GetCustomer(userId);
                if (customerInfo == null)
                {
                    return NotFound(ApiResponse<UserInfo>.ErrorResponse("Customer information not found"));
                }

                userInfo = new UserInfo
                {
                    Id = customerInfo.IdCustomer,
                    Name = customerInfo.Name ?? "",
                    Address = customerInfo.Address,
                    Email = customerInfo.Email,
                    Phone = customerInfo.Phone,
                    Avatar = customerInfo.Avatar,
                    Role = customerInfo.IdCustomerNavigation.Role ?? "",
                    UserType = "Customer"
                };
            }
            else
            {
                var employeeInfo = _accountRepo.GetEmployee(userId);
                if (employeeInfo == null)
                {
                    return NotFound(ApiResponse<UserInfo>.ErrorResponse("Employee information not found"));
                }

                userInfo = new UserInfo
                {
                    Id = employeeInfo.IdEmployee,
                    Name = employeeInfo.Name ?? "",
                    Address = employeeInfo.Address,
                    Email = employeeInfo.Email,
                    Phone = employeeInfo.Phone,
                    Avatar = employeeInfo.Avatar,
                    Salary = employeeInfo.Salary,
                    Role = employeeInfo.IdEmployeeNavigation.Role ?? "",
                    UserType = "Employee"
                };
            }

            return Ok(ApiResponse<UserInfo>.SuccessResponse(userInfo));
        }
    }
}

