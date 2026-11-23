# Decision log

<!-- Your entries start here -->

## 2026-11-23 — Difference-only rewards for improved star ratings

Options: Pay the full reward on every completion / Pay only the first completion / Pay only the difference when the best star rating improves.

Chose: Store the best star rating per level and pay only the difference between the new and previous total reward (1 star = 20, 2 stars = 60, 3 stars = 100). Equal or lower ratings pay nothing, and the saved rating never decreases. Stars use elapsed time: up to 40 s = 3, up to 80 s = 2, otherwise 1 before the timer expires.

Why: Players still have a reason to replay for a better rating, but cannot farm coins by repeating an easy level. Each level is capped at 100 coins, so total level income is predictable when pricing the shop. The first successful completion of a level always pays at least 20 coins, even with one star; later completions pay only when they improve the saved rating. Balance has not been measured yet; the values may change after testing against the economy sheet.

AI: Claude (AI) helped consolidate and phrase this entry from the team's notes.

Who: Team


## 2026-11-23 — Permanent equipment upgrades with increasing prices

Options: Movement speed as a consumable bonus / Permanent upgrade tracks bought with coins.

Chose: Two permanent tracks with 5 total levels each (starting level + 4 upgrades). Footwear: +5% speed per level, first upgrade 100 coins. Lighting: +10% visibility during power outages per level, first upgrade 130 coins. Each next upgrade price is the first price × 1.4^(upgrades already bought). Speed is no longer a consumable bonus.

Why: Permanent upgrades give long-term goals and a reason to keep earning coins after a level's rewards are collected. Increasing prices spread the upgrades over more play sessions. Price rounding, base values, and the fifth lighting item are still open. Balance has not been measured yet.

AI: Claude (AI) helped consolidate and phrase this entry from the team's notes.

Who: Team


## 2026-11-23 — Life purchases with a daily limit

Options: Unlimited life purchases / No life purchases / Limited daily life purchases.

Chose: 100 coins per life, at most 3 purchased lives per UTC day, only with enough coins and free space in the 3-life pool. Failed purchases cost nothing and do not count. Natural regeneration is unaffected. There is no paid continuation from a failure.

Why: Players who run out of lives can keep playing with coins they earned, while the daily limit keeps lives meaningful and stops coins from replacing the regeneration wait entirely. Balance has not been measured yet.

AI: Claude (AI) helped consolidate and phrase this entry from the team's notes.

Who: Team


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
