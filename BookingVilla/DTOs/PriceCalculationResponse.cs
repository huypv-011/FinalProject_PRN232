namespace BookingVilla.DTOs
{
    public class PriceCalculationResponse
    {
        public int VillaId { get; set; }
        public string VillaName { get; set; } = null!;
        public double BasePrice { get; set; }
        public double TotalPrice { get; set; }
        public int Days { get; set; }
        public double PricePerDay { get; set; }
        public List<ServiceDto> AvailableServices { get; set; } = new();
    }

    public class ServiceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public double Price { get; set; }
    }
}

