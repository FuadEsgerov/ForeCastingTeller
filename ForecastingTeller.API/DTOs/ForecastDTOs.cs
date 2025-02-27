using System.ComponentModel.DataAnnotations;
using ForecastingTeller.API.Models;

namespace ForecastingTeller.API.DTOs
{
    public class ForecastResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public ForecastType Type { get; set; }
        public string Category { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ForecastDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsFavorite { get; set; }
        public double Accuracy { get; set; }
    }

    public class RequestForecastRequest
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public ForecastType Type { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; }

        [Required]
        public DateTime ForecastDate { get; set; }
    }

    public class UpdateForecastRequest
    {
        public bool IsFavorite { get; set; }
        public double? Accuracy { get; set; }
    }

    public class ForecastListResponse
    {
        public IEnumerable<ForecastResponse> Forecasts { get; set; }
        public int TotalCount { get; set; }
    }
}