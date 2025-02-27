using System;
using System.Collections.Generic;

namespace ForecastingTeller.API.Models
{
    public class TarotCard
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public bool IsReversed { get; set; }
        public string Position { get; set; } // Position in the spread
    }

    public class TarotReading
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User User { get; set; }
        public string Question { get; set; }
        public string SpreadType { get; set; } // Three Card, Celtic Cross, etc.
        public List<TarotCard> Cards { get; set; } = new List<TarotCard>();
        public string Interpretation { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsFavorite { get; set; } = false;
    }
}