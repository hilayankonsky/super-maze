# Credits

This file lists where the assets in Super Maze come from. It separates **reference images**, which guided the design but are not part of the game, from **integrated assets**, which ship inside the Unity project.

## Reference images (not in the game)

| What | Source | AI |
| ---- | ------ | -- |
| Home screen mock-up (supermarket map, top bar, checkout, bottom bar) | Created by the team with ChatGPT (OpenAI) | Yes: ChatGPT image generation |
| Settings popup mock-up | Created by the team with ChatGPT (OpenAI) | Yes: ChatGPT image generation |
| Approved Home reference (`SuperMaze_Home_Kit/Reference/ApprovedHome.png`) | AI-generated concept developed in ChatGPT under the Super Maze team's art direction; supplied by the team | Yes: ChatGPT image generation |
| Logo reference crop (`SuperMaze_Home_Kit/Reference/LogoReferenceCrop.png`) | Crop of the team's logo as embedded in the concept, for reference only. The logo design belongs to its original team creator, not to the AI asset pack. | No (crop only) |

The complete mock-ups are design references. HomeReferenceV2 uses separate cutouts from the supplied Home and settings mock-ups.

## Integrated assets

### Home reference art (current): `SuperMaze/Assets/Art/HomeReferenceV2/`

The team supplied `Super_Maze_home_page.png` and the settings mock-up. The reference designs were created with ChatGPT (OpenAI) under the team's art direction. The team supplied `logo-super_maze.jpeg`.

| Files | Source and processing |
| ----- | --------------------- |
| `Produce.png`, `Bakery.png`, `Checkout.png`, `Level1.png`–`Level6.png`, `Leaves*.png` | Original visible pixels cropped from the Home reference. Codex traced alpha mattes, removed floor-colored edge pixels and used neighboring pixels on the number badges and star plates to clear the sample values. Parts outside the frame or hidden by other objects were unavailable in the source. |
| `Logo.png` | Trimmed from the existing transparent extraction of the team's supplied JPEG. Original logo design by the team. |
| `FloorStart.png`, `FloorLoop.png` | Visible path pixels from the Home reference composited onto an empty floor reconstructed with OpenAI image generation. The repeat and join reuse the earlier PathMap raster, re-sampled and reflected so adjoining edge rows coincide. The continuation and floor behind the objects are reconstructions. |
| `SettingsPlaque.png`, `SettingsClose.png`, `SettingsVibration.png`, `SettingsSound.png`, `SettingsDone.png` | Original visible pixels cropped and matted from the settings mock-up. Codex retouched the plaque and button faces to remove their sample text. Unity renders the editable SETTINGS and Done labels. |
| `ProduceComplete.png` | OpenAI image generation edited the cropped Home reference shelf to complete its left side, feet and attached flowerpot on transparency. Codex trimmed, resized and cleaned the alpha for Unity. |
| `SettingsPlaqueComplete.png` | OpenAI image generation edited the settings plaque to restore its right gold rim and remove the red X fragment. Codex trimmed and resized the transparent result. |
| `FloorStartJoined.png`, `FloorLoopJoined.png` | Codex reused the cream path and filled dots from the visible Level 1–2 route. The same repeat tile is used twice and its boundary row matches the starting segment. |
| `LevelCursor.png` | OpenAI image generation produced a white link-pointer hand with a dark outline on transparency. Codex trimmed and resized it to 32×32 for the Unity cursor. |

### Modular Home kit: `SuperMaze/Assets/Materials/HomeUI/ModularV1/`

32 PNG sprites based on the team's `SuperMaze_Home_Kit`. Home uses kit UI sprites, including the star artwork and press highlight. HomeReferenceV2 supplies the visible map composites.

| Files | Origin | AI |
| ----- | ------ | -- |
| `Food/` (Apple, Cheese, Baguettes, Milk, Strawberries, Honey), `Stands/` (BenchBlue, StoolYellow, BasketBench, MilkDisplay, CrateCoral), `Environment/ShelfProduce.png`, `Environment/ShelfBakery.png`, `Environment/Checkout.png`, `UI/` (Coin, Heart, Gear, Home, Shop, NumberPill, StarPlate, StarFull, StarEmpty, CreamBar, PressedYellow, Leaves) | Generated/reconstructed with OpenAI image generation in ChatGPT from the team's approved Home reference on 2026-11-24, cropped into separate PNGs, alpha checked against a light background. Not pixel-exact extractions of the reference. `StarEmpty` is derived from `StarFull` so both share one silhouette. The Shop icon includes a separate basket handle. | Yes: ChatGPT image generation |
| `Path/PathMap.png` | Generated on 2026-11-24 by Claude Code (Anthropic, Claude Opus 5.5) in Python/Pillow: one path image with a golden glow, soft shadow and dots, drawn along the route traced from the approved reference. No pixels copied from the reference. An upper segment supplies the restored continuation in HomeReferenceV2. | Yes: AI-written drawing code |
| `Path/PathLoop.png`, `Path/PathStart.png` | Regenerated earlier the same day by Claude Code as seamless S-curve tiles; they replaced the kit's original tiles. No longer referenced by the Home scene (replaced by `PathMap.png`). | Yes: AI-written drawing code |
| `Environment/ShelfProduceAngled.png`, `Environment/ShelfBakeryAngled.png` | Perspective-warped copies of the kit's `ShelfProduce.png` and `ShelfBakery.png`, made by Claude Code (Anthropic, Claude Opus 5.5) with Python/Pillow on 2026-11-24 so the shelves look angled like the approved reference. Derived from the AI-generated kit shelves. | Yes: derived from AI-generated art by AI-written code |
| `Environment/Floor.png` | Programmatically generated solid-color texture. | Generated by code |

