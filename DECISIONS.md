# Decision log

Add an entry every time the team makes a real choice: a tool, a design, a trade-off.
About 1–2 entries a week. Newest at the top. Write the reasoning yourselves; AI may fix the wording.

## Template
```
## YYYY-MM-DD — [The decision, in a few words]
Options: [A] / [B] / [C]
Chose: [what] — [why]
AI: [what the AI suggested, and what you did with it — or "not used"]
Who: [name(s)]
```

## Example
```
## 2026-11-20 — Save progress locally first
Options: PlayerPrefs / JSON file / Cloud Save
Chose: JSON file now, Cloud Save later
Why: works offline, easy to move later
AI: suggested a SQLite database — rejected, too heavy for one save file
Who: Yuval
```

---

<!-- Your entries start here -->
```
## 2026-11-04 — Time based gameplay
Options: Complete the level within a time limit / Fail when colliding with an obstacle
Chose: A time limit. Collisions cause a three-second delay instead of ending the attempt.
Why: A ticking clock encourages players to act faster and more efficiently, and motivates them to replay levels to beat their own time. It also gives us more options to expand the game.
AI: Not used in making this decision.
Who: Lior
```







