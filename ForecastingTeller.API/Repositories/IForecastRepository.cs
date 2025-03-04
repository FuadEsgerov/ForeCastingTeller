using ForecastingTeller.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ForecastingTeller.API.Repositories
{
    public interface IForecastRepository
    {
        Task<Forecast> GetByIdAsync(Guid id);
        Task<IEnumerable<Forecast>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<int> GetCountByUserIdAsync(Guid userId);
        Task<IEnumerable<Forecast>> GetByUserIdAndCategoryAsync(Guid userId, string category, int page = 1, int pageSize = 10);
        Task<int> GetCountByUserIdAndCategoryAsync(Guid userId, string category);
        Task<IEnumerable<Forecast>> GetFavoritesByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<IEnumerable<Forecast>> GetActiveByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<Forecast> CreateAsync(Forecast forecast);
        Task<Forecast> UpdateAsync(Forecast forecast);
        Task<bool> DeleteAsync(Guid id);
    }
}