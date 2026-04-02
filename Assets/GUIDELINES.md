# Scoundrel — Project Guidelines

> **Unity 6000.3 · URP 2D · DOTween · Input System**  
> A solo card dungeon-crawler. 44-card Scoundrel deck. Pure C# logic, event-driven views, state-machine turn loop.

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Layer Rules](#2-layer-rules)
3. [State Machine Contract](#3-state-machine-contract)
4. [Data & ScriptableObjects](#4-data--scriptableobjects)
5. [Naming Conventions](#5-naming-conventions)
6. [File & Folder Structure](#6-file--folder-structure)
7. [Event & Communication Rules](#7-event--communication-rules)
8. [View Layer Rules](#8-view-layer-rules)
9. [Animation Rules](#9-animation-rules)
10. [Service Locator Usage](#10-service-locator-usage)
11. [Adding New Features — Checklist](#11-adding-new-features--checklist)
12. [What Is Intentionally Stubbed](#12-what-is-intentionally-stubbed)
13. [Anti-Patterns to Avoid](#13-anti-patterns-to-avoid)

---

## 1. Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│  DATA LAYER          ScriptableObjects (CardSO)          │
│                      Enums (Suit, Rank, CardCategory)    │
├─────────────────────────────────────────────────────────┤
│  MODEL / LOGIC       Pure C# classes — no MonoBehaviour  │
│                      DeckManager, DungeonRoom,           │
│                      PlayerState                         │
├─────────────────────────────────────────────────────────┤
│  STATE MACHINE       GameStateMachine (MonoBehaviour)    │
│                      IGameState + concrete states        │
│                      GameContext (shared data bag)       │
├─────────────────────────────────────────────────────────┤
│  VIEW LAYER          MonoBehaviours that ONLY react      │
│                      RoomView, CardView, CardAnimator    │
├─────────────────────────────────────────────────────────┤
│  INFRASTRUCTURE      ServiceLocator, DeckLoader          │
└─────────────────────────────────────────────────────────┘
```

**Data flows downward. Events flow upward. No layer reaches across.**

---

## 2. Layer Rules

### Model / Logic (Pure C#)
- **No** `MonoBehaviour`, `UnityEngine.Object`, or scene references.
- **No** direct calls to the View layer.
- Communicate outward exclusively via **C# events** (`Action`, `Action<T>`).
- All state is private; expose read-only properties only.
- Constructor injection only — no `Find`, no `GetComponent`.

### State Machine
- States are **plain C# classes** (not MonoBehaviours).
- A state receives everything it needs through `GameContext` — never via `Find` or `ServiceLocator`.
- States **do not** instantiate GameObjects or touch the scene directly.
- Transitions are always initiated by calling `GameStateMachine.TransitionTo<T>()`.
- A state must be registered in `GameStateMachine.RegisterStates()` before it can be used.

### View Layer
- MonoBehaviours **only**. No game logic lives here.
- Views subscribe to model/state events in `Initialise()` or `OnEnable()` and unsubscribe in `OnDestroy()` or `OnDisable()`.
- Views call into states via the state's **public input API** (e.g. `PlayerChoiceState.SelectCard()`).
- Views never read `GameContext` directly.

### Infrastructure
- `ServiceLocator` is for **cross-cutting singletons** only (DeckManager, AudioManager, etc.).
- `DeckLoader` is the only class allowed to call `Resources.LoadAll`. Everything else uses injected references.

---

## 3. State Machine Contract

### IGameState
```csharp
public interface IGameState
{
    void Enter(GameContext context);
    void Exit(GameContext context);
}
```

### Rules
| Rule | Detail |
|------|--------|
| `Enter` does the work | All setup, logic, and immediate transitions happen in `Enter`. |
| `Exit` cleans up | Unsubscribe events, null out references, reset transient state. |
| No `Update` loop | States are event-driven. If polling is needed, add a dedicated `Tick(GameContext)` method and call it from `GameStateMachine.Update`. |
| One active state | `TransitionTo<T>` always calls `Exit` on the current state before `Enter` on the next. |
| Carry data via `SetX()` | If a state needs data from the previous state (e.g. `ResolvingState.SetCard()`), expose a typed setter and call it **before** `TransitionTo`. |

### Turn Loop (normal flow)
```
DrawingState → PlayerChoiceState ⇄ ResolvingState → (loop or GameOverState)
                     ↓
                 FleeState → DrawingState
```

---

## 4. Data & ScriptableObjects

### CardSO
- **Immutable at runtime.** Never add mutable fields to `CardSO`.
- One asset per card. 44 assets for the standard Scoundrel deck.
- `IsRemovedFromScoundrelDeck` is the single source of truth for deck filtering.
- Adding a new card type: add a value to `Suit`/`CardCategory`, update the `Category` switch, update `IsRemovedFromScoundrelDeck` if needed.

### Adding New ScriptableObjects
- Place under `Assets/Data/<Domain>/`.
- Use `[CreateAssetMenu(menuName = "Scoundrel/<Name>")]`.
- Keep them **data-only** — no logic, no coroutines, no scene references.
- If a SO needs to react to events, create a separate runtime wrapper class.

### Resources Folder
- Only `CardSO` assets live under `Resources/Data/Cards/` (loaded by `DeckLoader`).
- Everything else should be referenced directly or loaded via Addressables when that system is added.

---

## 5. Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Classes | PascalCase | `DeckManager`, `CardView` |
| Interfaces | `I` prefix + PascalCase | `IGameState` |
| Enums | PascalCase type, PascalCase values | `CardCategory.Monster` |
| Private fields | camelCase | `drawPile`, `currentHealth` |
| Serialized fields | camelCase | `[SerializeField] private int startingHealth` |
| Public properties | PascalCase | `DrawPileCount`, `IsDead` |
| Events | `On` prefix + PascalCase | `OnCardDrawn`, `OnDeath` |
| State classes | `<Name>State` suffix | `DrawingState`, `FleeState` |
| View classes | `<Name>View` suffix | `CardView`, `RoomView` |
| Animator classes | `<Name>Animator` suffix | `CardAnimator` |
| ScriptableObjects | `<Name>SO` suffix | `CardSO` |
| Prefabs | PascalCase, no suffix | `Card`, `RoomSlot` |
| Scenes | PascalCase | `Game`, `MainMenu`, `Bootstrap` |

---

## 6. File & Folder Structure

```
Assets/
├── Data/
│   └── Cards/              ← CardSO assets + CardEnums.cs + CardSO.cs
├── Resources/
│   └── Data/
│       └── Cards/          ← CardSO assets loaded at runtime by DeckLoader
├── Scenes/
│   ├── Bootstrap.unity     ← (future) service registration, loading screen
│   └── Game.unity          ← main gameplay scene
├── Scripts/
│   ├── Core/               ← Pure C# model classes (DeckManager, DungeonRoom)
│   ├── StateMachine/       ← IGameState, GameContext, GameStateMachine, all states
│   ├── Views/              ← MonoBehaviour view classes + animators
│   ├── UI/                 ← (future) HUD, menus, overlays
│   ├── Audio/              ← (future) AudioManager, SFX triggers
│   ├── Combat/             ← (future) CombatResolver
│   └── Infrastructure/     ← ServiceLocator, DeckLoader, Bootstrap
├── Sprites/
│   └── CuteCards - asset pack 0.1update/
├── Prefabs/
│   └── Cards/              ← Card prefab(s)
└── Settings/               ← URP renderer, scene templates
```

**One class per file. File name must match class name exactly.**

---

## 7. Event & Communication Rules

### Declaring Events
```csharp
// Always use Action / Action<T> — no UnityEvent in model classes
public event Action<int, int> OnHealthChanged;  // (current, max)
public event Action<CardSO>   OnCardDrawn;
public event Action           OnDeath;
```

### Firing Events
```csharp
// Always null-check with ?.Invoke()
OnHealthChanged?.Invoke(currentHealth, MaxHealth);
```

### Subscribing / Unsubscribing
```csharp
// Subscribe in Initialise() or OnEnable()
dungeonRoom.OnCardResolved += HandleCardResolved;

// Always unsubscribe in OnDestroy() or OnDisable()
dungeonRoom.OnCardResolved -= HandleCardResolved;
```

### Rules
- **Never** subscribe to an event without a matching unsubscribe.
- **Never** fire an event from inside a constructor.
- Events carry **data, not commands**. The subscriber decides what to do.
- If two systems need to communicate without a shared reference, route through `GameStateMachine.OnStateChanged` or add a dedicated event bus (see future work).

---

## 8. View Layer Rules

### CardView
- Receives its `CardSO` and `PlayerChoiceState` reference via `Initialise()` — never via `GetComponent` or `Find`.
- `isInteractable` must be `false` until the deal animation completes.
- Mouse input methods (`OnMouseDown`, `OnMouseEnter`, `OnMouseExit`) must guard on `isInteractable`.
- Calls `Destroy(gameObject)` only from inside an animation callback — never immediately.

### RoomView
- Owns the `List<CardView>` for the active room. No other class holds CardView references.
- Slot positions are set via `Transform[] cardSlots` in the Inspector — never hardcoded.
- `ClearRoom()` destroys all active views before dealing a new room.

### General
- Views are **stateless** with respect to game rules. They display what the model says.
- If a view needs to make a decision, that decision belongs in a state or model class.
- Prefer `Initialise(...)` over `Start()` for dependency injection in views.

---

## 9. Animation Rules

### CardAnimator
- All DOTween logic lives in `CardAnimator`. Zero tween calls in `CardView` or `RoomView`.
- Always call `Kill()` before starting a new tween on the same target.
- Completion callbacks (`onComplete`) are the only way to chain animation → game logic.
- Animation durations and eases are `private static readonly` constants at the top of the class.
- `hoverHeight` is in **world units** — keep it consistent with the camera's orthographic size.

### Adding New Animations
1. Add a `private static readonly float` duration constant.
2. Add a `public void Play<Name>(...)` method.
3. Call `Kill()` at the top of the method.
4. Store the result in `activeTween`.
5. Never return the `Tween` — callers use callbacks only.

---

## 10. Service Locator Usage

### When to Register
Register in a **Bootstrap** MonoBehaviour that runs before any gameplay scene loads:
```csharp
ServiceLocator.Register<DeckManager>(new DeckManager());
ServiceLocator.Register<AudioManager>(audioManager);
```

### When to Retrieve
Retrieve **once** at the start of a system's lifetime (e.g. `GameStateMachine.Start()`), then store the reference locally. Do not call `ServiceLocator.Get<T>()` every frame or inside loops.

### What Belongs in the Locator
| ✅ Register | ❌ Do Not Register |
|------------|-------------------|
| `DeckManager` | `PlayerState` (owned by GameContext) |
| `AudioManager` | `DungeonRoom` (owned by GameContext) |
| `ScoreManager` | `CardView` (transient, scene object) |
| `SaveManager` | Any state machine state |

---

## 11. Adding New Features — Checklist

### New Card Category / Effect
- [ ] Add value to `CardCategory` enum (if needed)
- [ ] Update `CardSO.Category` switch expression
- [ ] Add resolution branch in `ResolvingState.ResolveCard()`
- [ ] Add any new model methods to `PlayerState` or a new pure C# class
- [ ] Update `CardAnimator` if a new visual effect is needed
- [ ] Add `CardSO` assets for the new cards

### New Game State
- [ ] Create `<Name>State.cs` in `Assets/Scripts/StateMachine/`
- [ ] Implement `IGameState` (`Enter` + `Exit`)
- [ ] Register in `GameStateMachine.RegisterStates()`
- [ ] Define all transitions explicitly (what state comes after, under what condition)
- [ ] If the state needs input from the view, expose a typed public method (not a property setter)
- [ ] Document the state's role in the turn loop comment at the top of the file

### New View
- [ ] Create `<Name>View.cs` in `Assets/Scripts/Views/`
- [ ] Add `Initialise(...)` method — no `Find` or `GetComponent` for dependencies
- [ ] Subscribe to model/state events in `Initialise()`
- [ ] Unsubscribe in `OnDestroy()`
- [ ] If animation is non-trivial, create a companion `<Name>Animator.cs`

### New ScriptableObject Type
- [ ] Create `<Name>SO.cs` in `Assets/Data/<Domain>/`
- [ ] Add `[CreateAssetMenu]` attribute
- [ ] Keep it data-only
- [ ] If it needs to be loaded at runtime, add a loader class (follow `DeckLoader` pattern)

---

## 12. What Is Intentionally Stubbed

These are known incomplete areas. Do not work around them — implement them properly when the time comes.

| Stub | Location | What to Build |
|------|----------|---------------|
| `CombatResolver` | `ResolvingState.ResolveMonster()` | Separate pure C# class. Weapon vs. monster logic: weapon blocks damage if monster value < weapon value AND monster value > `weaponDurability`. After blocking, call `PlayerState.SetWeaponDurability(monster.Value)`. Taking bare damage unequips the weapon. |
| Game Over routing | `DrawingState.Enter()` | Currently kills the player directly. Should call `GameStateMachine.TransitionTo<GameOverState>()` via an injected reference or event. |
| Flee → DrawingState transition | `FleeState.Enter()` | Returns cards to deck but does not yet call `TransitionTo<DrawingState>()`. Needs `GameStateMachine` reference passed in or routed via event. |
| Victory condition | `GameOverState` | Victory is never triggered. Define the win condition (deck exhausted + player alive?) and fire it from `DrawingState` or a new `VictoryCheckState`. |
| Input System integration | `PlayerChoiceState` | `SelectCard()` and `RequestFlee()` are called directly by `CardView.OnMouseDown`. Wire to the Unity Input System action asset (`Assets/InputSystem_Actions.inputactions`) for controller/touch support. |
| Bootstrap scene | — | `DeckLoader` and `ServiceLocator` registration currently have no dedicated Bootstrap scene. Add one so services are available before `GameStateMachine.Start()` runs. |

---

## 13. Anti-Patterns to Avoid

| Anti-Pattern | Why | Instead |
|---|---|---|
| `GameObject.Find()` / `FindObjectOfType()` | Fragile, slow, couples to scene structure | Constructor injection or `Initialise()` |
| Game logic in `MonoBehaviour.Update()` | Polling is hard to reason about and test | Event-driven state transitions |
| Mutable fields on `CardSO` | SOs are shared assets — mutation causes bugs across all references | Keep SOs immutable; put runtime state in `PlayerState` or a wrapper |
| `UnityEvent` in model classes | Couples model to Unity serialization | Use `System.Action` events |
| `static` game state | Survives scene reloads, causes stale data bugs | Own state in `GameContext`; clear `ServiceLocator` on scene unload |
| Calling `TransitionTo` from inside `Enter` synchronously | Can cause re-entrant state changes | Use `StartCoroutine` or `Invoke` with 0-frame delay if an immediate transition is needed |
| Hardcoded card values in states | Breaks when card data changes | Always read from `CardSO.Value` |
| Destroying a `CardView` immediately on resolve | Skips the discard animation | Always destroy inside the animation `onComplete` callback |
