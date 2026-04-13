# Codex Workflow

## Purpose

This file is the human-readable operating manual for Codex work in this repo.

The goal is:

- a small durable instruction layer
- one clear read order at the start of every session
- task prompts shaped like issue specs
- manual Unity verification tracked in one place
- user actions and blockers tracked in one place
- lightweight git hygiene that fits a small prototype

`AGENTS.md` is the first file and the only always-read durable instruction layer. This file expands the workflow without turning `AGENTS.md` into a giant context dump.

## Session Start Order

Read these in order at the beginning of each coding session:

1. `AGENTS.md`
2. [`06-todos.md`](./06-todos.md)
3. the active task spec
4. [`08-user-input.md`](./08-user-input.md) and [`09-testing.md`](./09-testing.md) if the task touches Unity/manual verification
5. [`07-decisions.md`](./07-decisions.md) and [`03-project-map.md`](./03-project-map.md) when architecture or cross-system context matters

Current default task spec:

- [`05-first-boss-plan.md`](./05-first-boss-plan.md)

## Prompt Shape

Implementation prompts should read like a small GitHub issue. Use this shape whenever a new task spec is created:

```md
Goal:

Context:

Constraints:

Done when:

Manual verification needed:
```

Use extra sections only when they add real value, such as `Implementation notes` or `Open questions`.

Do not rely on a long generic persona prompt. Keep project identity short in `AGENTS.md` and keep the rest task-specific.

## Session Loop

1. Read the start-order files and define one coherent milestone for the session.
2. Gather only the code and scene context needed for that milestone.
3. Implement the change.
4. Update the living docs before handing the work off:
   - [`06-todos.md`](./06-todos.md) for status and next steps
   - [`07-decisions.md`](./07-decisions.md) for durable choices
   - [`08-user-input.md`](./08-user-input.md) for required human actions, missing assets, or judgment calls
   - [`09-testing.md`](./09-testing.md) for any Unity/manual verification steps
   - re-save any affected Unity scene or prefab after serialized-field changes if repo-on-disk state must stay trustworthy
5. Verify what can be verified locally.
6. Review `git status` and leave only intentional changes.

If a task is fuzzy, multi-step, or architecturally risky, switch to planning first instead of coding immediately.

## When To Read Or Update Each File

### `AGENTS.md`

- Read every session.
- Keep it short and durable.
- If it grows, move detail into the right doc and leave only the rule or link here.

### [`06-todos.md`](./06-todos.md)

- Read every session.
- Keep active work in `Now` and `Next`.
- Every active item must say what the next concrete step is.
- Move completed setup or milestones to `Done Recently`.

### [`07-decisions.md`](./07-decisions.md)

- Read when the task depends on architecture or previous choices.
- Update when a decision is durable enough that later sessions should not rediscover it.
- Do not log every temporary thought. Only log decisions that should survive across sessions.

### [`08-user-input.md`](./08-user-input.md)

- Read before starting work that may depend on assets, Unity inspector actions, logins, or user preference calls.
- Update it immediately when the AI needs something from the user.
- Put blockers first, then near-term asks, then optional/polish asks.
- Keep a `User response:` field so answers can be written directly in the file.

### [`09-testing.md`](./09-testing.md)

- Read before asking the user to open Unity.
- Update it before every manual verification handoff.
- Each test must include exact scene, setup, steps, expected result, failure clues, and what to capture if it fails.
- If a code change invalidates an old result, clear the old result and move the test back to `Pending` or `Re-test`.

### `Docs/tasks/*.md`

- Use this folder for future feature, bug, and investigation specs.
- Keep one stable file per coherent task instead of creating a new file for every small prompt.
- Link the active task file from [`06-todos.md`](./06-todos.md).

## Git Strategy

- Stay on `mukstoo` for normal solo work unless the user explicitly asks for another branch.
- Do not pull or push unless the user explicitly asks.
- Make local commits at meaningful checkpoints, not after every tiny edit.
- Use a short-lived feature branch or worktree only if the work is risky, large, or truly parallel.
- Review `git status` at the end of every session.
- Keep Unity-generated or personal files out of commits.

## Unity Manual Verification Workflow

This project is manual-test heavy because:

- there is no Unity editor bridge here
- there is no active automated Unity test suite
- there is no reliable local compile loop in this environment

So the workflow is:

1. Add or refresh the relevant test entry in [`09-testing.md`](./09-testing.md).
2. If the test depends on a user action or asset, add the matching request to [`08-user-input.md`](./08-user-input.md).
3. Hand off the exact test IDs to the user.
4. Have the user write the result directly under `User result:`.
5. If the code changes again, reset the outdated result instead of stacking stale answers.
6. If a script's serialized fields changed, make sure the affected scene or prefab is re-saved before treating the repo copy as the new source of truth.

If a Unity test shows missing sprites/scripts after a change, add a short troubleshooting step to the test:

- reimport the relevant assets, or
- restart Unity once

## Naming Conventions

- Keep numbered overview docs in `Docs/` while the top-level set stays small.
- Put future task specs in `Docs/tasks/` using short kebab-case names.
- Keep decision IDs in the form `DEC-YYYY-MM-DD-XX`.
- Keep user-input IDs in the form `UI-###`.
- Keep manual test IDs tied to the work, such as `BOSS-S1-001`.

## Lightweight Rules For This Repo

- Prefer small targeted changes over broad framework work.
- Follow the current MonoBehaviour-centric architecture unless a specific task forces a refactor.
- Reuse existing scene flow, UI systems, and health/damage systems where practical.
- Treat tracked `*.sln` and `*.csproj` files as legacy repo state for now. Do not mix that cleanup into gameplay work unless it becomes necessary.
- Do not add repo-local skills, MCP servers, or extra agent machinery until the workflow is stable enough to justify them.

## Canonical References

- GPT-5.4 launch: <https://openai.com/index/introducing-gpt-5-4/>
- Codex best practices: <https://developers.openai.com/codex/learn/best-practices>
- AGENTS.md guide: <https://developers.openai.com/codex/guides/agents-md>
- How OpenAI uses Codex: <https://openai.com/business/guides-and-resources/how-openai-uses-codex/>
- Outside workflow reference: <https://nimbalyst.com/blog/coding-with-ai-agents-best-practices-2026/>
- Caution on oversized repo context files: <https://www.layerthelatestinalattice.com/papers/e6c1b08d0c8b2417e821bbaaee7bb9fd91be17e9>
