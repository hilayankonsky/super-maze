# AGENTS.md — context for AI coding tools

## The game

Super Maze is a hybrid-casual mobile game set in a supermarket designed as a maze.

In each level, the player navigates the supermarket using a virtual joystick, collects all items from a predefined shopping list by approaching or touching them, and reaches the checkout before time runs out.

Obstacles such as customers blocking aisles, falling products, and power outages make completing each level more challenging.

Players earn 1–3 stars based on elapsed completion time, and coins only when they improve their best star rating for a level. Before an attempt, players can select consumable bonuses bought with coins. Permanent equipment upgrades, also bought with coins, improve movement speed and visibility during power outages. The player has a shared pool of up to 3 lives.

The game is designed for short sessions: a level normally lasts 120 seconds. The game is played in portrait orientation using one-thumb controls.

The confirmed design rules are listed below. Most of them are planned, not implemented. See "Implementation status" for what exists in code today.

## Confirmed game design (planned)

### Core loop and level time

- The player navigates the supermarket maze with a virtual joystick.
- The player must collect all required shopping-list items and reach the checkout before the timer expires.
- Each level normally lasts 120 seconds (countdown starts at 02:00).
- Successfully completing a level unlocks the next level. Failure does not unlock the next level.
- Colliding with a shopper or slipping on spilled or fallen material delays the player (see DECISIONS.md, 2026-11-04). Power outages reduce visibility.

### Stars and coin rewards

Stars depend on elapsed time since the attempt started, not on the remaining countdown:

| Elapsed time at checkout                     | Stars | Total reward for that rating |
| -------------------------------------------- | ----- | ---------------------------- |
| ≤ 40 seconds                                 | 3     | 100 coins                    |
| > 40 and ≤ 80 seconds                        | 2     | 60 coins                     |
| > 80 seconds, before the timer expires       | 1     | 20 coins                     |
| Timer expires (failure)                      | 0     | 0 coins                      |

Replay rewards:

- Store the best star rating for each level. Never reduce the saved best rating.
- A completion awards coins only when it improves the saved best rating, and only the difference between the new rating's total reward and the previous rating's total reward.
- First completion: 1 star = 20, 2 stars = 60, 3 stars = 100 coins.
- Improvement: 1 → 2 stars = 40, 2 → 3 stars = 40, 1 → 3 stars = 80 additional coins.
- Equal or lower rating: no additional coins.
- Each level can award at most 100 coins in total.

### Bonuses (consumable)

- Bonuses are bought with coins and selected before an attempt.
- Players may select up to one bonus of each type per attempt. Different bonus types can be combined.
- Every selected bonus is deducted from inventory when the attempt starts, even if the player never activates it during that attempt.
- Selecting and deselecting before starting does not consume inventory.
- Bonus purchases and remaining inventory persist between sessions.
- Movement speed is not a consumable bonus. It is a permanent equipment upgrade (see below).

| Bonus                         | Price      | Effect                                                                 |
| ----------------------------- | ---------- | ---------------------------------------------------------------------- |
| Extra time                    | 120 coins  | Automatically adds 10 seconds at the start                             |
| One fewer shopping-list item  | 180 coins  | Automatically reduces the number of required items by one at the start |
| Protection bubble             | 150 coins  | Player-activated protection from one delay                             |

Extra time:
- The countdown begins at 02:10 instead of 02:00.
- The 40-second and 80-second star thresholds remain unchanged.
- Completion after 80 seconds but before the extended timer expires awards 1 star, subject to the replay-reward rule.

One fewer shopping-list item:
- Which item is removed is not finalized. Do not invent a rule (see "Unresolved design decisions").

Protection bubble:
- After it is selected and the attempt starts, a small activation icon appears near the top of the gameplay screen.
- The player chooses when to activate it, once during that attempt.
- It lasts up to 10 seconds or until the first protected hit, whichever happens first.
- It prevents the usual delay caused by colliding with a shopper or slipping on spilled or fallen material. The first protected hit pops the bubble without delaying the player.
- It does not protect against power outages.
- If it is never activated, the consumed inventory unit is not refunded.

### Permanent equipment upgrades

- Equipment upgrades are bought with coins. Purchased levels and their effects persist between attempts and after closing the app.
- Each track has 5 total levels: the starting level 1 plus 4 purchasable upgrades. This is not 5 purchases plus a starting level.
- At level 5, no further upgrade can be purchased.
- Unrounded price of the next upgrade: firstUpgradePrice * 1.4^(number of upgrades already purchased on that track).

Footwear:
- Starts at level 1 with basic flip-flops.
- speed(level) = baseSpeed * 1.05^(level - 1), for levels 1–5 (+5% per upgrade relative to the previous level).
- First upgrade costs 100 coins. Unrounded prices: 100, 140, 196, 274.4.
- Names and art for footwear levels 2–5 are not finalized.

