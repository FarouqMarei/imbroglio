using UnityEngine;
using System.Collections.Generic;
using ImbroglioCombat.Units;
using ImbroglioCombat.UI;
using ImbroglioCombat.AI;
using ImbroglioCombat.Backend;

namespace ImbroglioCombat.Core
{
    public enum GameState
    {
        PlayerTurn,
        EnemyTurn,
        Processing,
        GameOver,
        Victory
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        public HexGrid hexGrid;
        public GameUI gameUI;
        public AIController aiController;

        [Header("Game State")]
        public GameState currentState = GameState.PlayerTurn;
        public int currentTurn = 1;
        public int score = 0;

        [Header("Units")]
        public List<PlayerUnit> playerUnits = new List<PlayerUnit>();
        public List<EnemyUnit> enemyUnits = new List<EnemyUnit>();
        
        [Header("Selection")]
        public Unit selectedUnit;
        public HexTile selectedTile;
        private List<HexTile> highlightedTiles = new List<HexTile>();

        [Header("Undo System")]
        private Stack<GameStateSnapshot> undoStack = new Stack<GameStateSnapshot>();
        public int maxUndoSteps = 3;

        [Header("Backend")]
        public BackendClient backendClient;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            InitializeGame();
        }

        public void InitializeGame()
        {
            currentState = GameState.PlayerTurn;
            currentTurn = 1;
            score = 0;
            
            if (hexGrid == null)
            {
                hexGrid = FindObjectOfType<HexGrid>();
            }
            
            if (gameUI == null)
            {
                gameUI = FindObjectOfType<GameUI>();
            }
            
            if (aiController == null)
            {
                aiController = FindObjectOfType<AIController>();
            }

            if (backendClient == null)
            {
                backendClient = FindObjectOfType<BackendClient>();
            }

            SpawnInitialUnits();
            UpdateUI();
            
            // Load game state from backend if available
            if (backendClient != null)
            {
                backendClient.LoadGameState();
            }
        }

        void SpawnInitialUnits()
        {
            // Clear existing units
            foreach (var unit in playerUnits)
            {
                if (unit != null) Destroy(unit.gameObject);
            }
            foreach (var unit in enemyUnits)
            {
                if (unit != null) Destroy(unit.gameObject);
            }
            
            playerUnits.Clear();
            enemyUnits.Clear();

            // Spawn player units (example positions)
            SpawnPlayerUnit(new Vector3Int(1, 1, -2));
            SpawnPlayerUnit(new Vector3Int(2, 1, -3));

            // Spawn enemy units (example positions)
            SpawnEnemyUnit(new Vector3Int(7, 7, -14));
            SpawnEnemyUnit(new Vector3Int(6, 7, -13));
            SpawnEnemyUnit(new Vector3Int(8, 6, -14));
        }

        void SpawnPlayerUnit(Vector3Int hexPosition)
        {
            HexTile tile = hexGrid.GetTile(hexPosition);
            if (tile != null && !tile.IsOccupied())
            {
                GameObject unitObj = new GameObject($"Player_Unit_{playerUnits.Count}");
                unitObj.transform.position = tile.transform.position;
                
                PlayerUnit unit = unitObj.AddComponent<PlayerUnit>();
                unit.Initialize(tile, true);
                
                tile.SetOccupyingUnit(unit);
                playerUnits.Add(unit);
            }
        }

        void SpawnEnemyUnit(Vector3Int hexPosition)
        {
            HexTile tile = hexGrid.GetTile(hexPosition);
            if (tile != null && !tile.IsOccupied())
            {
                GameObject unitObj = new GameObject($"Enemy_Unit_{enemyUnits.Count}");
                unitObj.transform.position = tile.transform.position;
                
                EnemyUnit unit = unitObj.AddComponent<EnemyUnit>();
                unit.Initialize(tile, false);
                
                tile.SetOccupyingUnit(unit);
                enemyUnits.Add(unit);
            }
        }

