# Beans AGENTS

## Project

- Beans is a small Unity 6 / URP 2.5D action-roguelike prototype.
- Use `mukstoo` as the default working branch unless the user asks for another branch or a worktree.

## Read Order

1. `Docs/00-codex-workflow.md`
2. `Docs/06-todos.md`
3. The current task spec. For the boss MVP, this is `Docs/05-first-boss-plan.md`.
4. `Docs/08-user-input.md` and `Docs/09-testing.md` for any task that needs Unity/editor/manual verification.
5. `Docs/07-decisions.md` and `Docs/03-project-map.md` when architecture context matters.

## Working Rules

- Keep one coherent milestone per coding session. If the task is fuzzy or multi-step, plan first.
- Shape implementation prompts like GitHub issues: `Goal`, `Context`, `Constraints`, `Done when`, `Manual verification needed`.
- Keep durable instructions here short. Put task detail in the relevant doc instead of growing this file.
- Update `Docs/06-todos.md`, `Docs/07-decisions.md`, `Docs/08-user-input.md`, and `Docs/09-testing.md` whenever the task state changes.

## Unity And Repo Hygiene

- Track source-controlled project data only: `Assets/`, `Packages/`, `ProjectSettings/`, and `Docs/`.
- Do not commit Unity-generated or personal workspace noise such as `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `.vs/`, `.vscode/`, or recovery backups.
- Do not revert unrelated local changes. Review `git status` before finishing.
- Do not pull or push unless the user explicitly asks.

## Coding Rules

- Prefer small, targeted changes over broad framework work.
- Follow the existing MonoBehaviour-centric style unless the task clearly requires a refactor.
- When the task needs Unity inspector work, assets, logins, or a human judgment call, record it in `Docs/08-user-input.md` immediately.
- Before asking the user to test something in Unity, add or refresh the exact steps in `Docs/09-testing.md`.

## Done Checklist

- Code and docs match the current task state.
- Manual verification steps exist for any behavior that cannot be confirmed here.
- Relevant entries in `Docs/06-todos.md`, `Docs/07-decisions.md`, `Docs/08-user-input.md`, and `Docs/09-testing.md` are updated.
- `git status` has been reviewed and only intentional changes remain.
