# GB Inventory System

A **modular, decoupled, and scalable inventory framework** for Unity.
Designed to be reused across multiple games with minimal coupling and high configurability through **ScriptableObject databases** and **policy-driven architecture**.

---

## 🧩 Overview

The **GB Inventory System** is a generic inventory core built around **clean separation of concerns**:

- **Domain Layer** - defines pure logic for slots, stacking, and profiles.
- **Aplication Layer** - exposes a simplified `IInventoryService` for gameplay, UI, and scripting.
- **Infrastructure Layer** - bridges Unity assets (`ScriptableObjects`) to the domain.
- **Samples Layer** - provides example scenes demonstrating how to integrate the system in 2D context.

The system can handle both **profile-driven inventories** (e.g., "Consumables", "Materials") and **free-form inventories** (e.g., "Any" type slot systems).
It is **independent from UI** and **Unity serialization**, making it portable, testable, and adaptable.

---

## 🚀 Features

- ✅ **Profile-based filtering** - restricts what items can go into each slot.
- ✅ **Per-item and per-profile stacking** - flexible stack limits and rules.
- ✅ **Dynamic resizing** - increase or decrease capacity safety at runtime.
- ✅ **Full policy-driven design** - slot filters, stacking logic, and metadata all injectable.
- ✅ **Effect system ready** - optional integration with gameplay "use" effects.
- ✅ **ScriptableObject databases** - easy item and profile configuration via the editor.
- ✅ **Automated unit tests** - covers all domain operations (add, move, split, resize...).
- ✅ **Samples included**:
    - `2D_ProfilesDemo` - inventory with distinct slot profiles.
    - `2D_AnyInventory` - generic, unrestricted inventory demo.

---

## 🏗️ Architecture

The project follows **DDD-inspired (Domain-Driven Design)** approach:

```
InventorySystem/
│
├── Domain/
│ ├── Abstractions/ # Core interfaces (IInventory, ISlot, IStack, IStackingPolicy...)
│ ├── Policies/ # Domain policies (stacking & slot filtering)
│ ├── ValueObjects/ # ItemMeta, SlotProfile
│ ├── Slot.cs # Internal slot implementation
│ ├── Stack.cs # Internal stack implementation
│ └── InventoryModel.cs # Core domain logic
│
├── Application/
│ ├── Abstractions/ # High-level contracts (IInventoryService, IEffectRegistry...)
│ ├── InventoryService.cs
│ └── ValueObjects/ # UseResult, ITurnContext...
│
├── Infrastructure/
│ ├── Definitions/ # ScriptableObject assets: ItemDatabase, SlotProfileDatabase
│ ├── Providers/ # Bridges: SoItemMetadataProvider, SoSlotProfileProvider
│ ├── Installers/ # MonoBehaviours to wire everything (InventoryInstaller)
│ └── Effects/ # (optional) ScriptableEffectRegistry, for item effects
│
├── Samples/
│ ├── 2D_ProfilesDemo/ # Demo scene with slot profiles ("Consumables", "Materials")
│ ├── 2D_FreeInventory/ # Demo scene with unrestricted slots ("Any" profile)
│ └── Common/ # Shared UI prefabs (SlotView, HUDPresenter, DebugPanel)
│
└── Tests/
├── Domain/ # Unit tests for InventoryModel, stacking, moving...
├── Infrastructure/ # Tests for SO providers
└── Application/ # Tests for usage and effects
```

---

## ⚙️ Core Concepts

| Concept | Description |
|----------|--------------|
| **Slot** | A cell in the inventory that may hold a stack or be empty. |
| **Stack** | A quantity of items with the same definition ID. |
| **ItemMeta** | Metadata describing type, tags, and stack rules for an item. |
| **SlotProfile** | Defines acceptance rules (types/tags) and optional stack overrides. |
| **IStackingPolicy** | Determines per-item max stack (e.g., via item metadata). |
| **ISlotFilterPolicy** | Determines whether a slot can accept a given item and computes effective max. |
| **InventoryModel** | Domain-level class implementing all core logic. |
| **InventoryService** | High-level façade for gameplay and UI to manipulate the model. |
| **InventoryInstaller** | MonoBehaviour that wires up the entire system from ScriptableObjects. |

## 🧠 Design Philosophy

The system is built around three goals:

1. **Decoupling:**
    No hard references to UI, UnityEvents, or MonoBehaviours in domain logic. The `InventoryModel` can be fully unit-tested without Unity dependencies.

2. **Configurability:**
    Items, profiles, and policies are data-driven via ScriptableObject databases. This allows designers (and users) to change inventory behavior without touching code.

3. **Extensibility:**
    New stacking policies, slot filters, or use effects can be implemented and injected without rewriting existing logic.

---

## 🧪 Testing

The system is fully covered by unit tests under `/Tests/Domain` and `/Tests/Infrastructure`.
Tests validate:
- Add / Split / Move / Clear.
- Stack merging and limits.
- Profile filtering.
- Capacity increase/decrease safety.
- Provider lookups and runtime consistency.

All tests use NUnut and run directly inside the Unity Test Runner.

---

## 🧰 How to Use

1. **Create Databases**
    - `ItemDatabase`: holds all item definitions (ID, type, tags, optional stack limit).
    - `SlotProfileDatabase`: holds slot profiles (rules for acceptance and stacking).

2. **Add an Installer**
    - In yout scene, add an `InventoryInstaller` component.
    - Assign both databases and (optionally) an `EffectRegistry` ScriptableObject.
    - Configure default capacity and default profile ID.

3. **Access the Inventory**
```csharp
   var svc = FindObjectOfType<InventoryInstaller>().Service;
   svc.TryAdd("wood", 10, out _, out var reason);
   Debug.Log(reason);

   // Other ways: e.g., with ServiceLocator registration.
```

## 🧱 Dependencies and Extensibility

- **Policies:** implement `IStackingPolicy` or `ISlotFilterPolicy` to customize rules.
- **Effects:** implement `IEffect` and register it via `IEffectRegistry` for item usage logic.
- **Turn Contexts:** The `ITurnContext` abstraction allows your game state (turns, phases, etc.) to incluence what can be used.

---

## 📚 Samples

| Scene                | Description                                                                    |
| -------------------- | ------------------------------------------------------------------------------ |
| **2D_ProfilesDemo**  | Demonstrates multiple slot profiles with filters (`Consumables`, `Materials`). |
| **2D_FreeInventory** | Demonstrates unrestricted slots (“Any” profile) and drag & drop behavior.      |


---

## 👤 Author
Developed by Gonzalo B.
Computer Science student and Unity Game Programmer - Madrid, Spain.
Focus: creating reusable gameplay systems and frameworks for Unity-based titles.

---

>💡 **Tip:**
This system was built to serve as a foundation for diverse gameplay genres — from survival inventories to collectible systems or card-based mechanics.
Extend the model, plug in your own data, and build on top of it without rewriting the core.