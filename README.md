# Entry Points Package

A Unity package that provides a flexible system for managing different entry points and configurations in your game, with editor toolbar integration for easy switching between configurations.

## Features

- **Toolbar Dropdown**: Select entry points directly from the Unity toolbar
- **Editor & Build Support**: Different configurations for editor vs builds
- **Extensible**: Create custom entry point configurations for different scenarios
- **Scene Management**: Built-in support for startup scene configuration
- **Clean Architecture**: Interface-based design for maximum flexibility

## Quick Start

### 1. Create an Entry Point

Create a startup scene launcher asset:

**Right-click in Project** → **Create** → **DGP** → **Startup Scene Launcher**

Assign your startup scene in the inspector.

### 2. Select Entry Point

Use the dropdown in the Unity toolbar (left side) to select your entry point. The dropdown includes:
- **[Current Scene]** - Uses whatever scene is currently open (default)
- Your created entry point configurations

### 3. Implement Your Bootstrapper (optional)

Create a bootstrapper class to initialize your game:

```csharp
using DGP.EntryPoints;
using UnityEngine;

public class GameBootstrapper : EntryPointBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        new GameBootstrapper().Bootstrap();
    }

    protected override IEntryPoint GetDefaultConfiguration()
    {
        // Return the default config for builds (typically from Resources)
        return Resources.Load<StartupSceneLauncherSO>("DefaultLaunchConfig");
    }

    public override void Bootstrap()
    {
        var config = GetActiveConfiguration();
        
        Debug.Log($"Bootstrapping with: {config.DisplayName}");
        
        // Call Bootstrap on the config (if it does custom initialization)
        config.Bootstrap();
        
        // Your game-specific initialization here
        // ServiceLocator.RegisterService<IPersistenceService>(new PersistenceService());
        // ServiceLocator.RegisterService<GameCoordinator>(new GameCoordinator());
        // etc.
    }
}
```

## Core Components

### IEntryPoint

The base interface for all entry point configurations:

```csharp
public interface IEntryPoint
{
    string DisplayName { get; }           // Name shown in toolbar dropdown
    void OnEntryPointSelected();          // Called when selected in editor
    void Bootstrap();                     // Called to initialize the game
}
```

### StartupSceneLauncherSO

A built-in ScriptableObject implementation that handles scene-based entry points:

- Drag and drop scene assignment in inspector
- Automatically sets the play mode start scene when selected
- Validation to ensure assigned scene exists

### EntryPointBootstrapper

Abstract base class for your game's bootstrapper:

- `GetActiveConfiguration()` - Returns the selected config (editor) or default (builds)
- `GetDefaultConfiguration()` - Override to provide your default config
- `Bootstrap()` - Override to implement your game initialization

### EntryPoints

Static accessor for the currently active entry point:

```csharp
var activeConfig = EntryPoints.ActiveEntryPoint;
```

## Creating Custom Entry Points

You can create custom entry point types by implementing `IEntryPoint`:

```csharp
using DGP.EntryPoints;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomEntryPoint", menuName = "Game/Custom Entry Point")]
public class CustomEntryPointSO : ScriptableObject, IEntryPoint
{
    [SerializeField] private GameMode gameMode;
    [SerializeField] private int startingLevel;
    
    public string DisplayName => name;
    
    public void OnEntryPointSelected()
    {
        // Called when selected in editor
        Debug.Log($"Selected {name} with mode {gameMode}");
    }
    
    public void Bootstrap()
    {
        // Initialize game systems with this configuration
        GameManager.Instance.SetGameMode(gameMode);
        GameManager.Instance.LoadLevel(startingLevel);
    }
}
```

## Use Cases

### Multiple Game Modes

Create different entry points for:
- Main menu flow
- Debug/testing scenarios
- Specific level testing
- Different game modes (single player, multiplayer, etc.)

### Team Workflows

Each team member can:
- Create their own entry point for their work
- Switch between configurations without affecting others
- Test specific scenarios quickly

### Testing & QA

- Create entry points that skip to specific game states
- Configure different test scenarios
- Quickly reproduce bugs in specific configurations

## Editor Behavior

In the Unity Editor:
- Selected entry point is stored in `EditorPrefs`
- Persists across editor sessions
- Each project can have its own selected entry point
- `[Current Scene]` option preserves the currently open scene

## Build Behavior

In builds:
- Editor selection is ignored
- `GetDefaultConfiguration()` provides the configuration
- Place your default config in a `Resources` folder to load it

## Package Structure

```
EntryPoints/
├── Runtime/
│   ├── IEntryPoint.cs                    # Core interface
│   ├── EntryPoints.cs                    # Static accessor
│   ├── EntryPointBootstrapper.cs         # Abstract bootstrapper base
│   └── StartupSceneLauncherSO.cs         # Built-in scene launcher
└── Editor/
    ├── PlayModeOptions.cs                # Toolbar integration
    └── StartupSceneLauncherSOEditor.cs   # Custom inspector (optional)
```

## Requirements

- Unity 2021.1 or newer
- No external dependencies