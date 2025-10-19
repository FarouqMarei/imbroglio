# API Reference - Imbroglio Combat Backend

Base URL: `http://localhost:5000/api`

## Authentication

Currently, no authentication is required. In production, implement JWT tokens or API keys.

## Game Management

### Create New Game

Creates a new game instance with initial units.

**Endpoint:** `POST /api/Game/new`

**Query Parameters:**
- `playerId` (string, optional): Player identifier. Default: "Player1"

**Response:** `GameState` object

```json
{
  "id": 1,
  "playerId": "Player1",
  "currentTurn": 1,
  "score": 0,
  "state": "PlayerTurn",
  "lastUpdated": "2025-10-19T12:00:00Z",
  "units": [
    {
      "id": 1,
      "unitName": "Player_Unit_1",
      "isPlayerUnit": true,
      "currentHealth": 120,
      "maxHealth": 120,
      "attack": 25,
      "defense": 8,
      "moveRange": 4,
      "attackRange": 1,
      "positionQ": 1,
      "positionR": 1,
      "positionS": -2,
      "hasActed": false
    }
  ]
}
```

**Example:**
```bash
curl -X POST "http://localhost:5000/api/Game/new?playerId=Player1"
```

---

### Get Game State

Retrieves current state of a specific game.

**Endpoint:** `GET /api/Game/{id}`

**Path Parameters:**
- `id` (integer): Game state ID

**Response:** `GameState` object

**Example:**
```bash
curl -X GET "http://localhost:5000/api/Game/1"
```

---

### Get Latest Game State

Gets the most recent game for a player.

**Endpoint:** `GET /api/Game/latest/{playerId}`

**Path Parameters:**
- `playerId` (string): Player identifier

**Response:** `GameState` object or 404 if not found

**Example:**
```bash
curl -X GET "http://localhost:5000/api/Game/latest/Player1"
```

---

### Update Game State

Updates an existing game state.

**Endpoint:** `PUT /api/Game/update`

**Request Body:** `GameState` object

```json
{
  "id": 1,
  "playerId": "Player1",
  "currentTurn": 2,
  "score": 100,
  "state": "PlayerTurn",
  "units": [...]
}
```

**Response:** Updated `GameState` object

**Example:**
```bash
curl -X PUT "http://localhost:5000/api/Game/update" \
  -H "Content-Type: application/json" \
  -d @gamestate.json
```

---

### Move Unit

Processes a unit movement.

**Endpoint:** `POST /api/Game/move`

**Request Body:**
```json
{
  "gameStateId": 1,
  "unitId": 1,
  "targetPosition": {
    "q": 2,
    "r": 2,
    "s": -4
  }
}
```

**Response:** `boolean` (true if successful)

**Validation:**
- Unit must exist
- Target must be within move range
- Target must not be occupied
- Target must be walkable

**Example:**
```bash
curl -X POST "http://localhost:5000/api/Game/move" \
  -H "Content-Type: application/json" \
  -d '{"gameStateId":1,"unitId":1,"targetPosition":{"q":2,"r":2,"s":-4}}'
```

---

### Attack Unit

Processes an attack between two units.

**Endpoint:** `POST /api/Game/attack`

**Request Body:**
```json
{
  "gameStateId": 1,
  "attackerUnitId": 1,
  "targetUnitId": 3
}
```

**Response:** `CombatResult` object

```json
{
  "success": true,
  "damageDealt": 20,
  "targetDestroyed": false,
  "message": "Attack dealt 20 damage"
}
```

**Validation:**
- Both units must exist
- Units must be on different teams
- Target must be within attack range

**Combat Formula:**
```
damage = max(1, attacker.attack - target.defense)
```

**Example:**
```bash
curl -X POST "http://localhost:5000/api/Game/attack" \
  -H "Content-Type: application/json" \
  -d '{"gameStateId":1,"attackerUnitId":1,"targetUnitId":3}'
```

---

### End Turn

Ends player turn and executes AI turn.

**Endpoint:** `POST /api/Game/endturn/{gameStateId}`

