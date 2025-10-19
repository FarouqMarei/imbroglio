using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using ImbroglioCombat.Core;
using ImbroglioCombat.Units;

namespace ImbroglioCombat.Backend
{
    public class BackendClient : MonoBehaviour
    {
        [Header("Backend Settings")]
        public string baseUrl = "http://localhost:5000/api";
        public string playerId = "Player1";

        [Header("Current State")]
        public int currentGameStateId = -1;

        private GameManager gameManager;

        void Start()
        {
            gameManager = GameManager.Instance;
        }

        // Create a new game on the backend
        public void CreateNewGame()
        {
            StartCoroutine(CreateNewGameCoroutine());
        }

        IEnumerator CreateNewGameCoroutine()
        {
            string url = $"{baseUrl}/Game/new?playerId={playerId}";

            using (UnityWebRequest request = UnityWebRequest.Post(url, ""))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    GameStateResponse response = JsonConvert.DeserializeObject<GameStateResponse>(jsonResponse);
                    
                    if (response != null)
                    {
                        currentGameStateId = response.id;
                        Debug.Log($"New game created with ID: {currentGameStateId}");
                    }
                }
                else
                {
                    Debug.LogError($"Error creating new game: {request.error}");
                }
            }
        }

        // Save current game state to backend
        public void SaveGameState()
        {
            if (gameManager == null || currentGameStateId == -1)
            {
                return;
            }

            StartCoroutine(SaveGameStateCoroutine());
        }

        IEnumerator SaveGameStateCoroutine()
        {
            GameStateRequest gameState = SerializeGameState();
            string jsonData = JsonConvert.SerializeObject(gameState);

            string url = $"{baseUrl}/Game/update";

            using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Game state saved successfully");
                }
                else
                {
                    Debug.LogError($"Error saving game state: {request.error}");
                }
            }
        }

        // Load game state from backend
        public void LoadGameState()
        {
            StartCoroutine(LoadGameStateCoroutine());
        }

        IEnumerator LoadGameStateCoroutine()
        {
            string url = $"{baseUrl}/Game/latest/{playerId}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    GameStateResponse response = JsonConvert.DeserializeObject<GameStateResponse>(jsonResponse);
                    
                    if (response != null)
                    {
                        ApplyGameState(response);
                        Debug.Log($"Game state loaded: Turn {response.currentTurn}, Score {response.score}");
                    }
                }
                else
                {
                    Debug.Log($"No saved game found or error: {request.error}");
                    CreateNewGame();
                }
            }
        }

        // Send move to backend
        public void SendMove(int unitId, Vector3Int targetPosition)
        {
            StartCoroutine(SendMoveCoroutine(unitId, targetPosition));
        }

        IEnumerator SendMoveCoroutine(int unitId, Vector3Int targetPosition)
        {
            MoveRequestData moveRequest = new MoveRequestData
            {
                gameStateId = currentGameStateId,
                unitId = unitId,
                targetPosition = new HexPositionData
                {
                    q = targetPosition.x,
                    r = targetPosition.y,
                    s = targetPosition.z
                }
            };

            string jsonData = JsonConvert.SerializeObject(moveRequest);
            string url = $"{baseUrl}/Game/move";

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Move sent successfully");
                }
                else
                {
                    Debug.LogError($"Error sending move: {request.error}");
                }
            }
        }

        // Send attack to backend
        public void SendAttack(int attackerId, int targetId)
        {
            StartCoroutine(SendAttackCoroutine(attackerId, targetId));
        }

        IEnumerator SendAttackCoroutine(int attackerId, int targetId)
        {
            AttackRequestData attackRequest = new AttackRequestData
            {
                gameStateId = currentGameStateId,
                attackerUnitId = attackerId,
                targetUnitId = targetId
            };

            string jsonData = JsonConvert.SerializeObject(attackRequest);
            string url = $"{baseUrl}/Game/attack";

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string response = request.downloadHandler.text;
                    Debug.Log($"Attack result: {response}");
                }
                else
                {
                    Debug.LogError($"Error sending attack: {request.error}");
                }
            }
        }

        // End turn on backend (triggers AI)
        public void EndTurn()
        {
            StartCoroutine(EndTurnCoroutine());
        }

        IEnumerator EndTurnCoroutine()
        {
            string url = $"{baseUrl}/Game/endturn/{currentGameStateId}";

            using (UnityWebRequest request = UnityWebRequest.Post(url, ""))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    GameStateResponse response = JsonConvert.DeserializeObject<GameStateResponse>(jsonResponse);
                    
                    if (response != null)
                    {
                        ApplyGameState(response);
                        Debug.Log("Turn ended, AI moves processed");
                    }
                }
                else
                {
                    Debug.LogError($"Error ending turn: {request.error}");
                }
            }
        }

        // Serialize current game state
        GameStateRequest SerializeGameState()
        {
            GameStateRequest request = new GameStateRequest
            {
                id = currentGameStateId,
                playerId = playerId,
                currentTurn = gameManager.currentTurn,
                score = gameManager.score,
                state = gameManager.currentState.ToString(),
                units = new List<UnitStateData>()
            };

            // Add player units
            foreach (var unit in gameManager.playerUnits)
            {
                if (unit != null)
                {
                    request.units.Add(SerializeUnit(unit));
                }
            }

            // Add enemy units
            foreach (var unit in gameManager.enemyUnits)
            {
                if (unit != null)
                {
                    request.units.Add(SerializeUnit(unit));
                }
            }

            return request;
        }

        UnitStateData SerializeUnit(Unit unit)
        {
            return new UnitStateData
            {
                unitName = unit.unitName,
                isPlayerUnit = unit.isPlayerUnit,
                currentHealth = unit.stats.currentHealth,
                maxHealth = unit.stats.maxHealth,
                attack = unit.stats.attack,
                defense = unit.stats.defense,
                moveRange = unit.stats.moveRange,
                attackRange = unit.stats.attackRange,
                position = new HexPositionData
                {
                    q = unit.currentTile.gridPosition.x,
                    r = unit.currentTile.gridPosition.y,
                    s = unit.currentTile.gridPosition.z
                },
                hasActed = unit.hasActedThisTurn
            };
        }

        // Apply game state from backend
        void ApplyGameState(GameStateResponse response)
        {
            if (gameManager == null)
            {
                return;
            }

            currentGameStateId = response.id;
            gameManager.currentTurn = response.currentTurn;
            gameManager.score = response.score;

            // Parse state
            System.Enum.TryParse(response.state, out GameState state);
            gameManager.currentState = state;

            // TODO: Update units based on response
            // This would require more complex logic to sync units
        }
    }

    // Data transfer objects for JSON serialization
    [System.Serializable]
    public class GameStateRequest
    {
        public int id;
        public string playerId;
        public int currentTurn;
        public int score;
        public string state;
        public List<UnitStateData> units;
    }

    [System.Serializable]
    public class GameStateResponse
    {
        public int id;
        public string playerId;
        public int currentTurn;
        public int score;
        public string state;
        public List<UnitStateData> units;
    }

    [System.Serializable]
    public class UnitStateData
    {
        public int id;
        public string unitName;
        public bool isPlayerUnit;
        public int currentHealth;
        public int maxHealth;
        public int attack;
        public int defense;
        public int moveRange;
        public int attackRange;
        public HexPositionData position;
        public bool hasActed;
    }

    [System.Serializable]
    public class HexPositionData
    {
        public int q;
        public int r;
        public int s;
    }

    [System.Serializable]
    public class MoveRequestData
    {
        public int gameStateId;
        public int unitId;
        public HexPositionData targetPosition;
    }

    [System.Serializable]
    public class AttackRequestData
    {
        public int gameStateId;
        public int attackerUnitId;
        public int targetUnitId;
    }
}