        public void SelectUnit(Unit unit)
        {
            if (currentState != GameState.PlayerTurn || !unit.isPlayerUnit)
            {
                return;
            }

            // Deselect previous
            if (selectedUnit != null)
            {
                ClearHighlights();
            }

            selectedUnit = unit;
            selectedTile = unit.currentTile;

            // Highlight movement and attack ranges
            ShowUnitRanges(unit);
        }

        public void DeselectUnit()
        {
            selectedUnit = null;
            selectedTile = null;
            ClearHighlights();
        }

        void ShowUnitRanges(Unit unit)
        {
            ClearHighlights();

            // Show movement range
            List<HexTile> moveRange = hexGrid.GetTilesInRange(unit.currentTile, unit.stats.moveRange, true);
            foreach (var tile in moveRange)
            {
                tile.ShowHighlight(HighlightType.MoveRange);
                highlightedTiles.Add(tile);
            }

            // Show attack range
            List<HexTile> attackRange = hexGrid.GetTilesInRange(unit.currentTile, unit.stats.attackRange, false);
            foreach (var tile in attackRange)
            {
                if (tile.IsOccupied() && tile.occupyingUnit.isPlayerUnit != unit.isPlayerUnit)
                {
                    tile.ShowHighlight(HighlightType.AttackRange);
                    if (!highlightedTiles.Contains(tile))
                    {
                        highlightedTiles.Add(tile);
                    }
                }
            }

            // Highlight selected unit's tile
            if (unit.currentTile != null)
            {
                unit.currentTile.ShowHighlight(HighlightType.Selected);
            }
        }

        void ClearHighlights()
        {
            foreach (var tile in highlightedTiles)
            {
                tile.HideHighlight();
            }
            highlightedTiles.Clear();
        }

        public void TryMoveUnit(HexTile targetTile)
        {
            if (selectedUnit == null || currentState != GameState.PlayerTurn)
            {
                return;
            }

            if (targetTile.isWalkable && !targetTile.IsOccupied())
            {
                int distance = selectedUnit.currentTile.GetDistanceTo(targetTile);
                
                if (distance <= selectedUnit.stats.moveRange)
                {
                    SaveStateForUndo();
                    MoveUnit(selectedUnit, targetTile);
                    
                    // Save to backend
                    if (backendClient != null)
                    {
                        backendClient.SaveGameState();
                    }
                }
            }
        }

        public void MoveUnit(Unit unit, HexTile targetTile)
        {
            // Clear old tile
            if (unit.currentTile != null)
            {
                unit.currentTile.SetOccupyingUnit(null);
            }

            // Move unit
            unit.MoveTo(targetTile);
            targetTile.SetOccupyingUnit(unit);

            ClearHighlights();
        }

        public void TryAttackUnit(Unit attacker, Unit target)
        {
            if (currentState == GameState.Processing)
            {
                return;
            }

            int distance = attacker.currentTile.GetDistanceTo(target.currentTile);
            
            if (distance <= attacker.stats.attackRange)
            {
                SaveStateForUndo();
                AttackUnit(attacker, target);
                
                // Save to backend
                if (backendClient != null)
                {
                    backendClient.SaveGameState();
                }
            }
        }

        public void AttackUnit(Unit attacker, Unit target)
        {
            int damage = Mathf.Max(1, attacker.stats.attack - target.stats.defense);
            target.TakeDamage(damage);

            if (target.stats.currentHealth <= 0)
            {
                OnUnitDeath(target);
            }

            CheckGameOver();
        }

        void OnUnitDeath(Unit unit)
        {
            if (unit.isPlayerUnit)
            {
                playerUnits.Remove(unit as PlayerUnit);
            }
            else
            {
                enemyUnits.Remove(unit as EnemyUnit);
                score += 100;
            }

            if (unit.currentTile != null)
            {
                unit.currentTile.SetOccupyingUnit(null);
            }

            Destroy(unit.gameObject);
        }

