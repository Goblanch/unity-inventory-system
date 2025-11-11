# GB Inventory System - Technical Documentation

> **Status:** public beta
> **Summary:** modular, decoupled, and configurable inventory system for Unity.
> **Layers:** Domain (core), Application (sercice), Infrastructure (SO/Installers).
> **UI:** not coupled. The system exposes slot/stack views so you can build any UI.

---

## Index
1. [Introduction](#intro)
2. [Concept Dictionary](#dictionary)
3. [Architecture](#architecture)
4. [How to Implement](#implementation)
   - [Suggested Folder Structure](#folders)
   - [Conventions & Organization](#conventions)
   - [Scene Installation (Installer)](#installation)
5. [API Reference (inventory usage)](#api-reference)
6. [How to Connect a UI](#connect-ui)
7. [How to Apply Effects](#effects)

---

<a id="intro"></a>
## 1. Introduction

**GB Inventory System** is a Unity inventory framework that is **UI-agnostic**, **versatile**, and **scalable**.  
It is configured via **ScriptableObjects** (items, slot profiles) and governed by **policies** (stacking and filters) that you inject.  
The goal is to reuse it across projects **without touching the core**, adapting behavior through configuration.

---

<a id="dictionary"></a>
## 2. Concept Dictionary

- **Slot:** A cell in the inventory. May be empty (no `Stack`). Holds `Index` and `SlotProfileId`.
- **Stack:** A homogeneous pile of items sharing the same `DefinitionId`. Has `Count`.
- **ItemMeta:** Metadata of an item (TypeId, Tags, stack overrides).
- **SlotProfile:** Slot rules (AllowedTypes, RequiredTags, BannedTags, and optional stackability/MaxStack overrides).
- **IStackingPolicy:** Policy that determines the **base max per item** (e.g., `sword=1`, `stone=50`).
- **ISlotFilterPolicy:** Policy that validates whether a slot (profile) **accepts** an item and what the **effective max** is (minimum between item and profile limits).
- **InventoryModel:** Implementation of `IInventory`. Core logic: add/move/split/clear/capacity/profiles.
- **IInventoryService / InventoryService:** High-level façade used by gameplay/UI. Includes `TryUse` (effects/phases) optionally.
- **IEffectRegistry:** Resolves usage effects for items (optional).
- **IUsagePhasePolicy:** Validates whether an item can be used in the current phase/turn (optional).
- **UseResult:** Result of `TryUse` (Success, ConsumeOne, Message).
- **Installer (`InventoryInstaller`):** Scene component that composes providers, policies, model, and service.
- **Samples:** Demo scenes to validate integration (2D with profiles / free).

---

<a id="architecture"></a>
## 3. Architecture

The system is organized into **three layers**:

- **Domain (core):**
  - Pure inventory logic: slots, stacks, add/move/split/clear, capacity.
  - Policies: `IStackingPolicy` (max per item) and `ISlotFilterPolicy` (what a slot accepts and the effective max).
  - Main types: `InventoryModel`, `ISlot`, `IStack`, `ItemMeta`, `SlotProfile`.

- **Application (service):**
  - High-level façade for gameplay/UI: `IInventoryService` / `InventoryService`.
  - Exposes public API (add/move/split/clear/capacity/profiles) and **TryUse** (optional).
  - Integrates **effects** (`IEffectRegistry`) and **phases** (`IUsagePhasePolicy`) if your game requires them.

- **Infrastructure (Unity):**
  - ScriptableObject databases: `ItemDatabase`, `SlotProfileDatabase`.
  - Providers: `SoItemMetadataProvider`, `SoSlotProfileProvider`.
  - Scene installers: `InventoryInstaller`.
  - (Optional) `ScriptableEffectRegistry`.

**Principles:**
- Core is **UI-free** and free of engine dependencies.
- **Policy injection** to change behavior without touching the core.
- **Data-driven configuration** (SO) oriented to designers and content authors.

---

<a id="implementation"></a>
## 4. How to Implement

<a id="folders"></a>
### Suggested Folder Structure

> **Golden rule:** do **not** put your game content (items, profiles, scenes, icons) inside `Assets/InventorySystem`.  
> That folder is the **framework**. Keep your content outside so you can update the system without conflicts.

```bash
Assets/
├─ InventorySystem/ ← (framework) do NOT place your content here
│ └─ … (system core)
│
├─ Data/
│ └─ Inventory/
│ ├─ Items/
│ │ ├─ Definitions/ ← ItemDefinition*.asset (one per item)
│ │ ├─ Icons/ ← Sprites/Atlases for item icons
│ │ ├─ Payloads/ ← JSON/TextAssets per item (optional)
│ │ └─ Types/ ← (optional) ScriptableObject item types
│ │
│ ├─ SlotProfiles/
│ │ └─ Definitions/ ← SlotProfileDefinition*.asset (one per profile)
│ │
│ └─ Databases/
│ ├─ ItemDatabase.asset
│ └─ SlotProfileDatabase.asset
│
├─ Game/
│ └─ Inventory/
│ ├─ Runtime/
│ │ ├─ Installers/ ← Prefab(s) with configured InventoryInstaller
│ │ ├─ Effects/ ← Concrete IEffect classes + (opt.) EffectRegistry.asset
│ ├─ UI/ ← HUD, SlotView, icon DB, etc. specific to your game
│
└─ Tests/ ← (optional) your game tests using the system
```

---

<a id="conventions"></a>
### Conventions & Organization

**1) `Assets/Data/Inventory/Items/Definitions/`**  
- **What:** one `ItemDefinition.asset` per game item.  
- **Typical fields (as per your system):**
  - `DefinitionId` → **unique ID** (recommended: kebab-case lowercase, e.g., `card-memory`, `wood`, `iron-sword`)
  - `Type` → "Card", "Object", "Resource", "Tool"… (or a type SO if you use one)
  - `Tags[]` → "Consumable", "Material", "Tool", etc.
  - (Optional) `EffectKey` → key for `EffectRegistry`
  - (Optional) `PayloadJson` → `TextAsset` with JSON for the effect
  - (Optional) `HasStackOverride` / `MaxStack` → if the item enforces its own stack limit (e.g., sword = 1)
- **Naming conventions:**
  - Asset name = `Item_<DefinitionId>.asset` (e.g., `Item_wood.asset`)
  - `DefinitionId` **unique and stable** (avoid renaming later)

**2) `Assets/Data/Inventory/Items/Icons/`**  
- **What:** sprites/atlas for item icons.  
- **Suggested convention:** `sprite.name == DefinitionId` (so `ItemIconDatabase` can resolve directly).  
- If you use an atlas, create an `ItemIconDatabase.asset` mapping `DefinitionId → Sprite`.

**3) `Assets/Data/Inventory/Items/Payloads/`**  
- **What:** per-item `TextAsset` JSON for effects (if used).  
- **File name:** `<DefinitionId>.json` (e.g., `card-memory.json`).  
- **Example contents:**
```json
{ "amount": 10, "radius": 2 }
```

**4) `Assets/Data/Inventory/Types/` (optional)**
* **What:** type ScriptableObjects (if you don't use plain strings).
Example: `ItemType_Resource.asset` with `TypeId = "Resource`.

**5) `Assets/Data/Inventory/SlotProfiles/Definitions/`**
* **What:** one `SlotProfileDefinition.asset` per slot profile.
* **Typical fields:**
    * `Id` → "Any", "Materials", "Consumables", "Weapons"... (PascalCase)
    * `AllowedTypes[]` (empty = any)
    * `RequiredTags[]` (all must be present)
    * `BannedTags[]`
    * Overrides:
        * `HasStackableOverrides` + `StackableOverride` (if `false` → not stackable → max 1)
        * `MaxStackOverride` (>0 clamps the stack per slot)
* **Convention:** `SlotProfile_<Id>.asset` (e.g., `SlotProfile_Materials.asset`)

**6) `Assets/Data/Inventory/Databases/`**
* **What:** databases consumed by the `Installer`:
    * `ItemDatabase.asset` → list of `ItemDefinition`
    * `SlotProfileDatabase.asset` → list of `SlotProfileDefinition`
* These DBs rebuild index in `OnValidate/OnEnable` (alreadey implemented)

**7) `Assets/Game/Inventory/Runtime/Installers/`**
* **What:** Prefab with `InventoryInstaller` configured:
    * `ItemDatabase` → reference the asset above
    * `SlotProfileDatabase`
    * `initialCapacity` → initial capacity.
    `defaultSlotProfileId` → default profile per slot when building the inventory (e.g., `"Any"` or `"Consumables"`)
    * (Optional) `EffectRegistry` if you will use `TryUse`
* **How to use:** place this prefab in any scene that needs inventory.

**8) `Assets/Game/Inventory/Runtime/Effects/`**
* **What:** you concrete `IEffect` classes and, if desired, a `ScriptableEffectRegistry.asset`.
* **Convention:** `Effect_<Name>.cs` and `EffectRegistry.asset` mapping `EffectKey → IEffect`-

**9) `Assets/Game/Inventory/UI/`**
* **What:** your actual HUD/UI:
    * Presenter, SlotView, DragGhost, `ItemIconDatabase.asset`, etc.
* **Note:** this folder is yours; the system does not enforce structure.

---

<a id="installation"></a>

## Scene Installation (Installer)

1. Create an empty **GameObject** in your scene (e.g., `InventoryRoot`).
2. Add the `InventoryInstaller` component (or your own derivd installer).
3. Assign:
    * `ItemDatabase` (asset)
    * `SlotProfileDatabase` (asset)
    * `initialCapacity`
    * `defaultSlotProfileId`
    * (Optional) `EffectRegistry` (SO) if you will use `TryUse`
4. At runtime, access the service:
```csharp
var svc = FindObjectOfType<InventoryInstaller>().Service;
```

---

<a id="api-reference"></a>

## 5. API Reference (inventory usage)

Main interface: `IInventoryService` (Application layer).
Key operations (funtional summary):
```csharp
int Capacity { get; }
IReadOnlyList<GB.Inventory.Domain.Abstractions.ISlot> SlotsView { get; }
// In some builds, you might expose a slot view where an empty slot has Stack == null.
// Use null-safe checks when reading.

bool TryAdd(string definitionId, int count, out int slotIndex, out string reason);
// Merge-first, then place remainder into the first compatible empty slot.
// Returns true if EVERYTHING fits; false if partial or nothing fits.

bool TryMove(int srcSlot, int dstSlot, out string reason);
// Same item → merge up to the effective max.
// Different items → swap if both profiles accept.

bool TrySplit(int slotIndex, int count, out int newSlotIndex, out string reason);
// Splits 'count' units into the first compatible empty slot. Reverts if none is available.

bool TryClear(int slotIndex, out string reason);
// Clears a slot (if it contains a stack).

bool TrySetSlotProfile(int slotIndex, string slotProfileId, out string reason);
// Changes the slot profile (affects future operations and effective max).

string GetSlotProfileId(int slotIndex);
// Returns the slot’s current profile.

bool SetCapacity(int newCapacity, out string reason);
bool IncreaseCapacity(int delta, out string reason);
// Increase adds empty slots; decreasing requires truncated slots to be empty.

bool TryUse(int slotIndex, ITurnContext ctx, out UseResult result, out string reason);
// (Optional) Resolves and applies the item’s effect, validates phase if present,
// and consumes 1 unit when the effect indicates so (result.ConsumeOne).
```

**Usage notes:**
* **Empty slots:** depending on your build, a slot can be "empty" if `slot == null` OR `slot.Stack == null`. Handle safely.
* **Effective max** `ISlotFilterPolicy` combines `IStackingPolicy.GetMaxPerStack` + profile overrides → final clamp.
* **Error messages:** methods return `reason` explaining rejections or partial operations.

---

<a id="connect-ui"></a>
## 6. How to Connect a UI

The system does not impose a UI. Typical flow:
1. **HUD Presenter (MonoBehaviour)**
    * Expose references to a `Grid/Transform` and a `SlotView` prefab.
    * `Bind(IInventoryService svc)`: cache the service and build the grid.
    * `Refresh()`: iterate `svc.SlotsView`:
        * If empty → `SlotView.RenderEmpty()`
        * If it has a stack → `SlotView.Render(defId, count, icon)`
2. **SlotView (prefab per slot)**
    * Shows `icon`, `count` (if > 1).
    * Implement `IBeginDrag/IDragHandler/IEndDrag` for drag & drop (optional).
    * In `OnDrop`, call `_svc.TryMove(srcIndex, dstIndex, out reason)`
3. **Icons**
    * `ItemIconDatabase` (simple SO) mapping `DefinitionId → Sprite`.

**Minimal binding example:**
```csharp
public class InventoryHUDPresenter : MonoBehaviour
{
    [SerializeField] Transform slotsGrid;
    [SerializeField] SlotView slotPrefab;
    [SerializeField] ItemIconDatabase iconDb;

    IInventoryService _svc;
    List<SlotView> _views = new();

    public void Bind(IInventoryService service)
    {
        _svc = service;
        BuildGrid();
        Refresh();
    }

    void BuildGrid()
    {
        // Instantiate N = _svc.Capacity
    }

    public void Refresh()
    {
        var view = _svc.SlotsView;
        for (int i = 0; i < _views.Count; i++)
        {
            var slot = view[i];
            if (slot == null || slot.Stack == null)
                _views[i].RenderEmpty();
            else
                _views[i].Render(slot.Stack.DefinitionId, slot.Stack.Count,
                                 iconDb ? iconDb.GetIcon(slot.Stack.DefinitionId) : null);
        }
    }
}
```

**Drag & Drop (summary):**
* `BeginDrag`: create a ghost image following the cursor.
* `OnDrop`: (on the destination `SlotView`): `_svc.TryMove(src, dst, out reason)`.
* `EndDrag`: destroy ghost and `Refresh()`.

---

<a id="effects"></a>
## 7. How to Apply Effects

**Goal:** `TryUse(slotIndex, ctx, out result, out reason)` executes the item's effect logic.
* **IEffectRegistry:** resolves an `IEffect` for a `DefinitionId` and (optionally) its payload.
* **IEffect:** `UseResult Apply(ITurnContext ctx, string, definitionId, object payload)`.
* **IUsagePhasePolicy (optional):** checks if the item can be used in the current phase.
* **UseResult:** indicates success/failure and wether to consume one unit.

`TryUse` flow:
1. Validate index and slot not empty.
2. (Optional) Phase: `IUsagePolicy.CanUse`.
3. `IEffectRegistry.TryResolve(defId, out effect)`
4. `IEffectRegistry.TryGetPayload(defId, out payload)`
5. `effect.Apply(ctx, defId, payload)`.
6. If `Success && ConsumeOne` → subtract 1 (clear or split + clear).

**Configuration:**
* In **ItemDatabase**, fill EffectKey for items that hace an effect.
* In your `IEffectRegistry`, map `EffectKey → IEffect` and resolve `payload` (JSON text or a parsed object).

**Payload example (JSON):**
```json
{"amount": 10, "radius": 2}
```

**Simplified example:**
```csharp
public sealed class TestEffect : IEffect
{
    public UseResult Apply(ITurnContext ctx, string definitionId, object payload)
    {
        // payload can be a JSON string or a pre-parsed object
        return UseResult.Ok(consumeOne: true, message: "Test effect applied");
    }
}
```

**Notes:**
* If you don't need effects → don't assign an `EffectRegistry`, and don't call `TryUse`.
* If you need phases → implement `IUsagePolicy`.