using Microsoft.EntityFrameworkCore;
using ImbroglioCombatAPI.Data;
using ImbroglioCombatAPI.Models;
using Newtonsoft.Json;

namespace ImbroglioCombatAPI.Services
{
    public class SaveService : ISaveService
    {
        private readonly GameDbContext _context;

        public SaveService(GameDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SaveGame(GameState gameState)
        {
            try
            {
                // Serialize game data
                gameState.SerializedGameData = JsonConvert.SerializeObject(new
                {
                    Turn = gameState.CurrentTurn,
                    Score = gameState.Score,
                    State = gameState.State,
                    Units = gameState.Units
                });

                gameState.LastUpdated = DateTime.UtcNow;
                
                _context.GameStates.Update(gameState);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving game: {ex.Message}");
                return false;
            }
        }

        public async Task<GameState?> LoadGame(string playerId)
        {
            return await _context.GameStates
                .Include(g => g.Units)
                .Where(g => g.PlayerId == playerId)
                .OrderByDescending(g => g.LastUpdated)
                .FirstOrDefaultAsync();
        }

        public async Task<List<GameState>> GetAllSaves(string playerId)
        {
            return await _context.GameStates
                .Include(g => g.Units)
                .Where(g => g.PlayerId == playerId)
                .OrderByDescending(g => g.LastUpdated)
                .ToListAsync();
        }

        public async Task<bool> DeleteSave(int gameStateId)
        {
            var gameState = await _context.GameStates.FindAsync(gameStateId);
            
            if (gameState == null)
            {
                return false;
            }

            _context.GameStates.Remove(gameState);
            await _context.SaveChangesAsync();
            
            return true;
        }
    }
}

