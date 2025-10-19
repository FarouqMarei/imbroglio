using Microsoft.AspNetCore.Mvc;
using ImbroglioCombatAPI.Services;
using ImbroglioCombatAPI.Models;
using ImbroglioCombatAPI.Models.DTOs;

namespace ImbroglioCombatAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly ILogger<GameController> _logger;

        public GameController(IGameService gameService, ILogger<GameController> logger)
        {
            _gameService = gameService;
            _logger = logger;
        }

        [HttpPost("new")]
        public async Task<ActionResult<GameState>> CreateNewGame([FromQuery] string playerId = "Player1")
        {
            try
            {
                var gameState = await _gameService.CreateNewGame(playerId);
                return Ok(gameState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new game");
                return StatusCode(500, "Error creating game");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GameState>> GetGameState(int id)
        {
            try
            {
                var gameState = await _gameService.GetGameState(id);
                
                if (gameState == null)
                {
                    return NotFound($"Game state with ID {id} not found");
                }

                return Ok(gameState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting game state");
                return StatusCode(500, "Error retrieving game state");
            }
        }

        [HttpGet("latest/{playerId}")]
        public async Task<ActionResult<GameState>> GetLatestGameState(string playerId)
        {
            try
            {
                var gameState = await _gameService.GetLatestGameState(playerId);
                
                if (gameState == null)
                {
                    return NotFound($"No game state found for player {playerId}");
                }

                return Ok(gameState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest game state");
                return StatusCode(500, "Error retrieving game state");
            }
        }

        [HttpPut("update")]
        public async Task<ActionResult<GameState>> UpdateGameState([FromBody] GameState gameState)
        {
            try
            {
                var updated = await _gameService.UpdateGameState(gameState);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating game state");
                return StatusCode(500, "Error updating game state");
            }
        }

        [HttpPost("move")]
        public async Task<ActionResult<bool>> MoveUnit([FromBody] MoveRequest request)
        {
            try
            {
                var success = await _gameService.ProcessMove(request);
                
                if (!success)
                {
                    return BadRequest("Invalid move");
                }

                return Ok(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing move");
                return StatusCode(500, "Error processing move");
            }
        }

        [HttpPost("attack")]
        public async Task<ActionResult<CombatResult>> Attack([FromBody] AttackRequest request)
        {
            try
            {
                var result = await _gameService.ProcessAttack(request);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing attack");
                return StatusCode(500, "Error processing attack");
            }
        }

        [HttpPost("endturn/{gameStateId}")]
        public async Task<ActionResult<GameState>> EndTurn(int gameStateId)
        {
            try
            {
                var gameState = await _gameService.EndPlayerTurn(gameStateId);
                return Ok(gameState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending turn");
                return StatusCode(500, "Error ending turn");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> DeleteGameState(int id)
        {
            try
            {
                var success = await _gameService.DeleteGameState(id);
                
                if (!success)
                {
                    return NotFound($"Game state with ID {id} not found");
                }

                return Ok(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting game state");
                return StatusCode(500, "Error deleting game state");
            }
        }
    }
}

