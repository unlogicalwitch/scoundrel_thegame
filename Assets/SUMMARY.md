# Scoundrel — Unity 6 Mobile Card Game

## Concept
Solo dungeon-crawler card game based on the original Scoundrel ruleset by Zach Gage & Kurt Bieg. A standard 52-card deck minus the two red Aces (44 cards total) is used as a dungeon. The player fights through rooms of 4 cards, managing HP and a weapon slot to survive.

## Goals
- Complete, playable core loop for personal use
- Clean flat/minimalist art (card assets already exist)
- Good game feel: satisfying animations, audio feedback, screen shake
- Android first, portable to iOS and desktop later

---

## Tech Stack
- Unity 6
- DOTween — card animations
- UI Toolkit — HUD
- Old Input System — touch/mouse (Input Manager)
- ScriptableObjects — card data

## Conventions
- `camelCase` for private fields (no underscore prefix)
- XML doc comments on all public API methods
- Pure C# for all game logic (no MonoBehaviour in core/state layers)

Scoundrel — Unity 6 Mobile Card Game
Current State
Core loop is functional. Cards deal, animate, player can tap/drag cards, flee works with swipe down, camera shake implemented via DOTween.
Architecture

Data — CardSO, CardEnums (Suit/Rank/CardCategory, Joker reserved)
Core — DeckManager, PlayerState, DungeonRoom, GameContext, CombatResolver (stub)
State Machine — GameStateMachine, DrawingState, PlayerChoiceState, ResolvingState, FleeState, GameOverState
Views — CardView, RoomView, CardAnimator, ZoneView, DragResolver, WeaponSlotView (pending), HPView, DrawDeckView
Infrastructure — ServiceLocator, DeckLoader

Key Decisions Made

Android first, portable later
Old Input System (not New)
World-space GameObjects for cards and HUD text (TextMeshPro 3D)
UI Toolkit only for drop zones
DOTween for all card animations and camera shake
Drag-to-zone interaction for card actions
Swipe down for flee (blocked if started on card)
Potions limited to once per room (PotionUsedThisRoom flag on DungeonRoom)
Flee only valid if no cards touched yet (RemainingCount == 4)
No fleeing two rooms in a row (FledLastRoom on PlayerState, pending)

Conventions

camelCase private fields
Pure C# for core/state layers
XML doc comments on public API

Game Rules Implemented

44-card deck (52 minus red Aces)
Monsters deal damage equal to value
Potions heal equal to value, once per room
Weapons equip to slot, old weapon discards
Flee returns cards to deck bottom
Weapon fights monster only if weapon value > monster value

Currently Working On
Win/Lose conditions + scoring

Lose — HP hits 0
Win — DrawingState tries to deal but fewer than 4 cards remain → discard leftovers → trigger win
Score — not yet defined, need to decide what counts

Remaining Steps

Win/lose + scoring — define score formula, build end screen
CombatResolver — weapon durability, barehanded vs weapon choice path
WeaponSlotView — weapon card flies to slot, old weapon flies to discard
Polish — audio (AudioMixer, card sounds, low HP heartbeat), screen shake on damage, damage number popups, card scale on drag pickup, zone pulse animation, HP color shift

### Unity project notes
- Input Handling: Input Manager (Old) — set in Project Settings → Player
- Cards: world-space GameObjects with SpriteRenderer + Collider2D
- HUD: UI Toolkit (unaffected by camera shake)

---

## Flee Rules (Scoundrel)
- Can only flee if no cards touched yet in the room (RemainingCount == 4)
- Cannot flee two rooms in a row (FledLastRoom flag — to be added to PlayerState in step 4)