Lighting:
- Starts at level 1 with a candle.
- visibilityRange(level) = baseVisibilityRange * 1.10^(level - 1), for levels 1–5 (+10% per upgrade relative to the previous level). Applies during power outages.
- First upgrade costs 130 coins. Unrounded prices: 130, 182, 254.8, 356.72.
- Previously discussed equipment: candle → small flashlight → large flashlight → night-vision goggles. That is 4 items for 5 levels; the fifth item and the final item-to-level mapping are unresolved. Do not invent another item.

Not yet approved:
- Price rounding. Proposal only: calculate each price from the original first-upgrade price and round up to whole coins, rather than repeatedly multiplying rounded prices.
- baseSpeed and baseVisibilityRange values.

### Lives and retry

- The player has a shared pool of up to 3 lives across all levels.
- Losing an attempt deducts exactly one life. Starting an attempt or pressing Retry does not deduct another life.
- At least one life is required to start an attempt.
- Retry starts a new attempt from the beginning and is enabled only if at least one life remains.
- With zero lives, Retry is disabled. The player can buy lives in the shop or wait for regeneration.
- Lost lives regenerate over time; each lost life takes a couple of minutes to recharge (exact duration not finalized).
- Lives can only be purchased outside an active level.
- There is no continuation, paid or free, from the point of failure.

Life purchases:
- Price: 100 coins per life.
- Maximum 3 purchased lives per UTC calendar day. Count lives purchased, not transactions or shop visits.
- A purchase requires enough coins and free space in the 3-life pool.
- An unsuccessful purchase deducts nothing and does not count toward the daily limit.
- The purchase count and its UTC date persist across app restarts.
- Regenerated lives do not count toward the purchase limit. Reaching the daily purchase limit does not prevent natural regeneration.

### Economy status

- The values above are the team's selected design values. They have not been validated as a balanced economy, and the course economy-sheet checks have not been confirmed as passed.
- Income sources are level rewards (at most 100 coins per level, earned once in total through first completion and improvements) and the existing daily reward (50 coins once per UTC day; unchanged).
- Because replay income is limited, affordability of bonuses, upgrades (about 711 coins for all footwear upgrades and 924 for all lighting upgrades under the rounding proposal) and lives must be evaluated against both income sources.

### Unresolved design decisions

- Which shopping-list item the "one fewer item" bonus removes.
- Price rounding for equipment upgrades (proposal above, not approved).
- baseSpeed and baseVisibilityRange values.
- Footwear names and art for levels 2–5.
- The fifth lighting item and the final lighting item-to-level mapping.
- Exact life regeneration duration. main documents "a couple of minutes"; the Level-1-Branch pitch says "about five minutes".
- Economy balance validation against the course economy sheet.

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
- Bootstrap then creates SettingsService with Services.Save before loading Home:
  Services.Settings = new SettingsService(Services.Save);
