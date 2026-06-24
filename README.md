# Dragon Slayer

> A 2D mobile **Hack-&-Slash Roguelike** (Vampire-Survivors-like) built in Unity — survive escalating waves, level up to draft random upgrades, and defeat the boss.

![Unity](https://img.shields.io/badge/Unity-2021.3.25f1-black?logo=unity)
![Language](https://img.shields.io/badge/Language-C%23-239120?logo=c-sharp)
![Platform](https://img.shields.io/badge/Platform-Android%20%2F%20Mobile-3DDC84?logo=android)
![Genre](https://img.shields.io/badge/Genre-Roguelike%20%7C%20Hack%20%26%20Slash-orange)

This is my first game — a mobile hack-&-slash roguelike created with Unity. You control a lone warrior who is swarmed by monsters; you auto-attack, dodge with a dash, collect EXP to level up, and pick from randomized upgrade cards to build a run. Survive long enough and a boss appears — beat it to win.

![gameplay](https://github.com/junsangyooo/DragonSlayer/assets/70479629/003d9354-c9d4-469d-9d41-aa7afad30861)
![gameplay](https://github.com/junsangyooo/DragonSlayer/assets/70479629/7aaa0164-1c84-47f1-9833-628d7350decf)

---

## Table of Contents
- [Gameplay Loop](#gameplay-loop)
- [Features](#features)
- [Controls](#controls)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Roadmap](#roadmap)
- [Credits](#credits)

---

## Gameplay Loop

```
Lobby ──▶ Cave (run start)
  │           │
  │           ├─ Move (joystick) + Dash, auto-attack the nearest enemy
  │           ├─ Enemies die ──▶ drop EXP gems & gold
  │           ├─ Collect EXP ──▶ Level Up ──▶ draft 1 of 3 random upgrade cards
  │           ├─ Difficulty ramps every 45s (tougher tiers, faster spawns)
  │           ├─ Survive to 3:00 ──▶ Boss spawns (regular spawns stop)
  │           │        ├─ Defeat boss ──▶ VICTORY
  │           │        └─ HP reaches 0 ──▶ DEFEAT
  │           └─ Run end ──▶ collected gold banked to meta progression
  │
  └──◀ Return to Lobby ──▶ spend gold on permanent upgrades (Shop)
```

The run always resolves in a win or a loss, then loops back to the lobby where banked gold buys **permanent** upgrades that carry into future runs.

## Features

- **Auto-battler combat** — the player automatically fires at the nearest enemy on an attack-speed timer; the player focuses on positioning and dodging.
- **Roguelike level-up drafting** — on level up, the game pauses and presents **3 random upgrade cards** drawn from a pool of **14** options (5 weapons + 9 stat buffs). The card UI is generated at runtime over a hand-crafted, transparent card frame.
- **5 weapons**, each with its own behavior and scaling:
  | Weapon | Behavior |
  | --- | --- |
  | **Fireball** | Fires projectiles at the nearest enemy; levels add more projectiles in a fan. |
  | **Fire Barrier** | A persistent burning aura that damages enemies around the player. |
  | **Thunder** | Periodically strikes enemies with lightning bolts. |
  | **Black Hole** | Spawns a singularity that pulls enemies in and crushes them. |
  | **Horn Wave** | Fires piercing crescent sword-waves in the aim direction. |
- **9 stat buffs** — weapon damage, max HP, move speed, attack speed, crit chance, crit damage, pickup magnet range, projectile speed, and invincibility window.
- **Enemies & boss** — 4 enemy types (Bat, Spider, Slime, One-Eye) across difficulty tiers, plus a **boss** that ends the run.
- **Wave spawner with difficulty ramp** — timed spawns that intensify (stronger tiers + faster cadence) every 45 seconds until the boss.
- **Meta progression** — gold persists between runs (via `PlayerPrefs`) and is spent in a **lobby shop** on permanent Max HP / Weapon Damage / Move Speed upgrades.
- **Full run lifecycle** — pause, win/lose detection, runtime VICTORY/DEFEAT screens with **Retry** (reload the run) and **Return to Lobby**.
- **Procedural audio** — SFX (death, pickup, level-up, hurt, victory, defeat) and a soft BGM loop are generated in code (no external audio assets).

## Controls

| Input | Action |
| --- | --- |
| On-screen joystick | Move |
| Dash button | Dash (with cooldown) |
| — | Attacks fire automatically at the nearest enemy |
| Pause button | Pause / resume |

## Tech Stack

- **Engine:** Unity **2021.3.25f1** (2D feature set)
- **Language:** C#
- **UI:** Unity uGUI + TextMeshPro
- **Input:** legacy Input Manager + [Joystick Pack](https://assetstore.unity.com/) on-screen joystick
- **Target:** Android / mobile (portrait)
- **Persistence:** `PlayerPrefs`
- **Version control:** Git (+ Git LFS for `.psd` source art)

## Architecture

The codebase favors lightweight, self-bootstrapping systems so most UI and content is created at runtime (minimal scene wiring):

- **Singletons** — `GameManager` (run state, HUD, pause, win/lose), `Player` (movement, health, dash, leveling, pickups), `UpgradeManager` and `AudioManager` (auto-attached at runtime).
- **`IDamageable` interface** — every enemy and the boss implement `TakeDamage(float)`, so all weapons deal damage through one uniform path (`Physics2D.OverlapCircle` queries or trigger hits) regardless of enemy type.
- **Runtime-generated UI** — the level-up cards, the victory/defeat screens, and the lobby shop are all built in code (`Canvas` + `CanvasScaler` + `GraphicRaycaster`), so they require no per-scene setup and can't break from missing Inspector references.
- **Data-driven upgrade pool** — upgrades are defined as a code catalog (name, icon label, description, max level, apply-action), making the draft pool easy to extend.
- **Procedural content** — weapon visuals are code-generated circle sprites and all audio clips are synthesized at runtime, keeping the project asset-light and Unity-editor-independent for logic changes.

## Project Structure

```
Assets/
├─ Scripts/
│  ├─ GameManager.cs        # run state, HUD, pause, win/lose, run-end screens
│  ├─ Player.cs             # movement, health, dash, pickups, leveling, meta apply
│  ├─ WeaponsAndBuffs.cs    # auto-attack + the 5 weapons + buff stats
│  ├─ UpgradeManager.cs     # level-up card draft (runtime UI, 14-entry pool)
│  ├─ EnemySpawner.cs       # timed wave spawner, difficulty ramp, boss spawn
│  ├─ EnemyBoss.cs          # boss (assembled at runtime)
│  ├─ MetaProgress.cs       # PlayerPrefs meta progression (gold + permanent upgrades)
│  ├─ AudioManager.cs       # procedural SFX/BGM
│  ├─ BasicAttack.cs, EXP.cs, Gold.cs, CameraMove.cs
│  ├─ Enemies/              # EnemyBat / EnemySpider / EnemySlime / EnemyOneEye (IDamageable)
│  ├─ Weapons/              # IDamageable, WeaponVisuals, DamageProjectile, FireBarrier, BlackHole, FadeAndDie
│  └─ Lobby/Main.cs         # main menu + runtime shop
├─ Resources/UI/            # upgrade card frame sprite (loaded at runtime)
├─ Scenes/                  # Lobby.unity, Cave.unity
├─ Prefabs/                 # player, enemies, weapons, EXP/gold, lobby
└─ Sprites/ , Assets2/      # art, UI kits, effect packs
```

## Getting Started

**Requirements:** Unity **2021.3.25f1** (install via Unity Hub) and Git with [Git LFS](https://git-lfs.com/) (the repo stores `.psd` source art via LFS).

```bash
git lfs install
git clone https://github.com/junsangyooo/dragon_slayer.git
```

1. Open the project in Unity **2021.3.25f1**.
2. Open `Assets/Scenes/Lobby.unity` and press **Play** (build settings: `Lobby` = scene 0, `Cave` = scene 1).
3. To build for Android: `File ▸ Build Settings ▸ Android ▸ Build`.

## Roadmap

- [ ] Replace code-generated weapon visuals and procedural audio with authored art/sound
- [ ] Localized (Korean) UI text once a Korean TMP font is added
- [ ] Continuous BGM across scene transitions
- [ ] Refactor the four enemy scripts into a shared `EnemyBase`
- [ ] Physics-based movement (`Rigidbody2D.MovePosition`) to remove frame-rate-dependent motion

## Credits

Built by **junsangyooo**. Uses third-party Unity asset packs (Dragon Warrior, 2D Casual UI, Dark UI, Slime, 2D Monsters Pack, Joystick Pack) for art and UI; all gameplay code is original.

<!--
Development Journal: https://handsomely-witness-6c4.notion.site/Dragon-Slayer-Game-Development-journal-72e7a679915c4ae99afb4791e9ef02ec
-->
