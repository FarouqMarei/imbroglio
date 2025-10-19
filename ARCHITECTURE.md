# Imbroglio Combat - Architecture Documentation

## System Overview

Imbroglio Combat is a client-server architecture game with Unity handling the presentation layer and .NET Core managing game logic and persistence.

```
┌─────────────────────────────────────────────────────────────┐
│                         Unity Client                         │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │    Input    │  │   Rendering  │  │   Game State     │   │
│  │  Management │→ │   & Visual   │← │   Management     │   │
│  └─────────────┘  └──────────────┘  └──────────────────┘   │
│         ↓                                      ↑              │
│  ┌─────────────────────────────────────────────────────┐    │
│  │            Backend Client (HTTP/REST)               │    │
│  └─────────────────────────────────────────────────────┘    │
└────────────────────────────┬────────────────────────────────┘
                             │ HTTPS/JSON
                             ↓
┌─────────────────────────────────────────────────────────────┐
│                    .NET Core Backend API                     │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │ Controllers │→ │   Services   │→ │   Data Layer     │   │
│  │  (REST API) │  │ (Game Logic) │  │  (EF Core/DB)    │   │
│  └─────────────┘  └──────────────┘  └──────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Unity Frontend Architecture

### Layer Structure

```
Unity Client
├── Presentation Layer (MonoBehaviours)
│   ├── UI Components (Canvas, Buttons, Text)
│   ├── Visual Feedback (Sprites, Animations)
│   └── Input Handlers (Touch, Mouse)
│
├── Game Logic Layer
│   ├── GameManager (State Machine)
│   ├── HexGrid (Map Management)
│   ├── Units (Player & Enemy)
│   └── AI Controller
│
└── Communication Layer
    └── BackendClient (API Integration)
```

### Key Components

#### 1. Core Systems

**GameManager.cs**
- Central state machine
- Manages game flow (PlayerTurn → EnemyTurn → Processing)
- Coordinates between systems
- Handles undo/redo
- Singleton pattern

**HexGrid.cs**
- Hexagonal coordinate system (Axial coordinates)
- Tile generation and management
- Neighbor calculation
- Distance calculations
- Pathfinding support

**HexTile.cs**
- Individual tile representation
- Occupancy tracking
- Visual feedback (highlights)
- Tile type management

#### 2. Unit System

**Unit.cs** (Abstract Base)
- Common unit functionality
- Stats management
- Movement and combat
- Health tracking
- Animation hooks

**PlayerUnit.cs**
- Player-controlled units
- Experience/leveling system
- Special abilities
- Input handling

**EnemyUnit.cs**
- AI-controlled units
- Different AI behaviors
- Attack patterns

#### 3. AI System

**AIController.cs**
- AI decision making
- Turn execution
- Multiple AI behaviors:
  - Aggressive: Always attack
  - Defensive: Protect position
  - Balanced: Tactical decisions

**Pathfinder.cs**
- A* pathfinding algorithm
- Reachable tiles calculation
- Obstacle avoidance

#### 4. Input System

**InputManager.cs**
- Multi-platform input (touch/mouse)
- Gesture recognition
- Camera controls (pan, zoom)
- Input abstraction layer

#### 5. UI System

**GameUI.cs**
- UI state management
- Button handling
- Game over screens
- Notifications

**HealthBar.cs**
- Dynamic health display
- Color gradients
- World-space UI

### Design Patterns Used

1. **Singleton**: GameManager, AudioManager
2. **State Machine**: GameState enum with transitions
3. **Observer**: UI updates on state changes
4. **Object Pool**: For performance (particles, effects)
5. **Command**: For undo/redo system
6. **Factory**: Unit creation

## Backend Architecture

### Layer Structure

```
.NET Core Backend
├── API Layer (Controllers)
│   ├── GameController
│   ├── SaveController
│   └── LeaderboardController
│
├── Business Logic Layer (Services)
│   ├── GameService (Core game logic)
│   ├── AIService (AI calculations)
│   ├── SaveService (Persistence)
│   └── LeaderboardService (Scoring)
│
├── Data Layer
│   ├── DbContext (EF Core)
│   ├── Models (Entities)
│   └── DTOs (Data Transfer Objects)
│
└── Infrastructure
    ├── Dependency Injection
    ├── CORS Configuration
    └── Swagger/OpenAPI
```

### Key Components

#### 1. Controllers (API Endpoints)

**GameController.cs**
- `POST /api/Game/new` - Create new game
- `GET /api/Game/{id}` - Get game state
- `POST /api/Game/move` - Process move
- `POST /api/Game/attack` - Process attack
- `POST /api/Game/endturn/{id}` - End turn, trigger AI

**SaveController.cs**
- `POST /api/Save/save` - Save game
- `GET /api/Save/load/{playerId}` - Load game
- `GET /api/Save/all/{playerId}` - Get all saves

**LeaderboardController.cs**
- `POST /api/Leaderboard/add` - Add entry
- `GET /api/Leaderboard/top/{count}` - Top scores
- `GET /api/Leaderboard/fastest/{count}` - Fastest times

#### 2. Services (Business Logic)

**GameService.cs**
- Game state management
- Combat calculations
- Turn management
- Win/loss conditions
- Game creation with initial units

**AIService.cs**
- AI turn execution
- Target selection
- Move calculation
- Tactical decisions
- Pathfinding helpers

**SaveService.cs**
- Game serialization
- Database operations
- Save/load management

**LeaderboardService.cs**
- Score ranking
- Time tracking
- Player rankings

#### 3. Data Models

**GameState**
- Game metadata (turn, score, state)
- Player identification
- Timestamp
- Serialized game data

**UnitState**
- Unit properties (health, attack, defense)
- Position (hex coordinates)
- Status flags
- Foreign key to GameState

**LeaderboardEntry**
- Player name
- Score
- Completion time
- Timestamp

### Design Patterns Used

1. **Repository Pattern**: DbContext abstracts data access
2. **Service Layer Pattern**: Business logic separated from controllers
3. **DTO Pattern**: Separate models for API and database
4. **Dependency Injection**: All services injected via DI container
5. **Unit of Work**: DbContext manages transactions

## Communication Protocol

### Request/Response Flow

```
Unity → Backend: HTTP POST /api/Game/move
{
  "gameStateId": 1,
  "unitId": 5,
  "targetPosition": { "q": 3, "r": 2, "s": -5 }
}