No third-party asset license is asserted by the kit. No font files were supplied with it.

### Home screen art (earlier, mostly unused): `SuperMaze/Assets/Materials/HomeUI/`

The Home scene uses `YellowButton.png` from this folder for Daily gift and +10. The other files remain in the project.

28 PNG sprites: floor gradient, vines, produce and bakery shelves, shelf sign, potted plant, flower pot, the six level stands with food (`LevelNode1`–`LevelNode6`), map path, number pill, checkout counter, receipt screen, card terminal, shopping bag, bread basket, cream bar, yellow button, pressed-state highlight, and the coin, heart, gear, home and shop icons.

- Made by: Claude Code (Anthropic, model Claude Opus 5.5) at the team's request.
- Method: drawn from code (Python with the Pillow library) out of simple shapes and gradients. They were **not** made with an image-generation model, and no pixels were copied from the ChatGPT reference images.
- AI: Yes. An AI assistant wrote the drawing code and produced the images.
- Based on: the team's ChatGPT reference image and the team palette (red `#FF5757`, yellow `#FADE7A`, blue `#7AB7D3`).

### Settings popup art: `SuperMaze/Assets/Materials/SettingsUI/`

| Files | Origin |
| ----- | ------ |
| `SettingsShellBlue.png`, `SettingsHeaderBlue.png`, `CloseButtonRed.png`, `ToggleTrackOnBlue.png`, `ToggleTrackOffBlue.png`, `ToggleKnobCream.png` | Made by Claude Code (Anthropic, Claude Opus 5.5) with the same code-drawn method and team palette as the Home art. AI: Yes. |
| `SettingsHeaderArch.png`, `CloseButtonX.png`, `VibrationIconNavy.png`, `SoundIconNavy.png` | Drawn from code (Python/Pillow) by Claude Code (Anthropic, Claude Opus 5.5) on 2026-11-24 to follow the team's settings mock-up (arched header, red close button, navy line icons). No pixels copied from the mock-up. `SettingsHeaderBlue.png` and `CloseButtonRed.png` are no longer referenced. AI: Yes. |
| `VibrationPhoneIcon.png`, `SoundSpeakerIcon.png` | Existing team assets. No longer referenced by the Home scene (replaced by the navy icons). Source and tool: *to be filled in by the team.* |
| `SettingsPopupShell.png`, `SettingsHeaderPlaque.png`, `DoneButtonFace.png`, `ToggleTrackOn.png`, `ToggleTrackOff.png` | Earlier wooden/navy settings design by the team. No longer referenced by the Home scene. Source and tool: *to be filled in by the team.* |

### Logo

- The Super Maze logo was designed by a team member: *name to be filled in by the team.*
- It is not attributed to AI.
- The team supplied the logo file on 2026-11-24. `SuperMaze/Assets/Materials/Logo/SuperMazeLogo.png` is that logo with its white background and drop shadow removed by code (Claude Code, Python/Pillow). Home uses its trimmed copy in HomeReferenceV2.
- App icon files in the same folder (`AppIcon.png`, `AppIconBackground.png`, `AppIconForeground.png`) place the same logo on a cream background, made by Claude Code with Python/Pillow. They are set as the app icon in Player Settings.

### Shop and level popup art: `SuperMaze/Assets/Art/ShopUI/`

| Files | Source and processing |
| ----- | --------------------- |
| `ShopFront.png`, `Footwear.png`, `Lighting.png` | Cropped from the team-supplied ChatGPT Shop mock-up (`Super Maze shop UI_ boosts and upgrades (1).png`). The side art on the equipment cards has soft alpha edges. |
| `Time.png` | Generated with the built-in OpenAI image tool from a prompt for a blue stopwatch with a gold crown, cream face, and coral hand on transparency. |
| `Item.png` | Generated with the built-in OpenAI image tool from a prompt for a grocery checklist, red apple, and minus badge on transparency. |
| `Shield.png` | Generated with the built-in OpenAI image tool from a prompt for a pearlescent bubble with a gold shield on transparency. |
| `UpgradesRibbon.png` | Generated with the built-in OpenAI image tool using the Shop mock-up as the visual reference. Prompted for a blank sky-blue ribbon with folded ends and amber accent dashes on transparency, then cropped and resized for Unity. The UPGRADES label is editable TextMeshPro text. |

The generated bonus icons were trimmed and resized by Codex. Home uses the same bonus icons in the Shop and the level popup.

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
| `SuperMaze/Assets/Scripts/Game/LevelNodeView.cs` | Claude Code (Anthropic, Claude Opus 5.5) | Yes |
| `SuperMaze/Assets/Scripts/Game/HomeShopController.cs` | Codex (OpenAI) | Yes |
