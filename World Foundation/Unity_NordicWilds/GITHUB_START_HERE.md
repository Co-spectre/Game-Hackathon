# GitHub Start Here

This project is already initialized as a Git repository in `Unity_NordicWilds` and is ready to publish to GitHub.

## What To Create On GitHub

- Repository name: `Nordic-Wilds` or `Unity_NordicWilds`
- Default branch: `main`
- Optional working branch: `dev`

## Recommended Push Flow

1. Create the empty repository on GitHub.
2. Add the remote locally.
3. Commit the initial project snapshot.
4. Push `main`.
5. Create `dev` for team integration work.
6. Invite your teammates to the repo.

## Suggested Commands

```powershell
git remote add origin https://github.com/<your-name>/<repo-name>.git
git add .
git commit -m "Initial Unity project"
git push -u origin main
git checkout -b dev
git push -u origin dev
```

## Team Setup Checklist

- Enable Git LFS before adding large art and audio files.
- Keep Unity `Visible Meta Files` and `Force Text` enabled.
- Use feature branches for all work.
- Open pull requests into `dev` or `main`, depending on your team flow.