**Path Parameters:**
- `gameStateId` (integer): Game state ID

**Response:** Updated `GameState` object after AI moves

**Process:**
1. Set state to "EnemyTurn"
2. Execute AI logic for all enemy units
3. Reset all unit "hasActed" flags
4. Increment turn counter
5. Check win/loss conditions
6. Set state to "PlayerTurn" or "GameOver"/"Victory"

**Example:**
```bash
curl -X POST "http://localhost:5000/api/Game/endturn/1"
```

---

### Delete Game State

Deletes a game state.

**Endpoint:** `DELETE /api/Game/{id}`

**Path Parameters:**
- `id` (integer): Game state ID

**Response:** `boolean` (true if successful)

**Example:**
```bash
curl -X DELETE "http://localhost:5000/api/Game/1"
```

---

## Save/Load System

### Save Game

Saves current game state with serialization.

**Endpoint:** `POST /api/Save/save`

**Request Body:** `GameState` object

**Response:** `boolean` (true if successful)

**Example:**
```bash
curl -X POST "http://localhost:5000/api/Save/save" \
  -H "Content-Type: application/json" \
  -d @gamestate.json
```

---

### Load Game

Loads most recent saved game for player.

**Endpoint:** `GET /api/Save/load/{playerId}`

**Path Parameters:**
- `playerId` (string): Player identifier

**Response:** `GameState` object or 404

**Example:**
```bash
curl -X GET "http://localhost:5000/api/Save/load/Player1"
```

---

### Get All Saves

Retrieves all saved games for a player.

**Endpoint:** `GET /api/Save/all/{playerId}`

**Path Parameters:**
- `playerId` (string): Player identifier

**Response:** Array of `GameState` objects

**Example:**
```bash
curl -X GET "http://localhost:5000/api/Save/all/Player1"
```

---

### Delete Save

Deletes a specific save.

**Endpoint:** `DELETE /api/Save/{gameStateId}`

**Path Parameters:**
- `gameStateId` (integer): Game state ID

**Response:** `boolean` (true if successful)

**Example:**
```bash
curl -X DELETE "http://localhost:5000/api/Save/1"
```

---

## Leaderboard

### Add Leaderboard Entry

Adds a new leaderboard entry.

**Endpoint:** `POST /api/Leaderboard/add`

**Request Body:**
```json
{
  "playerName": "Player1",
  "score": 1500,
  "turnsCompleted": 25,
  "completionTime": "00:15:30"
}
```

**Response:** Created `LeaderboardEntry` object

**Example:**
```bash
curl -X POST "http://localhost:5000/api/Leaderboard/add" \
  -H "Content-Type: application/json" \
  -d '{"playerName":"Player1","score":1500,"turnsCompleted":25,"completionTime":"00:15:30"}'
```

---

### Get Top Scores

Retrieves top scores.

**Endpoint:** `GET /api/Leaderboard/top/{count}`

**Path Parameters:**
- `count` (integer, optional): Number of entries. Default: 10

**Response:** Array of `LeaderboardEntry` objects ordered by score (desc)

**Example:**
```bash
curl -X GET "http://localhost:5000/api/Leaderboard/top/10"
```

---

### Get Fastest Completions

Retrieves fastest game completions.

**Endpoint:** `GET /api/Leaderboard/fastest/{count}`

**Path Parameters:**
- `count` (integer, optional): Number of entries. Default: 10

**Response:** Array of `LeaderboardEntry` objects ordered by completion time (asc)

**Example:**
```bash
curl -X GET "http://localhost:5000/api/Leaderboard/fastest/10"
```

---

### Get Player Rank

Gets a player's rank on the leaderboard.

**Endpoint:** `GET /api/Leaderboard/rank/{playerName}`

**Path Parameters:**
- `playerName` (string): Player name

**Response:** `integer` (rank position, 1-indexed) or -1 if not found

**Example:**
```bash
curl -X GET "http://localhost:5000/api/Leaderboard/rank/Player1"
```

---

## Data Models

### GameState

