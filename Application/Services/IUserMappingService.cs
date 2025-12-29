using Domain.Models;
using Application.DTOs;

namespace Application.Services
{
    public interface IUserMappingService
    {
        ApplicationUser MapToIdentityUser(User domainUser);
        User MapToDomainUser(ApplicationUser identityUser);
    }

    public class UserMappingService : IUserMappingService
    {
        public ApplicationUser MapToIdentityUser(User domainUser)
        {
            return new ApplicationUser
            {
                UserName = domainUser.Username,
                Email = domainUser.Email,
                EmailConfirmed = true,
                CreatedAt = domainUser.CreatedAt,
                LastLoginAt = domainUser.LastLoginAt,
                // Map fingerprint data if needed
                // AdditionalIdentityProperties = domainUser.SomeProperty
            };
        }

        public User MapToDomainUser(ApplicationUser identityUser)
        {
            return new User
            {
                Username = identityUser.UserName,
                Email = identityUser.Email,
                PasswordHash = "managed_by_identity", // Identity handles this
                IsActive = true,
                CreatedAt = identityUser.CreatedAt,
                LastLoginAt = identityUser.LastLoginAt
            };
        }
    }
}