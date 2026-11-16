# AGENTS.md — context for AI coding tools

## The game

Super Maze is a hybrid-casual mobile game set in a supermarket designed as a maze.

In each level, the player navigates the supermarket using a virtual joystick, collects all items from a predefined shopping list by approaching or touching them, and reaches the checkout before time runs out.

Obstacles such as customers blocking aisles, falling products, and power outages make completing each level more challenging.

Players receive 1–3 stars based on their completion time. Completing a level with 3 stars earns more coins than completing it with 2 stars, while 1 star allows progression without a coin reward.

Coins can be used to purchase power-ups before starting a level. Players choose whether and when to use them. Power-ups include extra time, an automatically collected item at the beginning of a level, and an additional attempt after failure.

The player has a shared pool of 3 lives across all levels. Failed attempts consume lives, which regenerate over time.

The game is designed for short sessions, with a 2-minute time limit per level that can be extended using power-ups. Players can complete levels faster depending on their performance. The game is played in portrait orientation using one-thumb controls.

## Tech

- Engine: Unity 6 LTS
- Programming language: C#
- Target: Mobile, portrait orientation
- Final target platforms: Android, ios
- orientation: Portrait
- UI: Unity UI (uGUI) and TextMeshPro
- Input: Unity Input System
  
## Architecture

### Scenes and initialization

- Scenes are stored in Assets/Scenes.
- Bootstrap is the first scene in the Scene List.
- Bootstrap creates the save service before loading Home:
  Services.Save = new PlayerPrefsSaveService();
- The Bootstrap GameObject uses DontDestroyOnLoad to survive scene changes.
- Home and Level use ScreenFlow for navigation.
- Normal navigation must not reload Bootstrap.

### Services and saving

- Service code is stored in Assets/Scripts/Services.
- ISaveService defines the saving interface: GetInt and SetInt.
- PlayerPrefsSaveService implements ISaveService using PlayerPrefs.
- Services exposes the active save service through Services.Save.
- Game and UI scripts access saved data through Services.Save.
- Direct PlayerPrefs access belongs inside the save service implementation.
- CoinCounter displays and updates the saved coin count.

### Assemblies

- Game code is stored in Assets/Scripts/Game.
- Game.asmdef references Services, Unity.TextMeshPro and Unity.InputSystem.
- Services.asmdef does not reference Game.
- Services must not depend on gameplay or UI classes.

### Current scope

- The save service is implemented.
- A second service is planned and must be documented here once implemented.

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
