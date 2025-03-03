using ForecastingTeller.API.DTOs;
using ForecastingTeller.API.Models;
using ForecastingTeller.API.Repositories;

namespace ForecastingTeller.API.Services
{
    public interface ITarotService
    {
        Task<TarotReadingResponse> GetTarotReadingAsync(Guid readingId);
        Task<TarotReadingListResponse> GetUserTarotReadingsAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<TarotReadingResponse> RequestTarotReadingAsync(Guid userId, RequestTarotReadingRequest request);
        Task<TarotReadingResponse> UpdateTarotReadingAsync(Guid userId, Guid readingId, UpdateTarotReadingRequest request);
        Task<bool> DeleteTarotReadingAsync(Guid userId, Guid readingId);
    }

    public class TarotService : ITarotService
    {
        private readonly ITarotRepository _tarotRepository;
        private readonly IUserRepository _userRepository;
        private  Dictionary<string, List<string>> _tarotCards;
        

        public TarotService(ITarotRepository tarotRepository, IUserRepository userRepository)
        {
            _tarotRepository = tarotRepository ?? throw new ArgumentNullException(nameof(tarotRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            
            // Initialize tarot card data
            InitializeTarotCardData();
        }

        private void InitializeTarotCardData()
        {
            // This is a simplified version - in a real application, this would be more comprehensive
            // and likely come from a database or external service
            _tarotCards = new Dictionary<string, List<string>>
            {
                { "Major Arcana", new List<string> {
                    "The Fool", "The Magician", "The High Priestess", "The Empress", "The Emperor",
                    "The Hierophant", "The Lovers", "The Chariot", "Strength", "The Hermit",
                    "Wheel of Fortune", "Justice", "The Hanged Man", "Death", "Temperance",
                    "The Devil", "The Tower", "The Star", "The Moon", "The Sun",
                    "Judgement", "The World"
                }},
                { "Wands", new List<string> {
                    "Ace of Wands", "Two of Wands", "Three of Wands", "Four of Wands", "Five of Wands",
                    "Six of Wands", "Seven of Wands", "Eight of Wands", "Nine of Wands", "Ten of Wands",
                    "Page of Wands", "Knight of Wands", "Queen of Wands", "King of Wands"
                }},
                { "Cups", new List<string> {
                    "Ace of Cups", "Two of Cups", "Three of Cups", "Four of Cups", "Five of Cups",
                    "Six of Cups", "Seven of Cups", "Eight of Cups", "Nine of Cups", "Ten of Cups",
                    "Page of Cups", "Knight of Cups", "Queen of Cups", "King of Cups"
                }},
                { "Swords", new List<string> {
                    "Ace of Swords", "Two of Swords", "Three of Swords", "Four of Swords", "Five of Swords",
                    "Six of Swords", "Seven of Swords", "Eight of Swords", "Nine of Swords", "Ten of Swords",
                    "Page of Swords", "Knight of Swords", "Queen of Swords", "King of Swords"
                }},
                { "Pentacles", new List<string> {
                    "Ace of Pentacles", "Two of Pentacles", "Three of Pentacles", "Four of Pentacles", "Five of Pentacles",
                    "Six of Pentacles", "Seven of Pentacles", "Eight of Pentacles", "Nine of Pentacles", "Ten of Pentacles",
                    "Page of Pentacles", "Knight of Pentacles", "Queen of Pentacles", "King of Pentacles"
                }}
            };
        }

        public async Task<TarotReadingResponse> GetTarotReadingAsync(Guid readingId)
        {
            var reading = await _tarotRepository.GetByIdAsync(readingId);
            if (reading == null)
            {
                throw new KeyNotFoundException($"Tarot reading with ID {readingId} not found");
            }

            return MapToTarotReadingResponse(reading);
        }

        public async Task<TarotReadingListResponse> GetUserTarotReadingsAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            var readings = await _tarotRepository.GetByUserIdAsync(userId, page, pageSize);
            var totalCount = await _tarotRepository.GetCountByUserIdAsync(userId);

            return new TarotReadingListResponse
            {
                TarotReadings = readings.Select(MapToTarotReadingResponse),
                TotalCount = totalCount
            };
        }

        public async Task<TarotReadingResponse> RequestTarotReadingAsync(Guid userId, RequestTarotReadingRequest request)
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            // Create a new tarot reading
            var reading = new TarotReading
            {
                UserId = userId,
                Question = request.Question,
                SpreadType = request.SpreadType,
                Cards = DrawCards(request.SpreadType),
                Interpretation = GenerateInterpretation(request.Question, request.SpreadType)
            };

            // Save the reading
            var createdReading = await _tarotRepository.CreateAsync(reading);

            return MapToTarotReadingResponse(createdReading);
        }

        public async Task<TarotReadingResponse> UpdateTarotReadingAsync(Guid userId, Guid readingId, UpdateTarotReadingRequest request)
        {
            // Get the reading
            var reading = await _tarotRepository.GetByIdAsync(readingId);
            if (reading == null)
            {
                throw new KeyNotFoundException($"Tarot reading with ID {readingId} not found");
            }

            // Verify the reading belongs to the user
            if (reading.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this reading");
            }

            // Update the reading
            reading.IsFavorite = request.IsFavorite;
            
            // Save the changes
            var updatedReading = await _tarotRepository.UpdateAsync(reading);

            return MapToTarotReadingResponse(updatedReading);
        }

