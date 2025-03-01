using ForecastingTeller.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ForecastingTeller.API.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(Guid id);
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByUsernameAsync(string username);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByUsernameAsync(string username);
        Task<bool> ValidateCredentialsAsync(string email, string passwordHash);
        Task<UserProfile> GetUserProfileAsync(Guid userId);
        Task<UserProfile> UpdateUserProfileAsync(UserProfile profile);
        Task<User> GetByEmailVerificationTokenAsync(string token);
    }
}