- Home is the current menu and level-selection screen.
- Level is the current gameplay UI and supermarket layout screen.
- level1 is a separate supermarket-layout prototype that exists only on the Level-1-Branch branch. It is not on main and not in the Scene List.
- SampleScene was deleted from Assets/Scenes but still appears as a disabled entry in the Scene List. It is not part of the normal application flow.
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
- ISettingsService exposes the saved VibrationOn and SoundOn settings. SettingsService reads and writes them through ISaveService and never accesses PlayerPrefs directly. Both settings default to on when no saved value exists.
- Services exposes the active settings service through Services.Settings.
- The Home settings toggles reflect the saved VibrationOn and SoundOn values without notifying their change listeners during initialization or when the panel opens.
- User toggle changes are saved through Services.Settings. Switching vibration from off to on calls Handheld.Vibrate once; switching it off does not vibrate.
- The Home Sound toggle controls a looping AudioSource outside the settings panel so playback can continue after the panel closes. Sound on plays an assigned clip and sound off stops it.
- Home music playback awaits an AudioClip assigned to the HomeMusic AudioSource in the Inspector. With no clip assigned, SoundOn still saves and loads without playback errors or warnings.
- The Home settings UI is a centered cream popup with a blue frame, yellow trim, a blue SETTINGS header plaque, a red X button, labeled Vibration and Sound rows, blue/grey pill switches with a cream knob, a yellow Done button, and a dimming click blocker. It is saved as editable uGUI and TextMeshPro objects under SafeArea and uses a short interruptible fade/slide transition. It follows the team's settings mock-up: an arched SETTINGS header (SettingsHeaderArch), a red close button with a drawn x (CloseButtonX; its old "X" text child Navy is inactive), and navy line icons (VibrationIconNavy, SoundIconNavy). Its other sprites are the *Blue track/shell files and ToggleKnobCream. All are in Assets/Materials/SettingsUI.
- The Home screen layout: a fixed cream TopBar (logo slot, coin icon with CoinsText, heart icon with LivesText, and the gear SettingsButton), the fixed Daily gift and +10 buttons below it, TopLeaves decoration, and a fixed cream BottomBar with Home and Shop. Between them, MapScroll is a vertical-only ScrollRect (Viewport with RectMask2D, Content anchored to the bottom so Home opens at the checkout). Its top edge is below the Daily gift row so those buttons never cover the map.
- The Home Canvas Scaler uses Scale With Screen Size with a portrait 1080x1920 reference resolution and Screen Match Mode Expand. Every phone gets at least 1080x1920 canvas units at the same scale, so nothing is squeezed; taller phones show more of the map above Level 6, and tablets get extra width at the sides.
- MapScroll/Viewport/Content holds the produce and bakery shelves (the angled ShelfProduceAngled/ShelfBakeryAngled sprites, partly past the screen edges with their signs readable), the Path (one PathMap image, 600x1800 at 1 px per canvas unit, drawn along the approved reference's winding route under every level and on upward for tall phones), the Checkout at the start of the map, and Levels with Level1Button–Level6Button. Decorations have Raycast Target off; only the level buttons and the transparent Viewport image receive pointer input, so dragging the map scrolls it without clicking a level.
- Each LevelNButton keeps its original OnClick (LevelPopupController.Open(N)). Its own transparent Image is the hit area, with Stand, Food, NumberPill + LevelNText and StarPlate with three star Images as children. LevelNodeView sets the number and shows the saved best rating from the existing "level_{id}_best_stars" value through Services.Save (three empty stars when nothing is saved).
- Home art comes from the modular kit in Assets/Materials/HomeUI/ModularV1 (Food, Stands, Environment, UI, Path). The older code-drawn sprites in Assets/Materials/HomeUI are no longer referenced by Home, except YellowButton (Daily gift, +10 and the Settings Done button). Level positions (number-pill centres) are taken from the approved reference, about 3% smaller so Level 6 fits under the Daily gift row on 16:9 screens. Adding levels needs new positions, a regenerated PathMap that passes under them, and a taller Content. Path/PathLoop.png and Path/PathStart.png are no longer used by the scene.
- The yellow capsule on the Home and Shop buttons is shown by PressHighlight only while a finger or mouse is held down on the button. It hides on release or when the finger slides off. It is presentation only and does not affect OnClick.
- Shop is an editable panel inside Home.unity. The shared bottom Shop button opens it, and the shared Home button returns to the same map scroll position. The selected Shop tab stays yellow. Boosts uses Services.Boosts, and Lives & Upgrades uses Services.Upgrades for equipment. Displayed prices come from the current services and EconomyConfig, which differ from some planned values above. Extra Life is displayed with purchase disabled until life purchase rules are implemented in a service.
- The Home level popup uses the blue and cream settings style and shows the same three item icons as the Shop. The existing level-title and Play bindings are retained. The three bonus buttons are presentation only and do not consume inventory or apply effects.
- The Logo Image in TopBar shows the team logo (Assets/Materials/Logo/SuperMazeLogo.png, background removed from the team's logo file). The same logo is the app icon: Player Settings Default Icon (AppIcon.png) and the Android Adaptive icon (AppIconBackground.png + AppIconForeground.png, logo inside the safe zone). Unity 6.6 has no separate Android Round/Legacy icon kinds. Asset sources are listed in CREDITS.md.

### Assemblies and script locations

- The Game assembly is defined in Assets/Scripts/Game/Game.asmdef, and its scripts are stored in Assets/Scripts/Game.
- ScreenFlow, Bootstrap, CoinCounter, DailyReward, SettingsPanelController, PressHighlight, UniformScaleFitter and LevelNodeView are in the Game assembly. UniformScaleFitter is no longer used by the Home scene.
- LevelPopupController and SafeAreaFitter currently remain directly under Assets/Scripts and compile into the predefined Assembly-CSharp assembly.
- Service interfaces and implementations are stored in Assets/Scripts/Services and compile into the Services assembly.
- Game.asmdef references Services, Unity.TextMeshPro and Unity.InputSystem.
- Services.asmdef does not reference Game.
- The dependency direction is Game → Services. Services must not depend on gameplay or UI classes.

## Implementation status

- Implemented: Bootstrap-first startup, PlayerPrefs-backed integer saving through ISaveService, persistent coins, the daily reward, saved vibration and sound settings, one-time vibration feedback when enabled from the Home toggle, Home-only looping music control awaiting an assigned clip, the Home settings panel, the Home level-selection popup, safe-area fitting, and navigation between Home and Level through ScreenFlow.
- Present as UI or scene layout but not yet implemented as complete gameplay: player movement and virtual joystick input, shopping-list item collection, checkout completion, the level timer, stars and performance rewards, level progression, obstacles, bonuses, lives and life regeneration, and failure/retry handling.
- The Shop can purchase boosts and footwear/lighting upgrades through the existing services. Applying those bonuses to attempts, applying the protection bubble effect, and life purchases with a daily limit are separate gameplay work.
- CoinCounter currently stores coins under the "coins" save key, and DailyReward stores its last claimed UTC day under "daily_last_day".
- The second service is SettingsService, exposed through ISettingsService and Services.Settings.

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