        public async Task<bool> DeleteTarotReadingAsync(Guid userId, Guid readingId)
        {
            // Get the reading
            var reading = await _tarotRepository.GetByIdAsync(readingId);
            if (reading == null)
            {
                throw new KeyNotFoundException($"Tarot reading with ID {readingId} not found");
            }

            // Verify the reading belongs to the user
            if (reading.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this reading");
            }

            // Delete the reading
            return await _tarotRepository.DeleteAsync(readingId);
        }

        private List<TarotCard> DrawCards(string spreadType)
        {
            // Determine how many cards to draw based on spread type
            int cardCount = GetCardCountForSpread(spreadType);
            
            // Create a list to hold all available cards
            var allCards = new List<string>();
            foreach (var suit in _tarotCards.Keys)
            {
                allCards.AddRange(_tarotCards[suit]);
            }
            
            // Shuffle the cards
            var random = new Random();
            allCards = allCards.OrderBy(c => random.Next()).ToList();
            
            // Draw the required number of cards
            var drawnCards = new List<TarotCard>();
            for (int i = 0; i < cardCount; i++)
            {
                var isReversed = random.Next(2) == 0; // 50% chance of being reversed
                var position = GetPositionForSpread(spreadType, i);
                
                drawnCards.Add(new TarotCard
                {
                    Name = allCards[i],
                    ImageUrl = GetCardImageUrl(allCards[i]),
                    IsReversed = isReversed,
                    Position = position
                });
            }
            
            return drawnCards;
        }

        private int GetCardCountForSpread(string spreadType)
        {
            // Return the number of cards based on spread type
            return spreadType.ToLower() switch
            {
                "single card" => 1,
                "three card" => 3,
                "celtic cross" => 10,
                "horse shoe" => 7,
                "relationship" => 5,
                _ => 3 // Default to three card spread
            };
        }

        private string GetPositionForSpread(string spreadType, int index)
        {
            // Return position name based on spread type and index
            return spreadType.ToLower() switch
            {
                "single card" => "Present",
                "three card" => index switch
                {
                    0 => "Past",
                    1 => "Present",
                    2 => "Future",
                    _ => $"Position {index + 1}"
                },
                "celtic cross" => index switch
                {
                    0 => "The Present",
                    1 => "The Challenge",
                    2 => "The Past",
                    3 => "The Future",
                    4 => "Above",
                    5 => "Below",
                    6 => "Advice",
                    7 => "External Influences",
                    8 => "Hopes and Fears",
                    9 => "Outcome",
                    _ => $"Position {index + 1}"
                },
                "relationship" => index switch
                {
                    0 => "You",
                    1 => "Your Partner",
                    2 => "The Relationship",
                    3 => "Challenges",
                    4 => "Outcome",
                    _ => $"Position {index + 1}"
                },
                _ => $"Position {index + 1}" // Default positioning
            };
        }

        private string GetCardImageUrl(string cardName)
        {
            // This is a placeholder - in a real application, this would return actual image URLs
            // Format the card name for the URL (lowercase, replace spaces with hyphens)
            var formattedName = cardName.ToLower().Replace(" ", "-").Replace("of-", "");
            return $"/images/tarot-cards/{formattedName}.jpg";
        }

        private string GenerateInterpretation(string question, string spreadType)
        {
            // This is a placeholder - in a real application, this would generate a meaningful interpretation
            // based on the cards drawn, the question asked, and potentially use AI for deeper insights
            return $"Interpretation for your question: \"{question}\" using a {spreadType} spread. " +
                   "The cards reveal that you are at a crossroads in your journey. The path ahead may seem uncertain, " +
                   "but trust in your intuition to guide you. The challenges you face are temporary, and with patience " +
                   "and perseverance, you will overcome them. Remember that change is a natural part of life, and " +
                   "embracing it will lead to growth and new opportunities.";
        }

        private TarotReadingResponse MapToTarotReadingResponse(TarotReading reading)
        {
            return new TarotReadingResponse
            {
                Id = reading.Id,
                Question = reading.Question,
                SpreadType = reading.SpreadType,
                Cards = reading.Cards.Select(c => new TarotCardResponse
                {
                    Name = c.Name,
                    ImageUrl = c.ImageUrl,
                    IsReversed = c.IsReversed,
                    Position = c.Position,
                    Meaning = GetCardMeaning(c.Name, c.IsReversed)
                }).ToList(),
                Interpretation = reading.Interpretation,
                CreatedAt = reading.CreatedAt,
                IsFavorite = reading.IsFavorite
            };
        }

        private string GetCardMeaning(string cardName, bool isReversed)
        {
            // This is a placeholder - in a real application, this would return actual card meanings
            // based on whether the card is upright or reversed
            if (isReversed)
            {
                return $"Reversed {cardName} typically represents challenges, obstacles, or the negative aspects of the card's energy.";
            }
            else
            {
                return $"Upright {cardName} typically represents positive energy, opportunities, and the beneficial aspects of the card's symbolism.";
            }
        }
    }
}