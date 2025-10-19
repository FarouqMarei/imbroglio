# Imbroglio Combat - Mobile Hex-Based Strategy Game

A single-player, mobile-friendly, turn-based strategy game built with Unity (C#) for the frontend and .NET Core for the backend. Features hexagonal grid combat, AI opponents, and persistent game state.

## 🎮 Features

### Game Mechanics
- **Hexagonal Grid System**: Strategic battlefield with hex-based movement
- **Turn-Based Combat**: Plan your moves and execute tactical attacks
- **Unit Management**: Player and enemy units with health, attack, defense stats
- **AI Opponents**: Smart enemy AI with pathfinding and tactical decisions
- **Mobile-Optimized**: Touch controls with zoom, pan, and intuitive tap selection

### Gameplay Features
- Move units across hex tiles with range limitations
- Attack enemies within range
- Visual feedback for selected units, movement range, and attack range
- Undo last move functionality
- Game state save/load system
- Score tracking and leaderboard

### Backend Features (.NET Core API)
- RESTful API for game state management
- SQLite database for persistence
- Game logic calculations (combat, damage, AI moves)
- Save/Load game progress
- Leaderboard system for high scores

## 📁 Project Structure

```
.
├── Unity/ImbroglioCombat/           # Unity Frontend Project
│   ├── Assets/
│   │   └── Scripts/
│   │       ├── Core/                # Core game systems
│   │       │   ├── HexTile.cs
│   │       │   ├── HexGrid.cs
│   │       │   └── GameManager.cs
│   │       ├── Units/               # Unit classes
│   │       │   ├── Unit.cs
│   │       │   ├── PlayerUnit.cs
│   │       │   └── EnemyUnit.cs
│   │       ├── AI/                  # AI and pathfinding
│   │       │   ├── AIController.cs
│   │       │   └── Pathfinder.cs
│   │       ├── UI/                  # UI components
│   │       │   ├── GameUI.cs
│   │       │   └── HealthBar.cs
│   │       ├── Input/               # Input handling
│   │       │   └── InputManager.cs
│   │       └── Backend/             # Backend communication
│   │           └── BackendClient.cs
│   └── Packages/
│       └── manifest.json
│
└── Backend/ImbroglioCombatAPI/      # .NET Core Backend
    ├── Controllers/                 # API endpoints
    │   ├── GameController.cs
    │   ├── SaveController.cs
    │   └── LeaderboardController.cs
    ├── Services/                    # Business logic
    │   ├── GameService.cs
    │   ├── AIService.cs
    │   ├── SaveService.cs
    │   └── LeaderboardService.cs
    ├── Models/                      # Data models
    │   ├── GameState.cs
    │   └── DTOs.cs
    ├── Data/                        # Database context
    │   └── GameDbContext.cs
    ├── Program.cs
    ├── appsettings.json
    └── ImbroglioCombatAPI.csproj
```

## 🚀 Getting Started

### Prerequisites

**For Unity Frontend:**
- Unity 2021.3 or later
- TextMeshPro package
- Newtonsoft.Json for Unity

**For .NET Backend:**
- .NET 8.0 SDK or later
- Visual Studio 2022 or Visual Studio Code

### Backend Setup

1. Navigate to the backend directory:
```bash
cd Backend/ImbroglioCombatAPI
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Run the backend API:
```bash
dotnet run
```

The API will start on `http://localhost:5000` (or `https://localhost:5001`)

4. Access Swagger UI for API documentation:
```
http://localhost:5000/swagger
```

### Unity Frontend Setup

1. Open Unity Hub
2. Click "Open" and select the `Unity/ImbroglioCombat` folder
3. Wait for Unity to import all assets
4. Install Newtonsoft.Json for Unity:
   - Download from Unity Asset Store or add via Package Manager
5. Open the main scene (create if not exists)
6. Configure the `BackendClient` script:
   - Set `baseUrl` to your backend URL (default: `http://localhost:5000/api`)
7. Press Play to test

### Building for Mobile

**Android:**
1. File → Build Settings → Android
2. Switch Platform
3. Player Settings:
   - Set Package Name
   - Set Minimum API Level (API 21 or higher recommended)
4. Build and Run

**iOS:**
1. File → Build Settings → iOS
2. Switch Platform
3. Player Settings:
   - Set Bundle Identifier
   - Set Target SDK
4. Build and open in Xcode

## 🎯 How to Play

1. **Start Game**: Launch the game to begin a new session
2. **Select Unit**: Tap on your unit (blue) to select it
3. **View Ranges**: 
   - Blue highlights show movement range
   - Red highlights show attack range
4. **Move**: Tap an empty blue-highlighted tile to move
5. **Attack**: Tap an enemy unit in red-highlighted range to attack
6. **End Turn**: Press "End Turn" button to let enemies move
7. **Undo**: Press "Undo" to revert your last action (limited uses)

### Win/Lose Conditions
- **Victory**: Eliminate all enemy units
- **Defeat**: All your units are destroyed

## 🔧 API Endpoints

### Game Management
- `POST /api/Game/new` - Create new game
- `GET /api/Game/{id}` - Get game state
- `GET /api/Game/latest/{playerId}` - Get latest game for player
- `PUT /api/Game/update` - Update game state
- `POST /api/Game/move` - Process unit movement
- `POST /api/Game/attack` - Process attack
- `POST /api/Game/endturn/{id}` - End player turn (triggers AI)
- `DELETE /api/Game/{id}` - Delete game

### Save/Load
- `POST /api/Save/save` - Save game
- `GET /api/Save/load/{playerId}` - Load game
- `GET /api/Save/all/{playerId}` - Get all saves
- `DELETE /api/Save/{id}` - Delete save

### Leaderboard
- `POST /api/Leaderboard/add` - Add leaderboard entry
- `GET /api/Leaderboard/top/{count}` - Get top scores
- `GET /api/Leaderboard/fastest/{count}` - Get fastest completions
- `GET /api/Leaderboard/rank/{playerName}` - Get player rank

## 🧩 Key Systems

### Hexagonal Grid
- Uses axial coordinate system (q, r, s)
- Flat-top hexagon orientation
- Distance calculation: `(|q1-q2| + |r1-r2| + |s1-s2|) / 2`

### AI System
- Three AI behaviors: Aggressive, Defensive, Balanced
- A* pathfinding for optimal routes
- Target selection based on proximity
- Move calculation considers terrain and occupied tiles

### Combat System
- Damage formula: `max(1, attacker.attack - defender.defense)`
- Range-based attacks
- Health tracking with visual feedback
- Unit destruction and score rewards

### Undo System
- Stack-based state snapshots
- Limited undo history (default: 3 moves)
- Preserves unit positions and health

## 📱 Mobile Optimizations

- **Touch Controls**: Single tap to select, drag to pan camera
- **Pinch to Zoom**: Two-finger zoom on mobile
- **Responsive UI**: Adapts to different screen sizes
- **Performance**: Optimized for mobile GPUs
- **Battery Efficient**: Turn-based gameplay reduces constant rendering

## 🛠️ Customization

### Adding New Units
1. Create new class inheriting from `Unit`
2. Set custom stats in `Initialize()` method
3. Add to spawn system in `GameManager`

### Creating New Tile Types
1. Add to `TileType` enum in `HexTile.cs`
2. Create prefab variant
3. Update `DetermineTileType()` in `HexGrid.cs`

### Modifying AI Behavior
- Edit `AIController.cs` to change enemy tactics
- Adjust difficulty by modifying stats in `GameService.cs`

### Custom Map Generation
- Modify `GenerateGrid()` and `DetermineTileType()` in `HexGrid.cs`
- Implement procedural generation algorithms
- Load maps from external files

## 🐛 Troubleshooting

**Backend won't start:**
- Ensure .NET 8.0 SDK is installed
- Check port 5000/5001 is not in use
- Verify SQLite is supported on your system

**Unity can't connect to backend:**
- Check `baseUrl` in `BackendClient.cs`
- Ensure backend is running
- Check firewall settings
- For Android builds, add Internet permission

**Units not moving:**
- Check if unit is selected
- Verify target tile is in range
- Ensure tile is walkable and not occupied

**Touch controls not working:**
- Ensure EventSystem is in scene
- Check Camera is tagged as "MainCamera"
- Verify Input System package is installed

## 📝 Future Enhancements

- [ ] Multiple unit types (archer, tank, healer)
- [ ] Special abilities and cooldowns
- [ ] Terrain effects (water, mountains)
- [ ] Campaign mode with story
- [ ] Multiplayer support
- [ ] Custom map editor
- [ ] Sound effects and music
- [ ] Particle effects for attacks
- [ ] Achievement system
- [ ] Daily challenges

## 📄 License

This project is provided as-is for educational and portfolio purposes.

## 🤝 Contributing

Feel free to fork this project and submit pull requests with improvements!

## 📧 Contact

For questions or feedback, please open an issue on GitHub.

---

**Built with ❤️ using Unity and .NET Core**