```typescript
{
  id: number,
  playerId: string,
  currentTurn: number,
  score: number,
  state: "PlayerTurn" | "EnemyTurn" | "Processing" | "GameOver" | "Victory",
  lastUpdated: string (ISO 8601),
  serializedGameData?: string,
  units: UnitState[]
}
```

### UnitState

```typescript
{
  id: number,
  gameStateId: number,
  unitName: string,
  isPlayerUnit: boolean,
  currentHealth: number,
  maxHealth: number,
  attack: number,
  defense: number,
  moveRange: number,
  attackRange: number,
  positionQ: number,
  positionR: number,
  positionS: number,
  hasActed: boolean
}
```

### HexPosition

```typescript
{
  q: number,  // Axial coordinate Q
  r: number,  // Axial coordinate R
  s: number   // Axial coordinate S (q + r + s = 0)
}
```

### CombatResult

```typescript
{
  success: boolean,
  damageDealt: number,
  targetDestroyed: boolean,
  message: string
}
```

### LeaderboardEntry

```typescript
{
  id: number,
  playerName: string,
  score: number,
  turnsCompleted: number,
  completedAt: string (ISO 8601),
  completionTime: string (TimeSpan format)
}
```

---

## Error Responses

All endpoints return standard HTTP status codes:

**200 OK** - Success
```json
{ "result": "..." }
```

**400 Bad Request** - Invalid input
```json
{ "message": "Invalid move" }
```

**404 Not Found** - Resource not found
```json
{ "message": "Game state with ID 123 not found" }
```

**500 Internal Server Error** - Server error
```json
{ "message": "Error processing request" }
```

---

## Rate Limiting

Currently not implemented. For production, consider:
- 100 requests per minute per IP
- 1000 requests per hour per player

---

## Versioning

Current version: **v1**

Future versions will use URL versioning:
- `http://localhost:5000/api/v1/Game/new`
- `http://localhost:5000/api/v2/Game/new`

---

## Swagger UI

Interactive API documentation available at:
`http://localhost:5000/swagger`

Features:
- Try endpoints directly
- View request/response schemas
- Download OpenAPI spec

---

## Example Workflows

### Complete Game Flow

```bash
# 1. Create new game
GAME_ID=$(curl -X POST "http://localhost:5000/api/Game/new?playerId=Player1" | jq -r '.id')

# 2. Move player unit
curl -X POST "http://localhost:5000/api/Game/move" \
  -H "Content-Type: application/json" \
  -d "{\"gameStateId\":$GAME_ID,\"unitId\":1,\"targetPosition\":{\"q\":2,\"r\":2,\"s\":-4}}"

# 3. Attack enemy
curl -X POST "http://localhost:5000/api/Game/attack" \
  -H "Content-Type: application/json" \
  -d "{\"gameStateId\":$GAME_ID,\"attackerUnitId\":1,\"targetUnitId\":3}"

# 4. End turn (AI moves)
curl -X POST "http://localhost:5000/api/Game/endturn/$GAME_ID"

# 5. Save game
curl -X POST "http://localhost:5000/api/Save/save" \
  -H "Content-Type: application/json" \
  -d @current_game_state.json

# 6. Add to leaderboard on victory
curl -X POST "http://localhost:5000/api/Leaderboard/add" \
  -H "Content-Type: application/json" \
  -d '{"playerName":"Player1","score":1500,"turnsCompleted":25,"completionTime":"00:15:30"}'
```

---

## WebSocket Support (Future)

For real-time multiplayer, consider adding SignalR:

```csharp
// Hub endpoint
/gamehub

// Events
OnPlayerMoved
OnUnitAttacked
OnGameStateChanged
OnPlayerJoined
OnPlayerLeft
```

---

## Notes

- All coordinates use hexagonal axial system (q, r, s)
- Distance formula: `(|q1-q2| + |r1-r2| + |s1-s2|) / 2`
- Combat damage: `max(1, attack - defense)`
- Turn order: Player → AI → repeat
- Game state persists in SQLite database

