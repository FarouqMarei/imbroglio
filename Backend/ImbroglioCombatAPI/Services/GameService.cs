using Microsoft.EntityFrameworkCore;
using ImbroglioCombatAPI.Data;
using ImbroglioCombatAPI.Models;
using ImbroglioCombatAPI.Models.DTOs;

namespace ImbroglioCombatAPI.Services
{
    public class GameService : IGameService
    {
        private readonly GameDbContext _context;
        private readonly IAIService _aiService;

        public GameService(GameDbContext context, IAIService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        public async Task<GameState> CreateNewGame(string playerId)
        {
            var gameState = new GameState
            {
                PlayerId = playerId,
                CurrentTurn = 1,
                Score = 0,
                State = "PlayerTurn",
                LastUpdated = DateTime.UtcNow,
                Units = new List<UnitState>()
            };

            // Create initial player units
            gameState.Units.Add(new UnitState
            {
                UnitName = "Player_Unit_1",
                IsPlayerUnit = true,
                CurrentHealth = 120,
                MaxHealth = 120,
                Attack = 25,
                Defense = 8,
                MoveRange = 4,
                AttackRange = 1,
                PositionQ = 1,
                PositionR = 1,
                PositionS = -2,
                HasActed = false
            });

            gameState.Units.Add(new UnitState
            {
                UnitName = "Player_Unit_2",
                IsPlayerUnit = true,
                CurrentHealth = 120,
                MaxHealth = 120,
                Attack = 25,
                Defense = 8,
                MoveRange = 4,
                AttackRange = 1,
                PositionQ = 2,
                PositionR = 1,
                PositionS = -3,
                HasActed = false
            });

            // Create initial enemy units
            gameState.Units.Add(new UnitState
            {
                UnitName = "Enemy_Unit_1",
                IsPlayerUnit = false,
                CurrentHealth = 80,
                MaxHealth = 80,
                Attack = 18,
                Defense = 5,
                MoveRange = 3,
                AttackRange = 1,
                PositionQ = 7,
                PositionR = 7,
                PositionS = -14,
                HasActed = false
            });

            gameState.Units.Add(new UnitState
            {
                UnitName = "Enemy_Unit_2",
                IsPlayerUnit = false,
                CurrentHealth = 80,
                MaxHealth = 80,
                Attack = 18,
                Defense = 5,
                MoveRange = 3,
                AttackRange = 1,
                PositionQ = 6,
                PositionR = 7,
                PositionS = -13,
                HasActed = false
            });

            gameState.Units.Add(new UnitState
            {
                UnitName = "Enemy_Unit_3",
                IsPlayerUnit = false,
                CurrentHealth = 80,
                MaxHealth = 80,
                Attack = 18,
                Defense = 5,
                MoveRange = 3,
                AttackRange = 1,
                PositionQ = 8,
                PositionR = 6,
                PositionS = -14,
                HasActed = false
            });

            _context.GameStates.Add(gameState);
            await _context.SaveChangesAsync();

            return gameState;
        }

        public async Task<GameState?> GetGameState(int gameId)
        {
            return await _context.GameStates
                .Include(g => g.Units)
                .FirstOrDefaultAsync(g => g.Id == gameId);
        }

        public async Task<GameState?> GetLatestGameState(string playerId)
        {
            return await _context.GameStates
                .Include(g => g.Units)
                .Where(g => g.PlayerId == playerId)
                .OrderByDescending(g => g.LastUpdated)
                .FirstOrDefaultAsync();
        }

        public async Task<GameState> UpdateGameState(GameState gameState)
        {
            gameState.LastUpdated = DateTime.UtcNow;
            _context.GameStates.Update(gameState);
            await _context.SaveChangesAsync();
            return gameState;
        }

        public async Task<bool> DeleteGameState(int gameId)
        {
            var gameState = await _context.GameStates.FindAsync(gameId);
            if (gameState == null)
            {
                return false;
            }

            _context.GameStates.Remove(gameState);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CombatResult> ProcessAttack(AttackRequest request)
        {
            var gameState = await GetGameState(request.GameStateId);
            if (gameState == null)
            {
                return new CombatResult { Success = false, Message = "Game state not found" };
            }

            var attacker = gameState.Units.FirstOrDefault(u => u.Id == request.AttackerUnitId);
            var target = gameState.Units.FirstOrDefault(u => u.Id == request.TargetUnitId);

            if (attacker == null || target == null)
            {
                return new CombatResult { Success = false, Message = "Unit not found" };
            }

            // Check if attacker and target are on different teams
            if (attacker.IsPlayerUnit == target.IsPlayerUnit)
            {
                return new CombatResult { Success = false, Message = "Cannot attack friendly units" };
            }

            // Check range
            int distance = CalculateHexDistance(
                attacker.PositionQ, attacker.PositionR, attacker.PositionS,
                target.PositionQ, target.PositionR, target.PositionS);

            if (distance > attacker.AttackRange)
            {
                return new CombatResult { Success = false, Message = "Target out of range" };
            }

            // Calculate damage
            int damage = Math.Max(1, attacker.Attack - target.Defense);
            target.CurrentHealth -= damage;

            bool targetDestroyed = target.CurrentHealth <= 0;
            if (targetDestroyed)
            {
                gameState.Units.Remove(target);
                _context.UnitStates.Remove(target);
                
                // Award score if enemy destroyed
                if (!target.IsPlayerUnit)
                {
                    gameState.Score += 100;
                }
            }

            attacker.HasActed = true;
            await UpdateGameState(gameState);

            return new CombatResult
            {
                Success = true,
                DamageDealt = damage,
                TargetDestroyed = targetDestroyed,
                Message = $"Attack dealt {damage} damage"
            };
        }

        public async Task<bool> ProcessMove(MoveRequest request)
        {
            var gameState = await GetGameState(request.GameStateId);
            if (gameState == null)
            {
                return false;
            }

            var unit = gameState.Units.FirstOrDefault(u => u.Id == request.UnitId);
            if (unit == null)
            {
                return false;
            }

            // Check if target position is within move range
            int distance = CalculateHexDistance(
                unit.PositionQ, unit.PositionR, unit.PositionS,
                request.TargetPosition.Q, request.TargetPosition.R, request.TargetPosition.S);

            if (distance > unit.MoveRange)
            {
                return false;
            }

            // Check if target tile is occupied
            bool isOccupied = gameState.Units.Any(u => 
                u.PositionQ == request.TargetPosition.Q &&
                u.PositionR == request.TargetPosition.R &&
                u.PositionS == request.TargetPosition.S);

            if (isOccupied)
            {
                return false;
            }

            // Move unit
            unit.PositionQ = request.TargetPosition.Q;
            unit.PositionR = request.TargetPosition.R;
            unit.PositionS = request.TargetPosition.S;
            unit.HasActed = true;

            await UpdateGameState(gameState);
            return true;
        }

        public async Task<GameState> EndPlayerTurn(int gameStateId)
        {
            var gameState = await GetGameState(gameStateId);
            if (gameState == null)
            {
                throw new Exception("Game state not found");
            }

            gameState.State = "EnemyTurn";
            await UpdateGameState(gameState);

            // Execute AI turn
            await _aiService.ExecuteAITurn(gameStateId);

            // Reset all units for next turn
            foreach (var unit in gameState.Units)
            {
                unit.HasActed = false;
            }

            gameState.CurrentTurn++;
            gameState.State = "PlayerTurn";
            
            // Check win/loss conditions
            bool hasPlayerUnits = gameState.Units.Any(u => u.IsPlayerUnit);
            bool hasEnemyUnits = gameState.Units.Any(u => !u.IsPlayerUnit);

            if (!hasPlayerUnits)
            {
                gameState.State = "GameOver";
            }
            else if (!hasEnemyUnits)
            {
                gameState.State = "Victory";
            }

            await UpdateGameState(gameState);
            return gameState;
        }

        private int CalculateHexDistance(int q1, int r1, int s1, int q2, int r2, int s2)
        {
            return (Math.Abs(q1 - q2) + Math.Abs(r1 - r2) + Math.Abs(s1 - s2)) / 2;
        }
    }
}

