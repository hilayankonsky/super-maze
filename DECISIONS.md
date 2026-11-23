# Decision log

<!-- Your entries start here -->
```
## 2026-11-23 — Star-Based Reward System
Options: Full reward on every replay / Full reward only when improving / Reward only the difference when improving
Chose: Reward only the difference - Players earn 40, 80, or 120 coins for 1–3 stars. Additional coins are awarded only when improving the previous best rating. This encourages replaying levels while preventing unlimited coin farming.
AI: AI suggested awarding only the difference between the previous and new rewards. We adopted this approach to keep the economy balanced.
Who: Hila

## 2026-11-11 – Level-specific Play button
Options: One general Play button / A separate Play button after selecting a level.
Chose: A Play button for each selected level.
Why: Players can choose power-ups before starting a level and return to previous levels to improve their scores. This makes level-specific navigation clearer and gives players more control over which level they want to play.
AI: Not used in making this decision.
Who: Hila


## 2026-11-11 — Minimum Android API level
Options: Keep the existing minimum of API 26 (Android 8.0) / Raise it to API 29 (Android 10).
Chose: API 29 (Android 10).
Why: The course requires API 29 as the minimum. This makes Android 10 the oldest Android version on which players can install Super Maze.
AI: helped explain me phrase this decision clearly.
Who: Lior


## 2026-11-04 — Time based gameplay
Options: Complete the level within a time limit / Fail when colliding with an obstacle
Chose: A time limit. Collisions cause a three-second delay instead of ending the attempt.
Why: A ticking clock encourages players to act faster and more efficiently, and motivates them to replay levels to beat their own time. It also gives us more options to expand the game.
AI: Not used in making this decision.
Who: Lior
```






