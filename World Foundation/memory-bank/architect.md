# Nordic Wilds: System Architect

## Overview

This file tracks the main architecture decisions for Nordic Wilds, especially the systems that support a Hades-style encounter loop and the transition from the Nordic act into the medieval Japan act.

## Architectural Decisions

1. Use C# MonoBehaviours for gameplay, combat, and editor tooling so the project stays native to Unity.
2. Separate player movement, combat, room flow, and camera feedback into different components to keep the combat layer easier to balance.
3. Keep the Nordic and Japan eras visually and mechanically distinct through lighting, environment dressing, and portal-based transition logic.
4. Use room-based encounters instead of open wandering combat so the progression stays readable and roguelite-friendly.

## Main Components

- `PlayerController`: Handles movement, dash, invincibility, and facing.
- `PlayerCombat`: Handles attacks, combo flow, dash strikes, and hit response.
- `Health`: Shared health, damage, healing, and death handling.
- `RoomController`: Controls encounter entry, lockout, and reward state.
- `WaveSpawner`: Spawns enemy waves and notifies when combat is complete.
- `CameraJuiceManager`: Handles hit stop and camera shake feedback.
- `NordicWorldBuilder`: Builds the Nordic environment and portal setup in the editor.
- `JapanPortal`: Moves the player between the two era spaces and changes the environment mood.

## Design Considerations

- The current prototype is already organized well enough to support more content, but enemy, reward, and UI content should become data-driven before the game scales.
- IMGUI-based HUD elements are fine for prototyping, but the final game should move to a proper Unity UI layer.
- The world generation script is useful for iteration, but the final version should likely become a set of reusable prefabs and scene templates.

