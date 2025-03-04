using ForecastingTeller.API.Data;
using ForecastingTeller.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ForecastingTeller.API.Repositories
{
    public class ForecastRepository : IForecastRepository
    {
        private readonly ApplicationDbContext _context;

        public ForecastRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Forecast> GetByIdAsync(Guid id)
        {
            return await _context.Forecasts
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<Forecast>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            // Calculate skip count
            int skipCount = (page - 1) * pageSize;

            return await _context.Forecasts
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Skip(skipCount)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCountByUserIdAsync(Guid userId)
        {
            return await _context.Forecasts
                .Where(f => f.UserId == userId)
                .CountAsync();
        }

        public async Task<IEnumerable<Forecast>> GetByUserIdAndCategoryAsync(Guid userId, string category, int page = 1, int pageSize = 10)
        {
            // Calculate skip count
            int skipCount = (page - 1) * pageSize;

            return await _context.Forecasts
                .Where(f => f.UserId == userId && f.Category.ToLower() == category.ToLower())
                .OrderByDescending(f => f.CreatedAt)
                .Skip(skipCount)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCountByUserIdAndCategoryAsync(Guid userId, string category)
        {
            return await _context.Forecasts
                .Where(f => f.UserId == userId && f.Category.ToLower() == category.ToLower())
                .CountAsync();
        }

        public async Task<IEnumerable<Forecast>> GetFavoritesByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            // Calculate skip count
            int skipCount = (page - 1) * pageSize;

            return await _context.Forecasts
                .Where(f => f.UserId == userId && f.IsFavorite)
                .OrderByDescending(f => f.CreatedAt)
                .Skip(skipCount)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Forecast>> GetActiveByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            // Calculate skip count
            int skipCount = (page - 1) * pageSize;
            var currentDate = DateTime.UtcNow;

            return await _context.Forecasts
                .Where(f => f.UserId == userId && f.ExpiryDate >= currentDate)
                .OrderBy(f => f.ExpiryDate) // Soonest to expire first
                .Skip(skipCount)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Forecast> CreateAsync(Forecast forecast)
        {
            await _context.Forecasts.AddAsync(forecast);
            await _context.SaveChangesAsync();
            return forecast;
        }

        public async Task<Forecast> UpdateAsync(Forecast forecast)
        {
            _context.Forecasts.Update(forecast);
            await _context.SaveChangesAsync();
            return forecast;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var forecast = await _context.Forecasts.FindAsync(id);
            if (forecast == null)
            {
                return false;
            }

            _context.Forecasts.Remove(forecast);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}