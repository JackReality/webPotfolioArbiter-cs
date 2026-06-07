---
name: blazor-session-startup
description: Mandatory startup procedure for Blazor Server sessions — launch dev server, list tasks, wait for user
source: auto-skill
extracted_at: '2026-06-07T16:35:00.000Z'
---

## Session startup procedure

Every time a new session begins for this Blazor Server project, execute these steps **automatically and in order** — do not ask the user to do anything that is already documented.

### Step 1 — Launch the dev server

```powershell
dotnet watch
```

Run it as a background process. The app will be available at http://localhost:5252.
Always use `dotnet watch` (not `dotnet run`) so the browser auto-reloads on file changes.

### Step 2 — List the tasks

Read the **"Session en cours"** section in `QWEN.md` and display:
- **Tâches à faire** (tasks remaining)
- **Tâches déjà faites** (tasks completed)

Present the list as-is, no reordering or commentary needed.

### Step 3 — Wait for the user

Do **NOT** start coding. Do **NOT** ask "sur quelle tâche tu veux qu'on travaille ?" if the answer is obvious from the list. Just stop and let the user decide.

### Anti-patterns (things that frustrated the user)

- **Asking the user to launch `dotnet watch`** — it's documented, just do it.
- **Responding in English** — QWEN.md mandates French responses.
- **Asking "what should we do?"** — the task list is in QWEN.md, read it.
- **Starting to code without the user's go-ahead** — list tasks, then wait.

### Reference

The startup procedure is also documented in `.qwen/onStartup.md`.
