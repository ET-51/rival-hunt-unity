# Rival Hunt

Rival Hunt is a small competitive gameplay system built in Unity and C#. The player collects
gems and levels up while a simulated Rival races through the same XP economy.

The project is inspired by the gem collection and leveling loop in Vampire Survivors. The twist
is that collecting gems does more than increase the player's level: recent pickups also slow the
Rival, turning normal XP collection into a readable race.

The assessment build uses a 60-second match and a 15-second Final Sprint so the complete loop can
be reviewed quickly. Both values are exposed in the Inspector for easy tuning.

## Playable build

Download the Windows build from the [latest GitHub release](https://github.com/ET-51/rival-hunt-unity/releases/latest).

## Technical document

The assessment write-up is available at [`Docs/Rival_Hunt_Technical_Document.pdf`](Docs/Rival_Hunt_Technical_Document.pdf).

## Unity version

Unity **6000.3.19f1** using the 2D Built-In Render Pipeline.

## How to open and run

1. Clone or download the repository.
2. Add the project folder through Unity Hub using Unity 6000.3.19f1.
3. Open `Assets/Scenes/Main.unity`.
4. Press Play.

## Controls

- `WASD` or arrow keys: move
- Walk into gems: collect XP
- Play Again button: restart after the match ends

## Core loop

1. Move through the arena and collect gems.
2. Gem XP fills the player bar and increases the player level.
3. The Rival gains XP automatically in the background.
4. Recent player pickups reduce the Rival's XP rate for 10 seconds, up to 40%.
5. A side that falls three levels behind receives a 15% rubber-band boost.
6. During the final 15 seconds, both sides earn double XP.
7. At 0:00, the higher level wins. Equal levels produce a tie.

## Architecture

The project uses seven scripts with one clear responsibility each:

- `PlayerController`: reads movement input and moves the player's `Rigidbody2D`.
- `Gem`: handles magnet movement, grants XP on contact, and reports the pickup event.
- `GemSpawner`: creates gems inside the arena and limits how many can exist at once.
- `XPSystem`: stores XP and levels. The same reusable component is used by the player and Rival.
- `RivalSimulator`: calculates the Rival's XP rate using starving, rubber-band, and sprint modifiers.
- `MatchController`: owns the timer, match state, player multipliers, and final result.
- `RaceUI`: displays bars, levels, timer, Final Sprint feedback, and the results panel.

The scripts communicate through small C# events. For example, gems announce when they are
collected, and `MatchController` announces when Final Sprint starts or the match ends. This keeps
the UI separate from the gameplay rules.

All balance values are serialized fields, so timings and XP rates can be adjusted in the
Inspector without changing code.


