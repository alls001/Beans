# Task Specs

Store future feature, bug, and investigation specs here. Keep one stable file per coherent task rather than creating a new doc for every small prompt.

## Naming

- Use short kebab-case names such as `boss-health-bar.md`, `level-phase-director.md`, or `enemy-cleanup.md`.
- If a doc needs ordering in the main `Docs/` folder, keep using numbered names there. The current boss seed spec stays at [`../05-first-boss-plan.md`](../05-first-boss-plan.md).

## Recommended Shape

- `Goal`
- `Context`
- `Constraints`
- `Done when`
- `Manual verification needed`
- optional `Implementation notes`
- optional `Open questions`

## Rules

- Link the active task doc from [`../06-todos.md`](../06-todos.md).
- When a decision becomes durable, copy it into [`../07-decisions.md`](../07-decisions.md).
- When a task needs user action, add it to [`../08-user-input.md`](../08-user-input.md).
- When a task needs Unity/manual validation, add the exact checklist to [`../09-testing.md`](../09-testing.md) before asking for help.
