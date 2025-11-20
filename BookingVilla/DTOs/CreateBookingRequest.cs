using System.ComponentModel.DataAnnotations;

namespace BookingVilla.DTOs
{
    public class CreateBookingRequest
    {
        [Required]
        public int VillaId { get; set; }

        [Required]
        public DateTime CheckinDate { get; set; }

        [Required]
        public DateTime CheckoutDate { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Amount of people must be at least 1")]
        public int AmountOfPeople { get; set; }

        public List<int>? ServiceIds { get; set; }
    }
}

