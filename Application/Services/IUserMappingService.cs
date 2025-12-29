using Application.DTOs; // ✅ Use your existing DTOs
using Domain.Models;

namespace Application.Services
{
    public interface IUserMappingService
    {
        UserDto MapToDto(User domainUser); // ✅ Use your UserDTO
        User MapToDomain(UserCreateDto dto);
        UserCreateDto MapToCreateDto(string username, string email, string PhoneNumber, string password);
    }

    public class UserMappingService : IUserMappingService
    {
        public UserDto MapToDto(User domainUser)
        {
            return new UserDto
            {
                Id = domainUser.Id.ToString(),
                UserName = domainUser.Username,
                Email = domainUser.Email,
                EmailConfirmed = true, // Domain users are confirmed by default
                PhoneNumber = domainUser.PhoneNumber ?? "",
                PhoneNumberConfirmed = !string.IsNullOrEmpty(domainUser.PhoneNumber),
                CreatedAt = domainUser.CreatedAt,
                LastLoginAt = domainUser.LastLoginAt,
                HasFingerprintEnrolled = domainUser.HasFingerprintEnrolled
            };
        }

        public User MapToDomain(UserCreateDto dto)
        {
            return new User
            {
                Username = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = dto.Password, // This should be hashed before saving
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                HasFingerprintEnrolled = false
            };
        }

        public UserCreateDto MapToCreateDto(string username, string email, string phonenumber, string password)
        {
            return new UserCreateDto
            {
                UserName = username,
                Email = email,
                PhoneNumber = phonenumber,
                Password = password
            };
        }
    }
}