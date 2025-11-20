using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookingVilla.DTOs;
using Repository;
using BussinessObject;
using System.Security.Claims;

namespace BookingVilla.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingRepositories _bookingRepository;
        private readonly IVillaRepositories _villaRepository;
        private readonly IServiceRepositories _serviceRepository;

        public BookingController(
            IBookingRepositories bookingRepository,
            IVillaRepositories villaRepository,
            IServiceRepositories serviceRepository)
        {
            _bookingRepository = bookingRepository;
            _villaRepository = villaRepository;
            _serviceRepository = serviceRepository;
        }

        [HttpPost]
        [Authorize(Roles = "us,ad")]
        public IActionResult CreateBooking([FromBody] CreateBookingRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid booking data",
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user"));
                }

                var customerId = int.Parse(userIdClaim);

                // Validate dates
                if (request.CheckoutDate <= request.CheckinDate)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Check-in date must be before check-out date"));
                }

                // Check if villa exists and is available
                var villa = _villaRepository.GetVillaById(request.VillaId, DateTime.Today);
                if (villa == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Villa not found"));
                }

                // Calculate price
                var days = (request.CheckoutDate - request.CheckinDate).Days;
                var price = villa.Price * days;

                // Create booking
                var booking = new BookingOnline
                {
                    IdCustomer = customerId,
                    IdVilla = request.VillaId,
                    CheckinDate = request.CheckinDate,
                    CheckoutDate = request.CheckoutDate,
                    AmountOfPeople = request.AmountOfPeople,
                    PriceBooking = price
                };

                var bookingId = _bookingRepository.AddBooking(booking);

                // Add services if provided
                if (request.ServiceIds != null && request.ServiceIds.Any())
                {
                    foreach (var serviceId in request.ServiceIds)
                    {
                        var addService = new AddService
                        {
                            IdBookingOnline = bookingId,
                            IdService = serviceId
                        };
                        _bookingRepository.AddServiceBooking(bookingId, addService);
                    }
                }

                return CreatedAtAction(nameof(GetBookingById), new { id = bookingId },
                    ApiResponse<int>.SuccessResponse(bookingId, "Booking created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetBookingById(int id)
        {
            try
            {
                // This would need to be implemented in the repository
                // For now, returning a placeholder
                return Ok(ApiResponse<BookingOnline>.SuccessResponse(null, "Booking retrieved"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<BookingOnline>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        [HttpGet("calculate-price")]
        [AllowAnonymous]
        public IActionResult CalculatePrice([FromQuery] int villaId, [FromQuery] DateTime checkinDate, 
            [FromQuery] DateTime checkoutDate, [FromQuery] int amountOfPeople)
        {
            try
            {
                if (checkoutDate <= checkinDate)
                {
                    return BadRequest(ApiResponse<PriceCalculationResponse>.ErrorResponse("Check-in date must be before check-out date"));
                }

                var villa = _villaRepository.GetVillaById(villaId, DateTime.Today);
                if (villa == null)
                {
                    return NotFound(ApiResponse<PriceCalculationResponse>.ErrorResponse("Villa not found"));
                }

                var days = (checkoutDate - checkinDate).Days;
                var basePrice = villa.Price * days;
                var services = _serviceRepository.GetAllService();
                var totalPrice = basePrice;

                var response = new PriceCalculationResponse
                {
                    VillaId = villaId,
                    VillaName = villa.Name ?? "",
                    BasePrice = basePrice,
                    Days = days,
                    PricePerDay = villa.Price,
                    AvailableServices = services.Select(s => new ServiceDto
                    {
                        Id = s.IdService,
                        Name = s.Name ?? "",
                        Price = s.Price
                    }).ToList(),
                    TotalPrice = totalPrice
                };

                return Ok(ApiResponse<PriceCalculationResponse>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PriceCalculationResponse>.ErrorResponse($"Error: {ex.Message}"));
            }
        }
    }
}

