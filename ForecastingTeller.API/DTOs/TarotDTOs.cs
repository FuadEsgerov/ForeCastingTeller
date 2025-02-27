using System.ComponentModel.DataAnnotations;

namespace ForecastingTeller.API.DTOs
{
    public class TarotCardResponse
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public bool IsReversed { get; set; }
        public string Position { get; set; }
        public string Meaning { get; set; }
    }

    public class TarotReadingResponse
    {
        public Guid Id { get; set; }
        public string Question { get; set; }
        public string SpreadType { get; set; }
        public List<TarotCardResponse> Cards { get; set; }
        public string Interpretation { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsFavorite { get; set; }
    }

    public class RequestTarotReadingRequest
    {
        [Required]
        [StringLength(200)]
        public string Question { get; set; }

        [Required]
        [StringLength(50)]
        public string SpreadType { get; set; }
    }

    public class UpdateTarotReadingRequest
    {
        public bool IsFavorite { get; set; }
    }

    public class TarotReadingListResponse
    {
        public IEnumerable<TarotReadingResponse> TarotReadings { get; set; }
        public int TotalCount { get; set; }
    }
}