using ImbroglioCombatAPI.Models.DTOs;

namespace ImbroglioCombatAPI.Services
{
    public interface IAIService
    {
        Task<AITurnResult> ExecuteAITurn(int gameStateId);
        Task<HexPosition?> FindBestMove(int unitId, int gameStateId);
        Task<int?> FindBestTarget(int unitId, int gameStateId);
    }
}

