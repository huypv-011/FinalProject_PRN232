using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookingVilla.DTOs;
using Repository;
using BussinessObject;

namespace BookingVilla.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VillaController : ControllerBase
    {
        private readonly IVillaRepositories _villaRepository;

        public VillaController(IVillaRepositories villaRepository)
        {
            _villaRepository = villaRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAllVillas([FromQuery] int page = 1, [FromQuery] string? sortBy = null)
        {
            try
            {
                List<Villa> villas;

                switch (sortBy?.ToLower())
                {
                    case "price":
                        villas = _villaRepository.GetAllVillasByPrice(page);
                        break;
                    case "people":
                        villas = _villaRepository.GetAllVillasByPeople(page);
                        break;
                    case "room":
                        villas = _villaRepository.GetAllVillasByRoom(page);
                        break;
                    default:
                        villas = _villaRepository.GetAllVillasByPrice(page);
                        break;
                }

                return Ok(ApiResponse<List<Villa>>.SuccessResponse(villas));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<Villa>>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult GetVillaById(int id)
        {
            try
            {
                var villa = _villaRepository.GetVillaById(id, DateTime.Today);
                if (villa == null)
                {
                    return NotFound(ApiResponse<Villa>.ErrorResponse("Villa not found"));
                }

                return Ok(ApiResponse<Villa>.SuccessResponse(villa));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<Villa>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        [HttpGet("available")]
        [AllowAnonymous]
        public IActionResult GetAvailableVillas([FromQuery] DateTime from, [FromQuery] DateTime to, 
            [FromQuery] int amountRoom, [FromQuery] int amountPeople)
        {
            try
            {
                if (to <= from)
                {
                    return BadRequest(ApiResponse<List<Villa>>.ErrorResponse("Check-in date must be before check-out date"));
                }

                var villas = _villaRepository.GetAvailableVillas(from, to, DateTime.Today, amountRoom, amountPeople);
                return Ok(ApiResponse<List<Villa>>.SuccessResponse(villas));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<Villa>>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public IActionResult SearchVillas([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(ApiResponse<List<Villa>>.ErrorResponse("Search name is required"));
                }

                var villas = _villaRepository.SearchByName(name);
                return Ok(ApiResponse<List<Villa>>.SuccessResponse(villas));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<Villa>>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        [HttpPost]
        [Authorize(Roles = "ad")]
        public IActionResult AddVilla([FromBody] Villa villa)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<int>.ErrorResponse("Invalid villa data",
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                var villaId = _villaRepository.AddVilla(villa);
                return CreatedAtAction(nameof(GetVillaById), new { id = villaId }, 
                    ApiResponse<int>.SuccessResponse(villaId, "Villa added successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<int>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ad")]
        public IActionResult UpdateVilla(int id, [FromBody] Villa villa)
        {
            try
            {
                if (id != villa.IdVilla)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Villa ID mismatch"));
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid villa data",
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                _villaRepository.UpdateVilla(villa);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Villa updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        [HttpGet("total")]
        [AllowAnonymous]
        public IActionResult GetTotalVillas()
        {
            try
            {
                var total = _villaRepository.GetNumberTotalVilla();
                return Ok(ApiResponse<int>.SuccessResponse(total));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<int>.ErrorResponse($"Error: {ex.Message}"));
            }
        }
    }
}

