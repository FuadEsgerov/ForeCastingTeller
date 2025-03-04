using ForecastingTeller.API.DTOs;
using ForecastingTeller.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ForecastingTeller.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;

        public UserProfileController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService ?? throw new ArgumentNullException(nameof(userProfileService));
        }

        /// <summary>
        /// Get the authenticated user's profile
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                Guid userId = GetAuthenticatedUserId();
                var profile = await _userProfileService.GetUserProfileAsync(userId);
                return Ok(profile);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "User Not Found",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                });
            }
        }

        /// <summary>
        /// Update the authenticated user's profile
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                Guid userId = GetAuthenticatedUserId();
                var profile = await _userProfileService.UpdateUserProfileAsync(userId, request);
                return Ok(profile);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "User Not Found",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                });
            }
        }

        /// <summary>
        /// Update the authenticated user's preferences
        /// </summary>
        [HttpPut("preferences")]
        [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUserPreferences([FromBody] UserPreferencesRequest request)
        {
            try
            {
                Guid userId = GetAuthenticatedUserId();
                var profile = await _userProfileService.UpdateUserPreferencesAsync(userId, request);
                return Ok(profile);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "User Not Found",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                });
            }
        }

        /// <summary>
        /// Helper method to get the authenticated user's ID from claims
        /// </summary>
        private Guid GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }
            return Guid.Empty;
        }
    }
}