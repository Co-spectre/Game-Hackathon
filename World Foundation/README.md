# Nordic Wilds

Nordic Wilds is a Hades-inspired isometric action roguelite built in Unity C#. The first campaign takes place in a harsh Nordic wilderness, then expands into a second campaign set in medieval Japan. The goal is to keep the combat crisp, the rooms readable, and the world identity strong enough that each era feels instantly distinct.

## Game Pillars

- Tight, readable combat with dash-centric movement and fast attack commitment.
- A room-by-room encounter loop with clear rewards, doors, and branching progression.
- Strong visual identity for each era: cold stone and snow for Nordic Wilds, warm shrine and forest language for medieval Japan.
- A light survival layer in the Nordic campaign through temperature and campfires.
- Progression that can support weapons, boons, relics, story events, and biome-specific enemies.

## Current Core Systems

- `PlayerController`: Rigidbody-based movement, dash, invulnerability frames, and isometric facing.
- `PlayerCombat`: Buffered attack input, combo tracking, dash-strike bonuses, and hit feedback.
- `RoomController` and `WaveSpawner`: Room lock, sequential enemy waves, and reward flow after combat.
- `CameraJuiceManager`: Hit stop and camera shake used for impact feedback.
- `NordicWorldBuilder`: Editor-side world generation for the Nordic arena, portal, campfire, and environmental dressing.
- `JapanPortal`: Realm transition logic that moves the player into the Japan section and changes the lighting mood.

## Build Setup

1. Open `World Foundation/Unity_NordicWilds` in Unity Hub.
2. Use Unity `6000.0.72f1` or a compatible Unity 6 build.
3. Make sure the `Player`, `Enemy`, and `Campfire` tags exist before running the scene.
4. Open the main scene, press Play, and test movement, dash, attack, and room clears.

## Notes

- The project uses C# for gameplay and Unity editor scripting.
- Most gameplay code is already structured for extension, but several systems still benefit from data-driven content, better UI, and stronger content separation between the two eras.

## Team Collaboration

If you are working with a team, start with [TEAM_WORKFLOW.md](TEAM_WORKFLOW.md). It covers the recommended GitHub setup, branch workflow, file ownership split, and Unity-specific collaboration tips.
