namespace BookingVilla.DTOs
{
    public class UserInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Avatar { get; set; }
        public string Role { get; set; } = null!;
        public double? Salary { get; set; } // Only for employees
        public string UserType { get; set; } = null!; // "Customer" or "Employee"
    }
}

