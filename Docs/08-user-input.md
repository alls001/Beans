# User Input Queue

Write responses directly under the matching `User response:` field so later sessions can read them quickly. If a request becomes obsolete, remove or replace the old response instead of stacking stale notes.

## Blockers


## Soon

### UI-003 - Run Unity checks when a session marks tests ready

Need from user: Open Unity and follow the current entries in [`09-testing.md`](./09-testing.md) whenever a session hands off manual verification.

Why it matters: Scene serialization, inspector wiring, and gameplay feel cannot be fully validated from here.

How to do it:

1. Wait for the session summary to say which test IDs are ready.
2. Open the listed scene in Unity.
3. Follow the steps exactly.
4. Write the result under the matching `User result:` field in [`09-testing.md`](./09-testing.md).

Unblocked when: The requested test IDs have results recorded.

User response:

### UI-006 - Capture scene render context if boss visuals still fail

Need from user: If the foot or shadow remain invisible after the next visual fix, capture the player and ground render info.

Why it matters: We can align sorting and ground placement precisely only if we know the actual render stack used in the scene.

How to do it:

1. Open `LevelBoss`.
2. Select `Player` in the Hierarchy.
3. In the Inspector, find the `SpriteRenderer` and note:
   - Sorting Layer
   - Order in Layer
4. Still on `Player`, note the Transform `Position` Y value.
5. Select the large ground or platform sprite (for example `Beanstalk_cenario` or the object that looks like the playable floor).
6. In the Inspector, note its `SpriteRenderer` sorting layer and order.
7. Write the values here.

Unblocked when: The values are recorded or the visuals are already correct.

User response:
From screenshots (2026-04-13):
- Player SpriteRenderer Sorting Layer: `player`
- Player Order in Layer: `0`
- Player Position Y: `8.7`
- Ground: `meshNuvem` MeshRenderer, Transform Y: `-5.8`

### UI-007 - Optional fixed spawn anchor for the boss foot

Need from user: Optional only if the code-driven fallback still does not produce a good start spot. If you want the foot to always start in a specific authored location, create a `BossSpawn` empty GameObject in `LevelBoss` and place it where the foot should idle.

Why it matters: The controller now auto-falls back to a player-start offset, so this is only for exact scene authorship if that fallback still looks wrong.

How to do it:

1. Open `LevelBoss`.
2. Create an empty GameObject named `BossSpawn`.
3. Move it to the spot where the foot should start.
4. Select `BossFoot` and in `BossFootController`, assign `BossSpawn` to `Spawn Anchor`.

Unblocked when: The anchor is set or we decide to keep the offset.

User response:

## Later

### UI-004 - Provide optional boss audio and impact FX

Need from user: Add stomp SFX, hurt SFX, or simple dust and impact art if you want the boss to feel better after the MVP works.

Why it matters: These are polish multipliers, not blockers.

Suggested asset formats: `.wav` or `.ogg` for audio at `44.1 kHz` or `48 kHz`, and transparent `.png` for dust or impact sprites.

Unblocked when: Optional files are added or intentionally skipped.

User response:

## Resolved

### UI-001 - Post-boss scene routing

Decision: Insert `LevelBoss` between the current main gameplay scene and its existing next scene.

Current flow on disk:

- `Cutscene -> Level01 -> Level02 -> Creditos`

So the boss flow becomes:

- `Cutscene -> Level01 -> LevelBoss -> Level02 -> Creditos`

Implementation note: Only touch `Level01` and `LevelBoss` scene routing (plus build settings). Avoid edits to unrelated scenes to reduce merge conflicts with other teammates.

User response: In fact it was recommended to me to avoid working on our other scenes. I believe this is to avoid issues as there are other people working on other things in the project, in those scenes. If this is something we should indeed worry about, tell me the best practice for a case like this. If it's reasonably safe for us to make any necessary changes in other scenes without worring too much about "breaking" them or our game in general, then just ignore this. Have after levelboss the same scene that was after our current main game scene (where the enemies spawn, that can be completed, etc). I dont know the current flow. If it was scene1 > Creditos it should be scene1 > levelboss > Creditos. If it was scene1 > scene2 it should be scene1 > levelboss > scene2... if it was scene 2 > something... the same. So we should only include levelboss (our boss fight) between the current "main game" level, and the current next step, whatever it is

### UI-002 - Placeholder boss visuals

Decision: Use the provided placeholder sprites for the MVP.

Selected assets:

- Foot: `Assets/_Sprites/Boss/monty_python_foot_placeholder_2048.png`
- Shadow: `Assets/_Sprites/Boss/shadow_pc_soft_1024.png`

User response: Considering I have no experience in image making/generation/editing/etc, no graphical design bg or tools (aside whatever I can get/use for free online), I'm not sure on how to best proceed. Can you provide me with a guide/step by step instructions? Like, either look online for some assets we can import as placeholder already int he formats/ways we want... Or guide me trhough selecting an image (or AI generating one), converting it to our desired formats, transparency, etc... Anyway. Guide me in obtaining preparing and adding the image to the project

### UI-005 - Choose the post-MVP tone direction

Need from user: Decide whether the first boss should lean more funny and cartoonish or more oppressive and heavy once the mechanic works.

Why it matters: This affects timing, sound, hit feedback, and later art direction, but not the first playable pass.

How to respond:

1. Reply with `funny/cartoonish`, `oppressive/heavy`, or a short hybrid note.

Unblocked when: The tone direction is chosen or intentionally left open.

User response:
