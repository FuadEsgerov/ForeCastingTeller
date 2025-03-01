// ForecastingTeller.Tests/UnitTests/Controllers/AuthControllerTests.cs
using ForecastingTeller.API.Controllers;
using ForecastingTeller.API.DTOs;
using ForecastingTeller.API.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ForecastingTeller.Tests.UnitTests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(_mockAuthService.Object);
        }

        [Fact]
        public async Task Register_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var authResponse = new AuthResponse
            {
                UserId = Guid.NewGuid(),
                Username = registerRequest.Username,
                Email = registerRequest.Email,
                Token = "jwt-token",
                TokenExpiration = DateTime.UtcNow.AddHours(1)
            };

            _mockAuthService.Setup(service => service.RegisterAsync(registerRequest))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Register(registerRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<AuthResponse>(okResult.Value);
            Assert.Equal(authResponse.UserId, returnValue.UserId);
            Assert.Equal(authResponse.Username, returnValue.Username);
            Assert.Equal(authResponse.Email, returnValue.Email);
            Assert.Equal(authResponse.Token, returnValue.Token);
            
            _mockAuthService.Verify(service => service.RegisterAsync(registerRequest), Times.Once);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsConflictResult()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            _mockAuthService.Setup(service => service.RegisterAsync(registerRequest))
                .ThrowsAsync(new InvalidOperationException("Email is already in use"));

            // Act
            var result = await _controller.Register(registerRequest);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var problemDetails = Assert.IsType<ProblemDetails>(conflictResult.Value);
            Assert.Equal("Registration Failed", problemDetails.Title);
            Assert.Equal("Email is already in use", problemDetails.Detail);
            
            _mockAuthService.Verify(service => service.RegisterAsync(registerRequest), Times.Once);
        }

        [Fact]
        public async Task Register_DuplicateUsername_ReturnsConflictResult()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            _mockAuthService.Setup(service => service.RegisterAsync(registerRequest))
                .ThrowsAsync(new InvalidOperationException("Username is already in use"));

            // Act
            var result = await _controller.Register(registerRequest);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var problemDetails = Assert.IsType<ProblemDetails>(conflictResult.Value);
            Assert.Equal("Registration Failed", problemDetails.Title);
            Assert.Equal("Username is already in use", problemDetails.Detail);
            
            _mockAuthService.Verify(service => service.RegisterAsync(registerRequest), Times.Once);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var authResponse = new AuthResponse
            {
                UserId = Guid.NewGuid(),
                Username = "testuser",
                Email = loginRequest.Email,
                Token = "jwt-token",
                TokenExpiration = DateTime.UtcNow.AddHours(1)
            };

            _mockAuthService.Setup(service => service.LoginAsync(loginRequest))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<AuthResponse>(okResult.Value);
            Assert.Equal(authResponse.UserId, returnValue.UserId);
            Assert.Equal(authResponse.Username, returnValue.Username);
            Assert.Equal(authResponse.Email, returnValue.Email);
            Assert.Equal(authResponse.Token, returnValue.Token);
            
            _mockAuthService.Verify(service => service.LoginAsync(loginRequest), Times.Once);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorizedResult()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            _mockAuthService.Setup(service => service.LoginAsync(loginRequest))
                .ThrowsAsync(new UnauthorizedAccessException("Invalid email or password"));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var problemDetails = Assert.IsType<ProblemDetails>(unauthorizedResult.Value);
            Assert.Equal("Login Failed", problemDetails.Title);
            Assert.Equal("Invalid email or password", problemDetails.Detail);
            
            _mockAuthService.Verify(service => service.LoginAsync(loginRequest), Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_ValidEmail_ReturnsOkResult()
        {
            // Arrange
            var forgotPasswordRequest = new ForgotPasswordRequest
            {
                Email = "test@example.com"
            };

            _mockAuthService.Setup(service => service.ForgotPasswordAsync(forgotPasswordRequest))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.ForgotPassword(forgotPasswordRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<bool>(okResult.Value);
            Assert.True(returnValue);
            
            _mockAuthService.Verify(service => service.ForgotPasswordAsync(forgotPasswordRequest), Times.Once);
        }

        [Fact]
        public async Task ResetPassword_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var resetPasswordRequest = new ResetPasswordRequest
            {
                Email = "test@example.com",
                Token = "reset-token",
                NewPassword = "NewPassword123!",
                ConfirmNewPassword = "NewPassword123!"
            };

            _mockAuthService.Setup(service => service.ResetPasswordAsync(resetPasswordRequest))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.ResetPassword(resetPasswordRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<bool>(okResult.Value);
            Assert.True(returnValue);
            
            _mockAuthService.Verify(service => service.ResetPasswordAsync(resetPasswordRequest), Times.Once);
        }

        [Fact]
        public async Task ResetPassword_InvalidToken_ReturnsBadRequestResult()
        {
            // Arrange
            var resetPasswordRequest = new ResetPasswordRequest
            {
                Email = "test@example.com",
                Token = "invalid-token",
                NewPassword = "NewPassword123!",
                ConfirmNewPassword = "NewPassword123!"
            };

            _mockAuthService.Setup(service => service.ResetPasswordAsync(resetPasswordRequest))
                .ThrowsAsync(new InvalidOperationException("Invalid token"));

            // Act
            var result = await _controller.ResetPassword(resetPasswordRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
            Assert.Equal("Password Reset Failed", problemDetails.Title);
            Assert.Equal("Invalid token", problemDetails.Detail);
            
            _mockAuthService.Verify(service => service.ResetPasswordAsync(resetPasswordRequest), Times.Once);
        }

        [Fact]
        public async Task VerifyEmail_ValidToken_ReturnsOkResult()
        {
            // Arrange
            var token = "valid-token";

            _mockAuthService.Setup(service => service.VerifyEmailAsync(token))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.VerifyEmail(token);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<bool>(okResult.Value);
            Assert.True(returnValue);
            
            _mockAuthService.Verify(service => service.VerifyEmailAsync(token), Times.Once);
        }

        [Fact]
        public async Task VerifyEmail_InvalidToken_ReturnsBadRequestResult()
        {
            // Arrange
            var token = "invalid-token";

            _mockAuthService.Setup(service => service.VerifyEmailAsync(token))
                .ThrowsAsync(new InvalidOperationException("Invalid token"));

            // Act
            var result = await _controller.VerifyEmail(token);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
            Assert.Equal("Email Verification Failed", problemDetails.Title);
            Assert.Equal("Invalid token", problemDetails.Detail);
            
            _mockAuthService.Verify(service => service.VerifyEmailAsync(token), Times.Once);
        }
    }
}