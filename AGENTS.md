# AGENTS.md — context for AI coding tools

## The game

Super Maze is a hybrid-casual mobile game set in a supermarket designed as a maze.

In each level, the player navigates the supermarket using a virtual joystick, collects all items from a predefined shopping list by approaching or touching them, and reaches the checkout before time runs out.

Obstacles such as customers blocking aisles, falling products, and power outages make completing each level more challenging.

Players receive 1–3 stars based on their completion time. Completing a level with 3 stars earns more coins than completing it with 2 stars, while 1 star allows progression without a coin reward.

Before starting a level, players can spend coins in the shop to buy power-ups: 10 extra seconds, increased movement speed, and starting with one shopping-list item already collected. Players can select up to one power-up of each type per attempt and use different types in the same attempt. Each used power-up consumes one unit.

The player has a shared pool of 3 lives across all levels. Each failed attempt immediately costs one life. Retrying starts the level from the beginning and does not deduct another life. Players can retry if they have lives remaining; otherwise, they are directed to the shop, where they can buy lives. Lost lives regenerate over time. Lives can only be purchased outside an active level. There is no option to continue a failed attempt from where it ended.

The game is designed for short sessions, with a 2-minute time limit per level that can be extended using power-ups. Players can complete levels faster depending on their performance. The game is played in portrait orientation using one-thumb controls.

## Tech

- Engine: Unity 6000.6.7f1 (see SuperMaze/ProjectSettings/ProjectVersion.txt).
- Programming language: C#
- Final target platforms: Android
- orientation: Portrait
- UI: Unity UI (uGUI) and TextMeshPro
- Input: Unity Input System

## Architecture

### Scenes and initialization

- Scenes are stored in Assets/Scenes.
- Bootstrap, Home and Level are enabled in the Scene List, in that order.
- Bootstrap is the application entry scene. Its Services GameObject has the Bootstrap component and uses DontDestroyOnLoad to survive scene changes.
- Bootstrap creates the save service first, then loads Home:
  Services.Save = new PlayerPrefsSaveService();
- Home is the current menu and level-selection screen.
- Level is the current gameplay UI and supermarket layout screen.
- level1 is a separate supermarket-layout prototype and is not in the Scene List.
- SampleScene is disabled in the Scene List and is not part of the normal application flow.
- Home → Level uses ScreenFlow.Play, and Level → Home uses ScreenFlow.Back.
- Normal navigation must not reload Bootstrap.
- In the Unity Editor, Assets/Editor/PlayFromBootstrap.cs makes Play Mode start from Bootstrap even when another scene is open.

### Services and saving

- Service code is stored in Assets/Scripts/Services.
- ISaveService defines the saving interface: GetInt and SetInt.
- PlayerPrefsSaveService implements ISaveService using PlayerPrefs.
- Services exposes the active save service through Services.Save.
- All game, screen and UI code must access saved data through Services.Save, never PlayerPrefs directly.
- Direct PlayerPrefs access belongs only inside PlayerPrefsSaveService.
- CoinCounter displays and updates the persistent saved coin count.
- DailyReward stores its last claimed UTC day through Services.Save and grants 50 coins once per UTC day.

### Assemblies and script locations

- The Game assembly is defined in Assets/Scripts/Game/Game.asmdef, and its scripts are stored in Assets/Scripts/Game.
- ScreenFlow, Bootstrap, CoinCounter and DailyReward are in the Game assembly.
- LevelPopupController and SafeAreaFitter currently remain directly under Assets/Scripts and compile into the predefined Assembly-CSharp assembly.
- Service interfaces and implementations are stored in Assets/Scripts/Services and compile into the Services assembly.
- Game.asmdef references Services, Unity.TextMeshPro and Unity.InputSystem.
- Services.asmdef does not reference Game.
- The dependency direction is Game → Services. Services must not depend on gameplay or UI classes.

## Implementation status

- Implemented: Bootstrap-first startup, PlayerPrefs-backed integer saving through ISaveService, persistent coins, the daily reward, the Home level-selection popup, safe-area fitting, and navigation between Home and Level through ScreenFlow.
- Present as UI or scene layout but not yet implemented as complete gameplay: player movement and virtual joystick input, shopping-list item collection, checkout completion, the level timer, stars and performance rewards, level progression, obstacles, power-ups, lives and life regeneration, and failure/retry handling.
- The second service is planned but is not implemented yet. Document it here when it is implemented.

## Rules for AI

- Follow the existing folder structure and naming. Don't create new top-level folders.
- Don't invent APIs. If unsure about an SDK or Unity API, say so and point to the docs.
- Don't change Player Settings, package name, API levels or build settings unless asked.
- Keep changes small and explain what you changed and why.
- Never commit secrets (keystores, passwords, API keys).
- Follow the gameplay mechanics and rules documented in this file.
- Do not introduce new gameplay mechanics or change existing ones without approval.
- Ask for clarification when game behavior or requirements are undefined.

## Conventions

- Follow the existing C# naming and formatting style.
- Put gameplay and UI scripts in Assets/Scripts/Game.
- Put service interfaces and implementations in Assets/Scripts/Services.
- Preserve Unity .meta files when moving or renaming assets.
- Update this document when the architecture changes.

## Validation

- Start the game through Bootstrap and verify that Home opens.
- Verify that the Console has no errors.
- Verify navigation from Home to Level and back.
- Verify that adding coins updates the display.
- On Android, close and reopen the app and verify that coins persist.
- Report which checks were performed and which were not.
