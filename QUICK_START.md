# Quick Start Guide - Imbroglio Combat

Get up and running in 5 minutes!

## 🚀 Fastest Way to Test

### Step 1: Start Backend (2 minutes)

Open a terminal and run:

```bash
cd Backend/ImbroglioCombatAPI
dotnet restore
dotnet run
```

Wait for: `Now listening on: http://localhost:5000`

### Step 2: Test Backend (30 seconds)

Open browser: `http://localhost:5000/swagger`

Click "Try it out" on `/api/Game/new` → Execute

You should see a JSON response with game state.

### Step 3: Open Unity (2 minutes)

1. Open Unity Hub
2. Click "Open" → Select `Unity/ImbroglioCombat`
3. Wait for import to complete

### Step 4: Play! (30 seconds)

1. In Unity, open any scene (or create new)
2. Add required GameObjects:
   - Create Empty → Add `GameManager.cs`
   - Create Empty → Add `HexGrid.cs`
   - Create Empty → Add `InputManager.cs`
3. Press **Play**!

## 🎮 Controls

**In Unity Editor (Mouse):**
- Left Click: Select/Move/Attack
- Right Click: Deselect
- Middle Mouse Drag: Pan Camera
- Scroll Wheel: Zoom

**On Mobile:**
- Tap: Select/Move/Attack
- Drag: Pan Camera
- Pinch: Zoom

## 📋 What to Try

1. **Select a Blue Unit** (your player)
2. **Tap a Highlighted Tile** to move
3. **Tap Red Enemy** to attack
4. **Press "End Turn"** to see AI move
5. **Press "Undo"** to undo last action

## ❓ Something Not Working?

### Backend won't start?
```bash
# Install .NET 8.0 from: https://dotnet.microsoft.com/download
dotnet --version
```

### Unity has errors?
- Check that all scripts are in correct folders
- Install Newtonsoft.Json: Window → Package Manager → + → "Add package from git URL" → `com.unity.nuget.newtonsoft-json`

### Can't click anything?
- Ensure you have a Camera tagged as "MainCamera"
- Check that HexTiles have Collider2D components

## 🔥 Pro Tips

- **Test in Editor First**: Use mouse input, it's easier to debug
- **Check Console**: Unity Console (Ctrl+Shift+C) shows all errors
- **Backend Logs**: Terminal shows all API calls
- **Swagger UI**: Test backend endpoints at `http://localhost:5000/swagger`

## 📖 Next Steps

Once it's working:
1. Read full [README.md](README.md) for features
2. Check [SETUP_GUIDE.md](SETUP_GUIDE.md) for detailed setup
3. Customize units in `Backend/Services/GameService.cs`
4. Create hex tile sprites and materials
5. Build for mobile!

## 🆘 Still Stuck?

Common fixes:
```bash
# Backend: Reset database
cd Backend/ImbroglioCombatAPI
rm imbroglio_game.db
dotnet run

# Unity: Reimport all
# In Unity: Assets → Reimport All
```

**Need more help?** Check the full [SETUP_GUIDE.md](SETUP_GUIDE.md)

---

**Have fun! 🎉**

