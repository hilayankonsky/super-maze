# Credits

This file lists where the assets in Super Maze come from. It separates **reference images**, which guided the design but are not part of the game, from **integrated assets**, which ship inside the Unity project.

## Reference images (not in the game)

| What | Source | AI |
| ---- | ------ | -- |
| Home screen mock-up (supermarket map, top bar, checkout, bottom bar) | Created by the team with ChatGPT (OpenAI) | Yes: ChatGPT image generation |
| Settings popup mock-up | Created by the team with ChatGPT (OpenAI) | Yes: ChatGPT image generation |

These images are design references only. They are not stored in `SuperMaze/Assets` and are not shown in the game.

## Integrated assets

### Home screen art: `SuperMaze/Assets/Materials/HomeUI/`

28 PNG sprites: floor gradient, vines, produce and bakery shelves, shelf sign, potted plant, flower pot, the six level stands with food (`LevelNode1`–`LevelNode6`), map path, number pill, checkout counter, receipt screen, card terminal, shopping bag, bread basket, cream bar, yellow button, pressed-state highlight, and the coin, heart, gear, home and shop icons.

- Made by: Claude Code (Anthropic, model Claude Opus 5.5) at the team's request.
- Method: drawn from code (Python with the Pillow library) out of simple shapes and gradients. They were **not** made with an image-generation model, and no pixels were copied from the ChatGPT reference images.
- AI: Yes. An AI assistant wrote the drawing code and produced the images.
- Based on: the team's ChatGPT reference image and the team palette (red `#FF5757`, yellow `#FADE7A`, blue `#7AB7D3`).

### Settings popup art: `SuperMaze/Assets/Materials/SettingsUI/`

| Files | Origin |
| ----- | ------ |
| `SettingsShellBlue.png`, `SettingsHeaderBlue.png`, `CloseButtonRed.png`, `ToggleTrackOnBlue.png`, `ToggleTrackOffBlue.png`, `ToggleKnobCream.png` | Made by Claude Code (Anthropic, Claude Opus 5.5) with the same code-drawn method and team palette as the Home art. AI: Yes. |
| `VibrationPhoneIcon.png`, `SoundSpeakerIcon.png` | Existing team assets, reused unchanged. Source and tool: *to be filled in by the team.* |
| `SettingsPopupShell.png`, `SettingsHeaderPlaque.png`, `DoneButtonFace.png`, `ToggleTrackOn.png`, `ToggleTrackOff.png` | Earlier wooden/navy settings design by the team. No longer referenced by the Home scene. Source and tool: *to be filled in by the team.* |

### Logo

- The Super Maze logo was designed by a team member: *name to be filled in by the team.*
- It is not attributed to AI.
- The logo file is not in the project yet. The Home top bar has an empty `Logo` slot for it.

### Fonts

| Font | Path | Source | License |
| ---- | ---- | ------ | ------- |
| Roboto Bold (TextMesh Pro SDF asset) | `SuperMaze/Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset` | Roboto by Google, bundled with Unity's TextMesh Pro "Examples & Extras" | Not verified here. Check the license notice shipped with TextMesh Pro before release. |
| Liberation Sans (TextMesh Pro SDF asset) | `SuperMaze/Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset` | Bundled with Unity's TextMesh Pro Essential Resources | Not verified here. Check the license notice shipped with TextMesh Pro before release. |

### Code

| File | Made by | AI |
| ---- | ------- | -- |
| `SuperMaze/Assets/Scripts/Game/PressHighlight.cs` | Claude Code (Anthropic, Claude Opus 5.5) | Yes |
| `SuperMaze/Assets/Scripts/Game/UniformScaleFitter.cs` | Claude Code (Anthropic, Claude Opus 5.5) | Yes |
