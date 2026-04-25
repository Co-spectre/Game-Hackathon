# Team Workflow for Nordic Wilds

This project should be managed in GitHub with a simple branch-and-review workflow.

## Recommended Setup

- Use one GitHub repository for the Unity project.
- Each person works on their own branch.
- Merge changes through pull requests, even if the team is small.
- Keep the main branch stable and playable.

## Suggested Ownership Split

- Gameplay programmer: player movement, combat, enemy AI, room flow, camera, and survival systems.
- Narrative/world designer: story, dialogue, progression, memory unlocks, biome identity, boss concepts.
- Character artist: the Samurai, the Viking, The Vessel, enemy silhouettes, armor, weapon shapes.
- Props/environment/sound: level dressing, shrine/fjord assets, VFX, ambient audio, music cues, UI sounds.

## How To Work At The Same Time

- Use separate folders for each discipline so people do not overwrite each other.
- Agree on file naming before anyone starts making assets.
- Keep prefabs modular so the environment team can swap art without touching code.
- Create tasks in GitHub Issues or a project board so everyone knows what is blocked and what is ready.
- Do short daily check-ins to avoid two people editing the same prefab or scene.

## Unity-Specific Advice

- GitHub works well if you add Git LFS for large binary files like `.psd`, `.fbx`, `.wav`, `.png`, and `.mp4`.
- If the project becomes very asset-heavy and many people edit scenes and prefabs at once, Unity Version Control (Plastic SCM) can be easier than plain Git for merge conflicts.
- For a small team, GitHub + Git LFS + clear branch rules is usually the simplest and best starting point.

## Branch Rules

- `main` is always stable.
- `dev` can hold the latest integrated work.
- Feature branches should be small and focused, like `combat/player-balance` or `art/japan-props`.
- No one should push directly to `main` unless the team explicitly agrees.

## Merge Checklist

- The change still opens in Unity.
- The relevant scene or prefab loads without errors.
- The affected script compiles.
- The branch contains only the intended work.
- Another teammate reviews it before merge.