# Imbroglio Combat - Setup Guide

This guide will help you set up and run the Imbroglio Combat game from scratch.

## Table of Contents
1. [System Requirements](#system-requirements)
2. [Backend Setup](#backend-setup)
3. [Unity Frontend Setup](#unity-frontend-setup)
4. [Running the Complete Application](#running-the-complete-application)
5. [Building for Mobile](#building-for-mobile)
6. [Troubleshooting](#troubleshooting)

## System Requirements

### For Backend Development
- **Operating System**: Windows 10/11, macOS 10.15+, or Linux
- **.NET SDK**: Version 8.0 or later
- **IDE** (choose one):
  - Visual Studio 2022 (Community Edition or higher)
  - Visual Studio Code with C# extension
  - JetBrains Rider
- **Database**: SQLite (included with EF Core)

### For Unity Development
- **Operating System**: Windows 10/11 or macOS 10.15+
- **Unity**: Version 2021.3 LTS or later
- **RAM**: Minimum 8GB (16GB recommended)
- **Storage**: 10GB free space

### For Mobile Testing
- **Android**: Android Studio with SDK API Level 21+ (for Android deployment)
- **iOS**: macOS with Xcode 12+ (for iOS deployment)

## Backend Setup

### Step 1: Install .NET SDK

#### Windows:
1. Download .NET 8.0 SDK from https://dotnet.microsoft.com/download
2. Run the installer
3. Verify installation:
```bash
dotnet --version
```

#### macOS:
```bash
brew install dotnet-sdk
dotnet --version
```

#### Linux:
```bash
# Ubuntu/Debian
wget https://dot.net/v1/dotnet-install.sh
sudo bash dotnet-install.sh --channel 8.0
dotnet --version
```

### Step 2: Navigate to Backend Directory

```bash
cd Backend/ImbroglioCombatAPI
```

### Step 3: Restore NuGet Packages

```bash
dotnet restore
```

This will download all required packages:
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Sqlite
- Swashbuckle.AspNetCore (for Swagger)
- Newtonsoft.Json

### Step 4: Create Database

The database will be created automatically on first run, but you can create it manually:

```bash
dotnet ef database update
```

If `ef` command is not found, install it:
```bash
dotnet tool install --global dotnet-ef
```

### Step 5: Run the Backend

```bash
dotnet run
```

You should see output like:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
```

### Step 6: Test the API

Open your browser and navigate to:
```
http://localhost:5000/swagger
```

You should see the Swagger UI with all API endpoints.

Test creating a new game:
```bash
curl -X POST http://localhost:5000/api/Game/new?playerId=TestPlayer
```

## Unity Frontend Setup

### Step 1: Install Unity Hub

1. Download Unity Hub from https://unity.com/download
2. Install Unity Hub
3. Sign in or create a Unity account

### Step 2: Install Unity Editor

1. Open Unity Hub
2. Go to "Installs" tab
3. Click "Add"
4. Select Unity 2021.3 LTS or later
5. Add these modules:
   - **Android Build Support** (if targeting Android)
     - Android SDK & NDK Tools
     - OpenJDK
   - **iOS Build Support** (if targeting iOS, macOS only)
   - **Windows Build Support** (if on macOS)
   - **Mac Build Support** (if on Windows)

### Step 3: Open the Unity Project

1. Open Unity Hub
2. Click "Open" or "Add"
3. Navigate to `Unity/ImbroglioCombat` folder
4. Click "Select Folder"
5. Wait for Unity to import all assets (first import may take 5-10 minutes)

### Step 4: Install Required Packages

Unity should automatically install packages from `manifest.json`, but verify:

1. In Unity, go to **Window → Package Manager**
2. Ensure these packages are installed:
   - TextMeshPro (3.0.6 or later)
   - 2D Sprite
   - 2D Tilemap
   - UI Toolkit

### Step 5: Install Newtonsoft.Json

**Method 1: Via Package Manager (Recommended)**
1. Window → Package Manager
2. Click "+" → "Add package from git URL"
3. Enter: `com.unity.nuget.newtonsoft-json`

**Method 2: Via Asset Store**
1. Download from Unity Asset Store: "JSON .NET For Unity"
2. Import into project

**Method 3: Manual**
1. Download Newtonsoft.Json for Unity from GitHub
2. Copy DLL files to `Assets/Plugins/`

### Step 6: Create Required Scene Setup

1. Create a new scene: **File → New Scene → 2D**
2. Save as `MainGame` in `Assets/Scenes/`

3. Add core objects:
   - **Create Empty GameObject**: "GameManager"
     - Add `GameManager.cs` script
   - **Create Empty GameObject**: "HexGrid"
     - Add `HexGrid.cs` script
   - **Create Empty GameObject**: "InputManager"
     - Add `InputManager.cs` script
   - **Create Empty GameObject**: "AIController"
     - Add `AIController.cs` script
   - **Create Empty GameObject**: "BackendClient"
     - Add `BackendClient.cs` script

4. Create UI Canvas:
   - Right-click in Hierarchy → **UI → Canvas**
   - Set Canvas Scaler to "Scale with Screen Size"
   - Reference Resolution: 1920x1080

### Step 7: Create Hex Tile Prefab

1. **Create → 2D Object → Sprite → Hexagon**
2. Name it "HexTile"
3. Add components:
   - `HexTile.cs` script
   - `BoxCollider2D` (or `PolygonCollider2D`)
4. Create child object "Highlight":
   - Add Sprite Renderer with semi-transparent sprite
   - Set to inactive by default
5. Drag to `Assets/Prefabs/` to create prefab
6. Delete from scene

### Step 8: Configure BackendClient

1. Select "BackendClient" GameObject
2. In Inspector, set:
   - **Base URL**: `http://localhost:5000/api`
   - **Player ID**: `Player1` (or any identifier)

### Step 9: Set Build Settings

1. **File → Build Settings**
2. Add current scene to "Scenes in Build"
3. Select your target platform:
   - **Android** for Android devices
   - **iOS** for iPhones/iPads
   - **PC/Mac/Linux** for desktop testing

## Running the Complete Application

### Step 1: Start the Backend

In terminal/command prompt:
```bash
cd Backend/ImbroglioCombatAPI
dotnet run
```

Leave this running.

### Step 2: Run Unity Editor

1. Open Unity project
2. Open `MainGame` scene
3. Click **Play** button

### Step 3: Test Integration

1. Game should create new game on backend automatically
2. Select a player unit (blue)
3. Click a tile to move
4. Click "End Turn" button
5. Enemy units should move automatically

Check Unity Console and backend terminal for logs.

## Building for Mobile

### Android Build

#### Prerequisites:
1. Install Android Studio
2. In Unity: **Edit → Preferences → External Tools**
3. Set paths for:
   - Android SDK
   - Android NDK
   - JDK

#### Build Steps:
1. **File → Build Settings → Android**
2. **Switch Platform**
3. **Player Settings**:
   - **Company Name**: Your company name
   - **Product Name**: Imbroglio Combat
   - **Package Name**: `com.yourcompany.imbrogliocombat`
   - **Minimum API Level**: Android 5.0 (API Level 21)
   - **Target API Level**: Latest
   - **Scripting Backend**: IL2CPP
   - **Target Architectures**: ARM64 (required for Play Store)
   - **Internet Access**: Required
4. **Build and Run** or **Build**

#### Testing on Device:
1. Enable Developer Options on Android device
2. Enable USB Debugging
3. Connect via USB
4. Click "Build and Run"

### iOS Build (macOS only)

#### Prerequisites:
1. Install Xcode from Mac App Store
2. Install iOS modules in Unity Hub

#### Build Steps:
1. **File → Build Settings → iOS**
2. **Switch Platform**
3. **Player Settings**:
   - **Company Name**: Your company name
   - **Product Name**: Imbroglio Combat
   - **Bundle Identifier**: `com.yourcompany.imbrogliocombat`
   - **Target Minimum iOS Version**: iOS 12.0+
   - **Architecture**: ARM64
   - **Camera Usage Description**: Not required (if not using camera)
4. **Build**

#### Post-Build (Xcode):
1. Open generated Xcode project
2. Select your Apple Developer account
3. Select target device or simulator
4. Click Run button in Xcode

## Troubleshooting

### Backend Issues

**Problem**: `dotnet: command not found`
- **Solution**: Reinstall .NET SDK, restart terminal

**Problem**: Port 5000 already in use
- **Solution**: Change port in `appsettings.json`:
```json
"Kestrel": {
  "Endpoints": {
    "Http": {
      "Url": "http://localhost:5555"
    }
  }
}
```

**Problem**: Database error on startup
- **Solution**: Delete `imbroglio_game.db` and restart

### Unity Issues

**Problem**: Scripts have compile errors
- **Solution**: Ensure all `.cs` files are in correct folders, check for syntax errors

**Problem**: "Newtonsoft.Json not found"
- **Solution**: Install Newtonsoft.Json package (see Step 5 above)

**Problem**: Scene is empty
- **Solution**: Follow Step 6-7 to create required GameObjects

**Problem**: Touch input not working in editor
- **Solution**: Use mouse input in editor, touch works on device only

### Integration Issues

**Problem**: Unity can't connect to backend
- **Solution**: 
  1. Ensure backend is running (`dotnet run`)
  2. Check `baseUrl` in BackendClient component
  3. Check firewall settings

**Problem**: API returns 404
- **Solution**: Verify backend URL and API routes

**Problem**: CORS errors in browser
- **Solution**: CORS is already configured in `Program.cs`, ensure backend is running

### Mobile Build Issues

**Android:**
- **Problem**: "SDK not found"
  - Install Android Studio, set paths in Unity Preferences

**iOS:**
- **Problem**: "Code signing required"
  - Add Apple Developer account in Xcode

## Testing Checklist

- [ ] Backend starts without errors
- [ ] Swagger UI loads at `http://localhost:5000/swagger`
- [ ] Unity opens without errors
- [ ] Play mode starts without errors
- [ ] Can select player unit
- [ ] Can move unit
- [ ] Can attack enemy
- [ ] End turn triggers AI moves
- [ ] Game state saved to backend
- [ ] Game state loads on restart

## Next Steps

- Customize unit stats in `GameService.cs`
- Create custom hex tile sprites
- Add sound effects
- Create main menu scene
- Implement tutorial

## Getting Help

- Check Unity Console for errors
- Check backend terminal for API logs
- Review `README.md` for API documentation
- Test API endpoints in Swagger UI

---

**Congratulations! Your Imbroglio Combat game should now be running!** 🎮