Backend Processing:
1. Validate game state exists
2. Validate unit exists and belongs to player
3. Check move is legal (range, obstacles)
4. Update unit position
5. Save to database

Backend → Unity: HTTP 200 OK
{
  "success": true
}
```

### Data Synchronization

**Optimistic Updates:**
- Unity updates UI immediately
- Backend validates and confirms
- On conflict, Unity reverts to backend state

**Turn-Based Sync:**
- Player actions: Unity → Backend immediately
- AI actions: Backend calculates → Unity receives full state

## Database Schema

```sql
GameStates
├── Id (PK)
├── PlayerId
├── CurrentTurn
├── Score
├── State
├── LastUpdated
└── SerializedGameData

UnitStates
├── Id (PK)
├── GameStateId (FK)
├── UnitName
├── IsPlayerUnit
├── CurrentHealth
├── MaxHealth
├── Attack
├── Defense
├── MoveRange
├── AttackRange
├── PositionQ
├── PositionR
├── PositionS
└── HasActed

LeaderboardEntries
├── Id (PK)
├── PlayerName
├── Score
├── TurnsCompleted
├── CompletedAt
└── CompletionTime
```

## Hexagonal Grid System

### Coordinate System

Uses **Axial Coordinates** (q, r, s) where q + r + s = 0

```
     (0,1,-1)   (1,0,-1)
         \     /
    (-1,1,0)-(0,0,0)-(1,-1,0)
         /     \
    (-1,0,1)   (0,-1,1)
```

### Distance Calculation

```csharp
distance = (|q1 - q2| + |r1 - r2| + |s1 - s2|) / 2
```

### Neighbor Finding

Six directions:
- East: (+1, 0, -1)
- Southeast: (+1, -1, 0)
- Southwest: (0, -1, +1)
- West: (-1, 0, +1)
- Northwest: (-1, +1, 0)
- Northeast: (0, +1, -1)

### World Position Conversion

```csharp
// Hex to World (flat-top)
x = hexSize * (3/2 * q)
y = hexSize * (√3/2 * q + √3 * r)

// World to Hex
q = (2/3 * x) / hexSize
r = (-1/3 * x + √3/3 * y) / hexSize
```

## Game State Flow

```
Start Game
    ↓
Initialize (Create units, grid)
    ↓
PlayerTurn
    ├→ Select Unit
    ├→ Move/Attack
    ├→ Undo (optional)
    └→ End Turn
        ↓
    EnemyTurn
        ├→ AI calculates moves
        ├→ Execute AI actions
        └→ Check win/loss
            ↓
        PlayerTurn (repeat)

Win/Loss Condition Check:
- No player units → GameOver
- No enemy units → Victory
```

## Performance Considerations

### Unity Optimizations

1. **Object Pooling**: Reuse game objects
2. **Sprite Atlasing**: Reduce draw calls
3. **Lazy Loading**: Load assets on demand
4. **Culling**: Only render visible tiles
5. **Batching**: Static batching for tiles

### Backend Optimizations

1. **EF Core Tracking**: No-tracking queries for read-only
2. **Caching**: In-memory cache for active games
3. **Async/Await**: All I/O operations async
4. **Connection Pooling**: Database connections
5. **Indexing**: Database indexes on frequently queried columns

## Security Considerations

1. **Input Validation**: All API inputs validated
2. **SQL Injection**: EF Core parameterized queries
3. **CORS**: Configured for Unity client only
4. **Rate Limiting**: (To be implemented)
5. **Authentication**: (To be implemented for production)

## Scalability

### Current Limitations
- Single-player only
- SQLite (single-file database)
- No real-time sync

### Future Scalability Path
1. Add PostgreSQL/SQL Server for multi-user
2. Implement SignalR for real-time multiplayer
3. Add Redis for caching
4. Horizontal scaling with load balancer
5. CDN for Unity WebGL builds

## Testing Strategy

### Unity Testing
- Play mode tests for game logic
- Edit mode tests for utilities
- Manual testing on devices

### Backend Testing
- Unit tests for services
- Integration tests for API
- Database tests with in-memory provider

### End-to-End Testing
- Unity Test Framework
- Backend API tests with real client

## Deployment

### Unity Build
- Android: APK/AAB
- iOS: Xcode project
- WebGL: Static hosting
- Desktop: Standalone executables

### Backend Deployment
- Docker container
- Azure App Service
- AWS Elastic Beanstalk
- Self-hosted Linux server

## Monitoring & Logging

### Unity
- Debug.Log for development
- Analytics (Unity Analytics, optional)
- Crash reporting (Crashlytics, optional)

### Backend
- ASP.NET Core logging
- Application Insights (optional)
- Error tracking (Sentry, optional)

---

This architecture is designed to be:
- **Modular**: Easy to extend
- **Testable**: Clear separation of concerns
- **Scalable**: Can grow from single-player to multiplayer
- **Maintainable**: Clean code structure

