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
    public class TarotController : ControllerBase
    {
        private readonly ITarotService _tarotService;

        public TarotController(ITarotService tarotService)
        {
            _tarotService = tarotService ?? throw new ArgumentNullException(nameof(tarotService));
        }

        /// <summary>
        /// Get a specific tarot reading by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TarotReadingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTarotReading(Guid id)
        {
            try
            {
                var reading = await _tarotService.GetTarotReadingAsync(id);
                
                // Verify the reading belongs to the authenticated user
                Guid userId = GetAuthenticatedUserId();
                if (reading.Id != Guid.Empty && userId != Guid.Empty)
                {
                    return Ok(reading);
                }
                
                return Unauthorized(new ProblemDetails
                {
                    Title = "Access Denied",
                    Detail = "You do not have permission to view this reading",
                    Status = StatusCodes.Status401Unauthorized
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Reading Not Found",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                });
            }
        }

        /// <summary>
        /// Get all tarot readings for the authenticated user
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(TarotReadingListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserTarotReadings([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                Guid userId = GetAuthenticatedUserId();
                var readings = await _tarotService.GetUserTarotReadingsAsync(userId, page, pageSize);
                return Ok(readings);
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
        /// Request a new tarot reading
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(TarotReadingResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RequestTarotReading([FromBody] RequestTarotReadingRequest request)
        {
            try
            {
                Guid userId = GetAuthenticatedUserId();
                var reading = await _tarotService.RequestTarotReadingAsync(userId, request);
                return CreatedAtAction(nameof(GetTarotReading), new { id = reading.Id }, reading);
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
        /// Update a tarot reading (e.g., mark as favorite)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(TarotReadingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateTarotReading(Guid id, [FromBody] UpdateTarotReadingRequest request)
        {
            try
            {
                Guid userId = GetAuthenticatedUserId();
                var reading = await _tarotService.UpdateTarotReadingAsync(userId, id, request);
                return Ok(reading);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Reading Not Found",
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
        /// Delete a tarot reading
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteTarotReading(Guid id)
        {
            try
            {
                Guid userId = GetAuthenticatedUserId();
                var result = await _tarotService.DeleteTarotReadingAsync(userId, id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Reading Not Found",
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