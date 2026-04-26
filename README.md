# Nordic Wilds

Nordic Wilds is a Hades-inspired isometric action roguelite built in Unity 6. The game starts in a harsh Nordic wilderness and later transitions into a medieval Japan-inspired realm. The overall goal is to keep the combat fast, readable, and punchy while making each era feel visually distinct.

## Game Vision

The project blends two fantasy cultures into one progression path:

- Nordic wilderness: cold terrain, fjord energy, longships, stone, snow, and survival pressure.
- Medieval Japan: warm shrine architecture, forest paths, bamboo, docks, and ritual storytelling.

The current implementation leans heavily into cinematic transitions, environmental storytelling, and arena combat flow.

## Core Gameplay

The current core loop is built around:

- Isometric player movement with dash-based mobility.
- Fast attack combos and close-range combat.
- Room-by-room enemy encounters with lock-in combat zones.
- Reward and progression flow after each fight.
- Realm transition sequences that move the player between the Nordic and Japan-themed sections.

## Current Systems In The Codebase

The following systems are already present in the Unity project:

- `PlayerController`: Rigidbody-based movement, dashing, controls lockout, and facing.
- `PlayerCombat`: attack buffering, combo handling, dash-strike behavior, and hit feedback.
- `RoomController` and `WaveSpawner`: enemy waves, room locking, and reward flow.
- `CameraJuiceManager`: camera shake and hit-stop style feedback.
- `NordicWorldBuilder`: editor-side world generation for the Nordic arena, the portal flow, the campfire area, and related world dressing.
- `JapanPortal`: realm transition logic into the Japan section.
- `ForestQuestController`: story progression, Leaf dialogue, artifact hunting, combat escalation, and the boat journey sequence.
- `CutsceneManager`: full-screen cutscene playback using the images from the Game Ideas folder.
- `BoatBobber` and `OceanWater`: procedural ocean and boat motion support.

## Story And Progression

The current story flow centers on a journey from a wilderness settlement into a larger mythic world.

### Nordic side

- The player begins in a Nordic area with a cinematic menu and longship departure.
- Leaf acts as the guide character during the early quest flow.
- The player collects artifacts across the world.
- Artifact collection triggers story cutscenes and dialogue beats.
- After the early quest chain, the player boards a boat and leaves the island.

### Japanese side

- The second major region is Yamato, a medieval Japan-inspired realm.
- The arrival sequence includes a dock, ocean, and a landing transition.
- The tone shifts toward shrine architecture, controlled geometry, and warmer lighting.

## Cutscenes And Presentation

The game now has a cutscene system driven by image assets from the Game Ideas folder.

- Start Game shows the first image for a few seconds before gameplay begins.
- Leaf intro scenes use separate cutscene entries.
- Artifact collection shows a full-screen image each time the player presses E near a relic.
- The boat departure flow includes a full-screen transition image after the sailing animation and before disembarking.
- Dialogue title cards are displayed inside boxed UI panels so character conversations stay readable.

The imported source images live in:

- `World Foundation/Unity_NordicWilds/Assets/Resources/Cutscenes`

## Visual Style

The current art direction aims for:

- Nordic: cool lighting, stone, moss, snow, longship silhouettes, and coastal atmosphere.
- Japan: warmer sunset tones, shrines, bamboo, wood, and layered landscape storytelling.
- UI: dark boxed dialogue cards with gold-accented borders.

## Project Structure

The Unity project lives here:

- `World Foundation/Unity_NordicWilds`

Notable folders:

- `Assets/Scripts`: gameplay, UI, environment, world logic, and combat.
- `Assets/Editor`: world-building tools and scene generation helpers.
- `Assets/Resources`: runtime-loaded images and other resource content.
- `Assets/Resources/Cutscenes`: the imported cutscene images.

## Setup

1. Open `World Foundation/Unity_NordicWilds` in Unity Hub.
2. Use Unity 6, version `6000.0.72f1`, or a compatible Unity 6 build.
3. Make sure the project imports cleanly and the required tags exist, such as `Player` and `Enemy`.
4. Open the main scene or run the editor world builder if the world needs to be regenerated.
5. Press Play and test the start menu, cutscenes, artifact flow, and boat sequence.

## Controls

The current gameplay code supports the following general interactions:

- Movement and camera-driven isometric exploration.
- Dash for fast repositioning.
- Attack combos in combat.
- E to interact with artifacts and the boat prompt.
- UI prompts for dialogue and quest flow.

## Current Status

This project is still actively being built. The following areas are already in place, but can still be expanded:

- More artifact scenes and dialogue beats.
- Additional enemy types and encounter variety.
- More polish on boat, dock, ocean, and realm-transition cinematics.
- More narrative content for both the Nordic and Japan sections.
- Content tuning for combat balance, pacing, and reward flow.

## Notes For Contributors

- Keep the main branch stable.
- Make small focused changes when editing Unity scenes or prefabs.
- Use Git LFS for large binary assets like textures, audio, models, and videos.
- If you are updating the world builder or cutscene flow, verify the affected scripts compile after the change.

## Related Documentation

- [World Foundation/README.md](World%20Foundation/README.md)
- [World Foundation/TEAM_WORKFLOW.md](World%20Foundation/TEAM_WORKFLOW.md)
- [World Foundation/Unity_NordicWilds/README.md](World%20Foundation/Unity_NordicWilds/README.md)
