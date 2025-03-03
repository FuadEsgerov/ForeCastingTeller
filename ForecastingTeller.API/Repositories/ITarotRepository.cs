using ForecastingTeller.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ForecastingTeller.API.Repositories
{
    public interface ITarotRepository
    {
        Task<TarotReading> GetByIdAsync(Guid id);
        Task<IEnumerable<TarotReading>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<int> GetCountByUserIdAsync(Guid userId);
        Task<IEnumerable<TarotReading>> GetFavoritesByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<TarotReading> CreateAsync(TarotReading reading);
        Task<TarotReading> UpdateAsync(TarotReading reading);
        Task<bool> DeleteAsync(Guid id);
    }
}