using Microsoft.EntityFrameworkCore;
using ImbroglioCombatAPI.Data;
using ImbroglioCombatAPI.Models;
using ImbroglioCombatAPI.Models.DTOs;

namespace ImbroglioCombatAPI.Services
{
    public class AIService : IAIService
    {
        private readonly GameDbContext _context;

        public AIService(GameDbContext context)
        {
            _context = context;
        }

        public async Task<AITurnResult> ExecuteAITurn(int gameStateId)
        {
            var gameState = await _context.GameStates
                .Include(g => g.Units)
                .FirstOrDefaultAsync(g => g.Id == gameStateId);

            if (gameState == null)
            {
                return new AITurnResult { Success = false, Message = "Game state not found" };
            }

            var result = new AITurnResult { Success = true, Actions = new List<AIAction>() };
            var enemyUnits = gameState.Units.Where(u => !u.IsPlayerUnit && !u.HasActed).ToList();

            foreach (var enemy in enemyUnits)
            {
                // Find nearest player unit
                var nearestPlayer = FindNearestPlayerUnit(enemy, gameState.Units);
                
                if (nearestPlayer == null)
                {
                    continue;
                }

                int distance = CalculateDistance(enemy, nearestPlayer);

                // If in attack range, attack
                if (distance <= enemy.AttackRange)
                {
                    int damage = Math.Max(1, enemy.Attack - nearestPlayer.Defense);
                    nearestPlayer.CurrentHealth -= damage;

                    result.Actions.Add(new AIAction
                    {
                        ActionType = "Attack",
                        UnitId = enemy.Id,
                        TargetUnitId = nearestPlayer.Id
                    });

                    if (nearestPlayer.CurrentHealth <= 0)
                    {
                        gameState.Units.Remove(nearestPlayer);
                        _context.UnitStates.Remove(nearestPlayer);
                    }
                }
                // Otherwise, move towards player
                else
                {
                    var bestMove = FindBestMoveTowards(enemy, nearestPlayer, gameState.Units);
                    
                    if (bestMove != null)
                    {
                        enemy.PositionQ = bestMove.Q;
                        enemy.PositionR = bestMove.R;
                        enemy.PositionS = bestMove.S;

                        result.Actions.Add(new AIAction
                        {
                            ActionType = "Move",
                            UnitId = enemy.Id,
                            TargetPosition = bestMove
                        });

                        // Check if now in range to attack
                        distance = CalculateDistance(enemy, nearestPlayer);
                        if (distance <= enemy.AttackRange)
                        {
                            int damage = Math.Max(1, enemy.Attack - nearestPlayer.Defense);
                            nearestPlayer.CurrentHealth -= damage;

                            result.Actions.Add(new AIAction
                            {
                                ActionType = "Attack",
                                UnitId = enemy.Id,
                                TargetUnitId = nearestPlayer.Id
                            });

                            if (nearestPlayer.CurrentHealth <= 0)
                            {
                                gameState.Units.Remove(nearestPlayer);
                                _context.UnitStates.Remove(nearestPlayer);
                            }
                        }
                    }
                }

                enemy.HasActed = true;
            }

            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<HexPosition?> FindBestMove(int unitId, int gameStateId)
        {
            var gameState = await _context.GameStates
                .Include(g => g.Units)
                .FirstOrDefaultAsync(g => g.Id == gameStateId);

            if (gameState == null)
            {
                return null;
            }

            var unit = gameState.Units.FirstOrDefault(u => u.Id == unitId);
            if (unit == null)
            {
                return null;
            }

            var targetUnit = unit.IsPlayerUnit 
                ? gameState.Units.FirstOrDefault(u => !u.IsPlayerUnit)
                : gameState.Units.FirstOrDefault(u => u.IsPlayerUnit);

            if (targetUnit == null)
            {
                return null;
            }

            return FindBestMoveTowards(unit, targetUnit, gameState.Units);
        }

        public async Task<int?> FindBestTarget(int unitId, int gameStateId)
        {
            var gameState = await _context.GameStates
                .Include(g => g.Units)
                .FirstOrDefaultAsync(g => g.Id == gameStateId);

            if (gameState == null)
            {
                return null;
            }

            var unit = gameState.Units.FirstOrDefault(u => u.Id == unitId);
            if (unit == null)
            {
                return null;
            }

            var targetUnits = gameState.Units.Where(u => u.IsPlayerUnit != unit.IsPlayerUnit).ToList();
            UnitState? nearestTarget = null;
            int shortestDistance = int.MaxValue;

            foreach (var target in targetUnits)
            {
                int distance = CalculateDistance(unit, target);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestTarget = target;
                }
            }

            return nearestTarget?.Id;
        }

        private UnitState? FindNearestPlayerUnit(UnitState enemy, List<UnitState> allUnits)
        {
            var playerUnits = allUnits.Where(u => u.IsPlayerUnit).ToList();
            UnitState? nearest = null;
            int shortestDistance = int.MaxValue;

            foreach (var player in playerUnits)
            {
                int distance = CalculateDistance(enemy, player);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearest = player;
                }
            }

            return nearest;
        }

        private HexPosition? FindBestMoveTowards(UnitState mover, UnitState target, List<UnitState> allUnits)
        {
            // Get all positions within move range
            List<HexPosition> possibleMoves = GetPositionsInRange(
                new HexPosition { Q = mover.PositionQ, R = mover.PositionR, S = mover.PositionS },
                mover.MoveRange);

            HexPosition? bestPosition = null;
            int bestDistance = CalculateDistance(mover, target);

            foreach (var position in possibleMoves)
            {
                // Check if position is occupied
                bool isOccupied = allUnits.Any(u =>
                    u.PositionQ == position.Q &&
                    u.PositionR == position.R &&
                    u.PositionS == position.S);

                if (isOccupied)
                {
                    continue;
                }

                int distance = CalculateDistance(position, new HexPosition 
                { 
                    Q = target.PositionQ, 
                    R = target.PositionR, 
                    S = target.PositionS 
                });

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestPosition = position;
                }
            }

            return bestPosition;
        }

        private List<HexPosition> GetPositionsInRange(HexPosition center, int range)
        {
            var positions = new List<HexPosition>();

            for (int q = -range; q <= range; q++)
            {
                for (int r = Math.Max(-range, -q - range); r <= Math.Min(range, -q + range); r++)
                {
                    int s = -q - r;
                    var pos = new HexPosition
                    {
                        Q = center.Q + q,
                        R = center.R + r,
                        S = center.S + s
                    };

                    int distance = CalculateDistance(center, pos);
                    if (distance <= range && distance > 0)
                    {
                        positions.Add(pos);
                    }
                }
            }

            return positions;
        }

        private int CalculateDistance(UnitState a, UnitState b)
        {
            return (Math.Abs(a.PositionQ - b.PositionQ) + 
                    Math.Abs(a.PositionR - b.PositionR) + 
                    Math.Abs(a.PositionS - b.PositionS)) / 2;
        }

        private int CalculateDistance(HexPosition a, HexPosition b)
        {
            return (Math.Abs(a.Q - b.Q) + Math.Abs(a.R - b.R) + Math.Abs(a.S - b.S)) / 2;
        }
    }
}