        public void EndPlayerTurn()
        {
            if (currentState != GameState.PlayerTurn)
            {
                return;
            }

            DeselectUnit();
            currentState = GameState.EnemyTurn;
            UpdateUI();

            // Start enemy turn
            StartCoroutine(ExecuteEnemyTurn());
        }

        System.Collections.IEnumerator ExecuteEnemyTurn()
        {
            yield return new WaitForSeconds(0.5f);

            if (aiController != null)
            {
                yield return StartCoroutine(aiController.ExecuteTurn(enemyUnits));
            }

            currentState = GameState.PlayerTurn;
            currentTurn++;
            UpdateUI();
            
            // Save to backend
            if (backendClient != null)
            {
                backendClient.SaveGameState();
            }
        }

        void CheckGameOver()
        {
            if (playerUnits.Count == 0)
            {
                currentState = GameState.GameOver;
                UpdateUI();
                if (gameUI != null)
                {
                    gameUI.ShowGameOver(false);
                }
            }
            else if (enemyUnits.Count == 0)
            {
                currentState = GameState.Victory;
                UpdateUI();
                if (gameUI != null)
                {
                    gameUI.ShowGameOver(true);
                }
            }
        }

        public void RestartGame()
        {
            undoStack.Clear();
            InitializeGame();
        }

        void SaveStateForUndo()
        {
            GameStateSnapshot snapshot = new GameStateSnapshot(this);
            undoStack.Push(snapshot);

            if (undoStack.Count > maxUndoSteps)
            {
                // Remove oldest state
                Stack<GameStateSnapshot> temp = new Stack<GameStateSnapshot>();
                for (int i = 0; i < maxUndoSteps; i++)
                {
                    temp.Push(undoStack.Pop());
                }
                undoStack = temp;
            }
        }

        public void UndoLastMove()
        {
            if (undoStack.Count > 0 && currentState == GameState.PlayerTurn)
            {
                GameStateSnapshot snapshot = undoStack.Pop();
                snapshot.Restore(this);
                UpdateUI();
            }
        }

        void UpdateUI()
        {
            if (gameUI != null)
            {
                gameUI.UpdateTurnText(currentTurn, currentState);
                gameUI.UpdateScore(score);
            }
        }
    }

    // Snapshot class for undo functionality
    [System.Serializable]
    public class GameStateSnapshot
    {
        public int turn;
        public int score;
        public List<UnitSnapshot> unitSnapshots = new List<UnitSnapshot>();

        public GameStateSnapshot(GameManager manager)
        {
            turn = manager.currentTurn;
            score = manager.score;

            foreach (var unit in manager.playerUnits)
            {
                unitSnapshots.Add(new UnitSnapshot(unit));
            }

            foreach (var unit in manager.enemyUnits)
            {
                unitSnapshots.Add(new UnitSnapshot(unit));
            }
        }

        public void Restore(GameManager manager)
        {
            manager.currentTurn = turn;
            manager.score = score;

            // This is a simplified restore - in production you'd need more detailed restoration
            // For now, just restore health values
            foreach (var snapshot in unitSnapshots)
            {
                Unit unit = null;
                
                if (snapshot.isPlayer)
                {
                    unit = manager.playerUnits.Find(u => u.name == snapshot.unitName);
                }
                else
                {
                    unit = manager.enemyUnits.Find(u => u.name == snapshot.unitName);
                }

                if (unit != null)
                {
                    unit.stats.currentHealth = snapshot.health;
                    HexTile tile = manager.hexGrid.GetTile(snapshot.position);
                    if (tile != null && tile != unit.currentTile)
                    {
                        manager.MoveUnit(unit, tile);
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class UnitSnapshot
    {
        public string unitName;
        public bool isPlayer;
        public int health;
        public Vector3Int position;

        public UnitSnapshot(Unit unit)
        {
            unitName = unit.name;
            isPlayer = unit.isPlayerUnit;
            health = unit.stats.currentHealth;
            position = unit.currentTile.gridPosition;
        }
    }
}

