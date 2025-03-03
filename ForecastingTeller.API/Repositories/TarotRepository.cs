using ForecastingTeller.API.Data;
using ForecastingTeller.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ForecastingTeller.API.Repositories
{
    public class TarotRepository : ITarotRepository
    {
        private readonly ApplicationDbContext _context;

        public TarotRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<TarotReading> GetByIdAsync(Guid id)
        {
            return await _context.TarotReadings
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<TarotReading>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            // Calculate skip count
            int skipCount = (page - 1) * pageSize;

            return await _context.TarotReadings
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skipCount)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCountByUserIdAsync(Guid userId)
        {
            return await _context.TarotReadings
                .Where(r => r.UserId == userId)
                .CountAsync();
        }

        public async Task<IEnumerable<TarotReading>> GetFavoritesByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            // Calculate skip count
            int skipCount = (page - 1) * pageSize;

            return await _context.TarotReadings
                .Where(r => r.UserId == userId && r.IsFavorite)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skipCount)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<TarotReading> CreateAsync(TarotReading reading)
        {
            await _context.TarotReadings.AddAsync(reading);
            await _context.SaveChangesAsync();
            return reading;
        }

        public async Task<TarotReading> UpdateAsync(TarotReading reading)
        {
            _context.TarotReadings.Update(reading);
            await _context.SaveChangesAsync();
            return reading;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var reading = await _context.TarotReadings.FindAsync(id);
            if (reading == null)
            {
                return false;
            }

            _context.TarotReadings.Remove(reading);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}