using Microsoft.AspNetCore.Mvc;
using ImbroglioCombatAPI.Services;
using ImbroglioCombatAPI.Models;

namespace ImbroglioCombatAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;
        private readonly ILogger<LeaderboardController> _logger;

        public LeaderboardController(ILeaderboardService leaderboardService, ILogger<LeaderboardController> logger)
        {
            _leaderboardService = leaderboardService;
            _logger = logger;
        }

        [HttpPost("add")]
        public async Task<ActionResult<LeaderboardEntry>> AddEntry([FromBody] LeaderboardEntry entry)
        {
            try
            {
                var addedEntry = await _leaderboardService.AddEntry(entry);
                return Ok(addedEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding leaderboard entry");
                return StatusCode(500, "Error adding leaderboard entry");
            }
        }

        [HttpGet("top/{count?}")]
        public async Task<ActionResult<List<LeaderboardEntry>>> GetTopScores(int count = 10)
        {
            try
            {
                var entries = await _leaderboardService.GetTopScores(count);
                return Ok(entries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top scores");
                return StatusCode(500, "Error retrieving leaderboard");
            }
        }

        [HttpGet("fastest/{count?}")]
        public async Task<ActionResult<List<LeaderboardEntry>>> GetFastestCompletions(int count = 10)
        {
            try
            {
                var entries = await _leaderboardService.GetFastestCompletions(count);
                return Ok(entries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fastest completions");
                return StatusCode(500, "Error retrieving leaderboard");
            }
        }

        [HttpGet("rank/{playerName}")]
        public async Task<ActionResult<int>> GetPlayerRank(string playerName)
        {
            try
            {
                var rank = await _leaderboardService.GetPlayerRank(playerName);
                
                if (rank == -1)
                {
                    return NotFound($"Player {playerName} not found in leaderboard");
                }

                return Ok(rank);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting player rank");
                return StatusCode(500, "Error retrieving rank");
            }
        }
    }
}

