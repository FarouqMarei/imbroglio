using ImbroglioCombatAPI.Models;

namespace ImbroglioCombatAPI.Services
{
    public interface ISaveService
    {
        Task<bool> SaveGame(GameState gameState);
        Task<GameState?> LoadGame(string playerId);
        Task<List<GameState>> GetAllSaves(string playerId);
        Task<bool> DeleteSave(int gameStateId);
    }
}

