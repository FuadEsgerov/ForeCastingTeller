// Services/AuthService.cs
using ForecastingTeller.API.DTOs;
using ForecastingTeller.API.Infrastructure;
using ForecastingTeller.API.Models;
using ForecastingTeller.API.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace ForecastingTeller.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
        Task<bool> VerifyEmailAsync(string token);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            IConfiguration configuration)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // Validate that email doesn't exist
            if (await _userRepository.ExistsByEmailAsync(request.Email))
            {
                throw new InvalidOperationException("Email is already in use");
            }

            // Validate that username doesn't exist
            if (await _userRepository.ExistsByUsernameAsync(request.Username))
            {
                throw new InvalidOperationException("Username is already in use");
            }

            // Hash the password
            string salt;
            string passwordHash = _passwordHasher.HashPassword(request.Password, out salt);

            // Create the user
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                Salt = salt,
                EmailVerificationToken = Guid.NewGuid().ToString("N")
            };

            // Set up the user profile
            user.Profile = new UserProfile
            {
                UserId = user.Id
            };

            // Save the user to the database
            var createdUser = await _userRepository.CreateAsync(user);

            // TODO: Send verification email if required
            bool requireEmailVerification = bool.Parse(_configuration["EmailVerification:RequireEmailVerification"] ?? "false");
            if (requireEmailVerification)
            {
                // Send email with verification token
                // This would typically be done via an email service
            }

            // Generate JWT token
            DateTime tokenExpiration;
            string token = _jwtTokenGenerator.GenerateToken(createdUser, out tokenExpiration);

            // Return the response
            return new AuthResponse
            {
                UserId = createdUser.Id,
                Username = createdUser.Username,
                Email = createdUser.Email,
                Token = token,
                TokenExpiration = tokenExpiration
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            // Get the user by email
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Validate the password
            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.Salt))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Check if email verification is required
            bool requireEmailVerification = bool.Parse(_configuration["EmailVerification:RequireEmailVerification"] ?? "false");
            if (requireEmailVerification && !user.IsEmailVerified)
            {
                throw new UnauthorizedAccessException("Email not verified");
            }

            // Update last login time
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Generate JWT token
            DateTime tokenExpiration;
            string token = _jwtTokenGenerator.GenerateToken(user, out tokenExpiration);

            // Return the response
            return new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Token = token,
                TokenExpiration = tokenExpiration
            };
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            // Get the user by email
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                // We return true even if the user doesn't exist for security reasons
                return true;
            }

            // Generate password reset token
            string resetToken = Guid.NewGuid().ToString("N");
            user.PasswordResetToken = resetToken;
            user.PasswordResetExpiry = DateTime.UtcNow.AddHours(24); // Token expires in 24 hours
            await _userRepository.UpdateAsync(user);

            // TODO: Send password reset email
            // This would typically be done via an email service

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            // Get the user by email
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || user.PasswordResetToken != request.Token)
            {
                throw new InvalidOperationException("Invalid token");
            }

            // Check if token has expired
            if (user.PasswordResetExpiry < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Token has expired");
            }

            // Hash the new password
            string salt;
            string passwordHash = _passwordHasher.HashPassword(request.NewPassword, out salt);

            // Update user password
            user.PasswordHash = passwordHash;
            user.Salt = salt;
            user.PasswordResetToken = null;
            user.PasswordResetExpiry = null;
            await _userRepository.UpdateAsync(user);

            return true;
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            // Find user by verification token
            var users = await _userRepository.GetByEmailVerificationTokenAsync(token);
            if (users == null)
            {
                throw new InvalidOperationException("Invalid token");
            }

            // Mark email as verified
            users.IsEmailVerified = true;
            users.EmailVerificationToken = null;
            await _userRepository.UpdateAsync(users);

            return true;
        }
    }
}