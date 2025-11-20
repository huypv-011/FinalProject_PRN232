namespace BookingVilla.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public UserInfo UserInfo { get; set; } = null!;
        public string Message { get; set; } = "Login successful";
    }
}

