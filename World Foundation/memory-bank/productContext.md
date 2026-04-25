# Product Context

## Overview

Nordic Wilds is a Unity action roguelite where the player starts in a frozen Nordic world and later crosses into medieval Japan. The project is currently built as a gameplay prototype with working room flow, combat, camera shake, portal travel, and editor-generated environment pieces.

## Core Features

- Isometric third-person combat with dash, attack, and combo behavior.
- Room encounters that lock the player in until enemies are defeated.
- A Nordic environment that uses snow, stone, campfires, and cold lighting.
- A portal system that shifts the game into a Japan-themed realm.
- Basic HUD and damage feedback for health, stamina, and hit numbers.

## Technical Stack

- Unity 6 (`6000.0.72f1`)
- C# gameplay scripts and editor scripts
- Unity physics and Rigidbody movement
- Unity UI / IMGUI for the current HUD prototype
- Built-in render pipeline materials and scene lighting

## Existing Implementation Notes

- The codebase already has a clear split between player control, combat, room flow, camera juice, world generation, and portal travel.
- Some systems are still prototype-level and should be replaced with more data-driven and UI-friendly versions as the project grows.