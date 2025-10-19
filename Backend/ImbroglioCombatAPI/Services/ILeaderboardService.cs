using ImbroglioCombatAPI.Models;

namespace ImbroglioCombatAPI.Services
{
    public interface ILeaderboardService
    {
        Task<LeaderboardEntry> AddEntry(LeaderboardEntry entry);
        Task<List<LeaderboardEntry>> GetTopScores(int count = 10);
        Task<List<LeaderboardEntry>> GetFastestCompletions(int count = 10);
        Task<int> GetPlayerRank(string playerName);
    }
}

