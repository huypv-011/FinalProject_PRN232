using BookingVilla.DTOs;
using BussinessObject;

namespace BookingVilla.Services
{
    public interface IJwtService
    {
        string GenerateToken(Account account, UserInfo userInfo);
        bool ValidateToken(string token);
    }
}

