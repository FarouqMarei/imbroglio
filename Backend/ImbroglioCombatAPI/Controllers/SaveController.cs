using Microsoft.AspNetCore.Mvc;
using ImbroglioCombatAPI.Services;
using ImbroglioCombatAPI.Models;

namespace ImbroglioCombatAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SaveController : ControllerBase
    {
        private readonly ISaveService _saveService;
        private readonly ILogger<SaveController> _logger;

        public SaveController(ISaveService saveService, ILogger<SaveController> logger)
        {
            _saveService = saveService;
            _logger = logger;
        }

        [HttpPost("save")]
        public async Task<ActionResult<bool>> SaveGame([FromBody] GameState gameState)
        {
            try
            {
                var success = await _saveService.SaveGame(gameState);
                return Ok(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving game");
                return StatusCode(500, "Error saving game");
            }
        }

        [HttpGet("load/{playerId}")]
        public async Task<ActionResult<GameState>> LoadGame(string playerId)
        {
            try
            {
                var gameState = await _saveService.LoadGame(playerId);
                
                if (gameState == null)
                {
                    return NotFound($"No saved game found for player {playerId}");
                }

                return Ok(gameState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading game");
                return StatusCode(500, "Error loading game");
            }
        }

        [HttpGet("all/{playerId}")]
        public async Task<ActionResult<List<GameState>>> GetAllSaves(string playerId)
        {
            try
            {
                var saves = await _saveService.GetAllSaves(playerId);
                return Ok(saves);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all saves");
                return StatusCode(500, "Error retrieving saves");
            }
        }

        [HttpDelete("{gameStateId}")]
        public async Task<ActionResult<bool>> DeleteSave(int gameStateId)
        {
            try
            {
                var success = await _saveService.DeleteSave(gameStateId);
                
                if (!success)
                {
                    return NotFound($"Save with ID {gameStateId} not found");
                }

                return Ok(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting save");
                return StatusCode(500, "Error deleting save");
            }
        }
    }
}

