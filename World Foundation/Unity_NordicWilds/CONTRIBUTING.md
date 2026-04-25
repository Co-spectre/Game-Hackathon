# Contributing to Nordic Wilds

This project uses GitHub with a branch-per-feature workflow so multiple people can work at the same time without overwriting each other.

## Repository Rules

- Keep `main` stable and playable.
- Create a short-lived branch for every task.
- Open a pull request for every change.
- Keep commits small and focused.
- Do not edit the same Unity scene at the same time unless the team agrees first.

## Suggested Branch Names

- `feature/player-combat`
- `feature/environment-art`
- `feature/sound-pass`
- `fix/enemy-ai`
- `fix/portal-flow`

## Unity Collaboration Tips

- Turn on `Visible Meta Files` and `Force Text` in Unity Project Settings.
- Use Git LFS for large binary assets such as `.fbx`, `.png`, `.psd`, `.wav`, `.mp3`, and `.mp4`.
- Keep prefabs modular so art changes do not require code changes.
- Prefer one scene owner at a time.
- If two people must touch the same scene, split the work into tiny PRs.

## Before You Open a PR

- Confirm the project still opens in Unity.
- Check the affected script compiles.
- Verify the scene or prefab loads without missing references.
- Keep the PR limited to the task you meant to do.

## Review Standard

- Ask for one teammate review before merging.
- Resolve conflicts locally before requesting review.
- Use screenshots or short clips for visual changes.
