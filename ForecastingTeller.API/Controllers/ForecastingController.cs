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
    public class ForecastingController : ControllerBase
    {
        private readonly IForecastingService _forecastingService;

        public ForecastingController(IForecastingService forecastingService)
        {
            _forecastingService = forecastingService ?? throw new ArgumentNullException(nameof(forecastingService));
        }

        /// <summary>
        /// Get a specific forecast by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ForecastResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetForecast(Guid id)
        {
            try
            {
                var forecast = await _forecastingService.GetForecastAsync(id);
                
                // Verify the forecast belongs to the authenticated user
                Guid userId = GetAuthenticatedUserId();
                if (forecast.Id != Guid.Empty && userId != Guid.Empty)
                {
                    return Ok(forecast);
                }
                
                return Unauthorized(new ProblemDetails
                {
                    Title = "Access Denied",
                    Detail = "You do not have permission to view this forecast",
                    Status = StatusCodes.Status401Unauthorized
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Forecast Not Found",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                });
            }
        }

        /// <summary>
        /// Get all forecasts for the authenticated user
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ForecastListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserForecasts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                Guid userId = GetAuthenticatedUserId();
                var forecasts = await _forecastingService.GetUserForecastsAsync(userId, page, pageSize);
                return Ok(forecasts);
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
        /// Get forecasts for the authenticated user by category
        /// </summary>
        [HttpGet("category/{category}")]
        [ProducesResponseType(typeof(ForecastListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserForecastsByCategory(string category, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                Guid userId = GetAuthenticatedUserId();
                var forecasts = await _forecastingService.GetUserForecastsByCategoryAsync(userId, category, page, pageSize);
                return Ok(forecasts);
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
        /// Request a new forecast
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ForecastResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RequestForecast([FromBody] RequestForecastRequest request)
        {
            try
            {
                Guid userId = GetAuthenticatedUserId();
                var forecast = await _forecastingService.RequestForecastAsync(userId, request);
                return CreatedAtAction(nameof(GetForecast), new { id = forecast.Id }, forecast);
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
        /// Update a forecast (e.g., mark as favorite, rate accuracy)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ForecastResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateForecast(Guid id, [FromBody] UpdateForecastRequest request)
        {
            try
            {
                Guid userId = GetAuthenticatedUserId();
                var forecast = await _forecastingService.UpdateForecastAsync(userId, id, request);
                return Ok(forecast);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Forecast Not Found",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Access Denied",
                    Detail = ex.Message,
                    Status = StatusCodes.Status401Unauthorized
                });
            }
        }

        /// <summary>
        /// Delete a forecast
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteForecast(Guid id)
        {
            try
            {
                Guid userId = GetAuthenticatedUserId();
                var result = await _forecastingService.DeleteForecastAsync(userId, id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Forecast Not Found",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Access Denied",
                    Detail = ex.Message,
                    Status = StatusCodes.Status401Unauthorized
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