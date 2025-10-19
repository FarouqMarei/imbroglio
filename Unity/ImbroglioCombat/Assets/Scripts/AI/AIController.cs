using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ImbroglioCombat.Core;
using ImbroglioCombat.Units;

namespace ImbroglioCombat.AI
{
    public class AIController : MonoBehaviour
    {
        [Header("AI Settings")]
        public float actionDelay = 0.5f;
        
        private HexGrid hexGrid;
        private Pathfinder pathfinder;

        void Start()
        {
            hexGrid = FindObjectOfType<HexGrid>();
            pathfinder = GetComponent<Pathfinder>();
            
            if (pathfinder == null)
            {
                pathfinder = gameObject.AddComponent<Pathfinder>();
            }
        }

        public IEnumerator ExecuteTurn(List<EnemyUnit> enemyUnits)
        {
            foreach (var enemy in enemyUnits)
            {
                if (enemy != null && enemy.CanAct())
                {
                    yield return StartCoroutine(ExecuteEnemyAction(enemy));
                    yield return new WaitForSeconds(actionDelay);
                }
            }

            // Reset all enemy units for next turn
            foreach (var enemy in enemyUnits)
            {
                if (enemy != null)
                {
                    enemy.ResetTurnState();
                }
            }
        }

        IEnumerator ExecuteEnemyAction(EnemyUnit enemy)
        {
            // Find nearest player unit
            PlayerUnit nearestPlayer = FindNearestPlayer(enemy);
            
            if (nearestPlayer == null)
            {
                yield break;
            }

            int distanceToPlayer = enemy.currentTile.GetDistanceTo(nearestPlayer.currentTile);

            // If in attack range, attack
            if (distanceToPlayer <= enemy.stats.attackRange)
            {
                yield return StartCoroutine(enemy.AttackAnimation(nearestPlayer));
                enemy.SetActed();
            }
            // Otherwise, move towards player
            else
            {
                HexTile targetTile = FindBestMoveTowards(enemy, nearestPlayer.currentTile);
                
                if (targetTile != null && targetTile != enemy.currentTile)
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.MoveUnit(enemy, targetTile);
                    }
                    
                    yield return new WaitForSeconds(0.3f);
                    
                    // Check if now in range to attack
                    distanceToPlayer = enemy.currentTile.GetDistanceTo(nearestPlayer.currentTile);
                    if (distanceToPlayer <= enemy.stats.attackRange)
                    {
                        yield return StartCoroutine(enemy.AttackAnimation(nearestPlayer));
                    }
                }
                
                enemy.SetActed();
            }
        }

        PlayerUnit FindNearestPlayer(EnemyUnit enemy)
        {
            PlayerUnit nearest = null;
            int shortestDistance = int.MaxValue;

            if (GameManager.Instance != null)
            {
                foreach (var player in GameManager.Instance.playerUnits)
                {
                    if (player != null)
                    {
                        int distance = enemy.currentTile.GetDistanceTo(player.currentTile);
                        
                        if (distance < shortestDistance)
                        {
                            shortestDistance = distance;
                            nearest = player;
                        }
                    }
                }
            }

            return nearest;
        }

        HexTile FindBestMoveTowards(EnemyUnit enemy, HexTile targetTile)
        {
            if (pathfinder == null || hexGrid == null)
            {
                return null;
            }

            // Get walkable tiles in move range
            List<HexTile> movableTiles = hexGrid.GetTilesInRange(enemy.currentTile, enemy.stats.moveRange, true);
            
            if (movableTiles.Count == 0)
            {
                return enemy.currentTile;
            }

            // Find tile that gets closest to target
            HexTile bestTile = enemy.currentTile;
            int bestDistance = enemy.currentTile.GetDistanceTo(targetTile);

            foreach (var tile in movableTiles)
            {
                int distance = tile.GetDistanceTo(targetTile);
                
                if (distance < bestDistance)
                {
                    // Check if path exists
                    List<HexTile> path = pathfinder.FindPath(enemy.currentTile, tile, hexGrid);
                    
                    if (path != null && path.Count > 0 && path.Count <= enemy.stats.moveRange)
                    {
                        bestDistance = distance;
                        bestTile = tile;
                    }
                }
            }

            return bestTile;
        }

        // Advanced AI behavior based on enemy type
        public IEnumerator ExecuteAIBehavior(EnemyUnit enemy)
        {
            switch (enemy.aiType)
            {
                case EnemyAIType.Aggressive:
                    yield return StartCoroutine(ExecuteAggressiveBehavior(enemy));
                    break;
                    
                case EnemyAIType.Defensive:
                    yield return StartCoroutine(ExecuteDefensiveBehavior(enemy));
                    break;
                    
                case EnemyAIType.Balanced:
                    yield return StartCoroutine(ExecuteBalancedBehavior(enemy));
                    break;
            }
        }

        IEnumerator ExecuteAggressiveBehavior(EnemyUnit enemy)
        {
            // Always pursue and attack the nearest player
            yield return StartCoroutine(ExecuteEnemyAction(enemy));
        }

        IEnumerator ExecuteDefensiveBehavior(EnemyUnit enemy)
        {
            // Only attack if player is nearby, otherwise stay put
            PlayerUnit nearestPlayer = FindNearestPlayer(enemy);
            
            if (nearestPlayer != null)
            {
                int distance = enemy.currentTile.GetDistanceTo(nearestPlayer.currentTile);
                
                if (distance <= enemy.stats.attackRange + 1)
                {
                    yield return StartCoroutine(ExecuteEnemyAction(enemy));
                }
                else
                {
                    enemy.SetActed();
                }
            }
        }

        IEnumerator ExecuteBalancedBehavior(EnemyUnit enemy)
        {
            // Attack if health is high, retreat if low
            float healthPercent = (float)enemy.stats.currentHealth / enemy.stats.maxHealth;
            
            if (healthPercent > 0.5f)
            {
                yield return StartCoroutine(ExecuteAggressiveBehavior(enemy));
            }
            else
            {
                // Try to move away from nearest player
                PlayerUnit nearestPlayer = FindNearestPlayer(enemy);
                
                if (nearestPlayer != null)
                {
                    HexTile retreatTile = FindBestRetreatTile(enemy, nearestPlayer.currentTile);
                    
                    if (retreatTile != null && retreatTile != enemy.currentTile)
                    {
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.MoveUnit(enemy, retreatTile);
                        }
                        yield return new WaitForSeconds(0.3f);
                    }
                }
                
                enemy.SetActed();
            }
        }

        HexTile FindBestRetreatTile(EnemyUnit enemy, HexTile threatTile)
        {
            if (hexGrid == null)
            {
                return enemy.currentTile;
            }

            List<HexTile> movableTiles = hexGrid.GetTilesInRange(enemy.currentTile, enemy.stats.moveRange, true);
            
            HexTile bestTile = enemy.currentTile;
            int bestDistance = enemy.currentTile.GetDistanceTo(threatTile);

            foreach (var tile in movableTiles)
            {
                int distance = tile.GetDistanceTo(threatTile);
                
                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    bestTile = tile;
                }
            }

            return bestTile;
        }
    }
}

