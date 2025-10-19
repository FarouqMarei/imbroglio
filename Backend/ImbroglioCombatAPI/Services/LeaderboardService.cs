using Microsoft.EntityFrameworkCore;
using ImbroglioCombatAPI.Data;
using ImbroglioCombatAPI.Models;

namespace ImbroglioCombatAPI.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly GameDbContext _context;

        public LeaderboardService(GameDbContext context)
        {
            _context = context;
        }

        public async Task<LeaderboardEntry> AddEntry(LeaderboardEntry entry)
        {
            entry.CompletedAt = DateTime.UtcNow;
            _context.LeaderboardEntries.Add(entry);
            await _context.SaveChangesAsync();
            return entry;
        }

        public async Task<List<LeaderboardEntry>> GetTopScores(int count = 10)
        {
            return await _context.LeaderboardEntries
                .OrderByDescending(e => e.Score)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<LeaderboardEntry>> GetFastestCompletions(int count = 10)
        {
            return await _context.LeaderboardEntries
                .OrderBy(e => e.CompletionTime)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetPlayerRank(string playerName)
        {
            var allEntries = await _context.LeaderboardEntries
                .OrderByDescending(e => e.Score)
                .ToListAsync();

            var entry = allEntries.FirstOrDefault(e => e.PlayerName == playerName);
            
            if (entry == null)
            {
                return -1;
            }

            return allEntries.IndexOf(entry) + 1;
        }
    }
}

