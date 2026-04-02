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
- New Input System — touch/mouse
- ScriptableObjects — card data

## Conventions
- `camelCase` for private fields (no underscore prefix)
- XML doc comments on all public API methods
- Pure C# for all game logic (no MonoBehaviour in core/state layers)

---

## Architecture

```
Data layer        CardSO, CardEnums — immutable definitions, no runtime state
Core logic        DeckManager, PlayerState, DungeonRoom, GameContext — pure C#
State machine     GameStateMachine + states — drives the turn loop
Views             CardView, RoomView, CardAnimator — MonoBehaviours, visuals only
Infrastructure    ServiceLocator, DeckLoader — startup wiring
```

### Scene structure
- **Bootstrap** — registers services, loads card assets, transitions to Game scene
- **Game** — GameStateMachine creates GameContext and starts the turn loop

---

## Progress

### Step 1 — Data + Deck ✅
- `CardEnums.cs` — Suit, Rank, CardCategory enums. Joker reserved but inactive.
- `CardSO.cs` — ScriptableObject per card. Suit → category, rank → value. Auto-filters red Aces. Has internal Init() for tests.
- `DeckManager.cs` — Fisher-Yates shuffle, Draw, DrawMultiple, Discard, ReturnToDeck (inserts at bottom for flee). Events: OnDeckShuffled, OnCardDrawn, OnDeckExhausted.
- `DeckLoader.cs` — loads all CardSO assets from Resources/Data/Cards/ and initialises DeckManager.

### Step 2 — State Machine + Core Logic ✅
- `IGameState.cs` — Enter(GameContext), Exit(GameContext)
- `GameContext.cs` — holds DeckManager, PlayerState, DungeonRoom. Passed to every state.
- `GameStateMachine.cs` — MonoBehaviour. Creates context, registers states, TransitionTo<T>().
- `PlayerState.cs` — HP, equipped weapon + durability. Events: OnHealthChanged, OnWeaponChanged, OnDeath.
- `DungeonRoom.cs` — 4 active cards, HasFled flag. Events: OnRoomDealt, OnCardResolved, OnRoomCleared.
- `DrawingState.cs` — draws 4 cards into the room. Kills player if deck empty.
- `PlayerChoiceState.cs` — waits for SelectCard() or RequestFlee() calls from input. Flee valid only after 1+ card resolved and room not previously fled.
- `ResolvingState.cs` — applies card effect. Monster = stub damage (replaced in step 4). Potion = heal. Weapon = equip.
- `FleeState.cs` — returns remaining cards to deck bottom.
- `GameOverState.cs` — fires OnGameOver with Death or Victory result.
- `ServiceLocator.cs` — static Register<T> / Get<T>.

### Step 3 — Views + Animations ✅
- `CardAnimator.cs` — pure C#. DOTween sequences: deal arc (staggered), flip (scaleX swap), discard (move + fade), hover enter/exit, invalid tap punch.
- `CardView.cs` — MonoBehaviour on card prefab. Initialise → DealToSlot → Discard. OnMouseDown/Enter/Exit handle input. Exposes CardData for RoomView matching.
- `RoomView.cs` — manages 4 slot Transforms, deck + discard positions. Subscribes to DungeonRoom events, instantiates/destroys CardViews.

---

## Remaining Steps

### Step 4 — CombatResolver 🔲
Weapon damage logic and durability rules:
- Weapon can fight a monster only if monster value < weapon value
- After fighting, weapon durability = defeated monster's value (can only fight weaker monsters after)
- Player can always fight barehanded (take full damage) regardless of weapon
- Potions only heal if the previous card resolved was not also a potion

### Step 5 — HUD 🔲
UI Toolkit — HP bar, deck counter, weapon slot display

### Step 6 — Polish 🔲
- Screen shake via Cinemachine impulse on damage
- Audio: AudioMixer with SFX + music groups, card sounds, low HP heartbeat
- Final juice pass