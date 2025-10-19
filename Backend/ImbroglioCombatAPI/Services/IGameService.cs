using ImbroglioCombatAPI.Models;
using ImbroglioCombatAPI.Models.DTOs;

namespace ImbroglioCombatAPI.Services
{
    public interface IGameService
    {
        Task<GameState> CreateNewGame(string playerId);
        Task<GameState?> GetGameState(int gameId);
        Task<GameState?> GetLatestGameState(string playerId);
        Task<GameState> UpdateGameState(GameState gameState);
        Task<bool> DeleteGameState(int gameId);
        Task<CombatResult> ProcessAttack(AttackRequest request);
        Task<bool> ProcessMove(MoveRequest request);
        Task<GameState> EndPlayerTurn(int gameStateId);
    }
}

