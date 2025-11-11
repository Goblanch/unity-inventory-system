# GB Inventory System - Documentación Técnica

> **Estado**: beta pública
> **Resumen**: sistema de inventario modular, desacoplado y configurable para Unity.
> **Capas**: Domain (núcleo), Application (servicio), Infrastructure (SO/Installers).
> **UI**: No acoplada (el sisteme expone vistas de slot/stacks para que puedas construir tu propia UI).

---

## Índice

1. [Introducción](#intro)
2. [Diccionario de conceptos](#dictionary)
3. [Arquitectura](#architecture)
4. [Cómo implementarlo](#implementation)
    - [Estructura de carpetas sugeridas](#folders)
    - [Convenciones y organización](#conventions)
    - [Instalación por escena (Installer)](#instalation)
5. [API Reference (uso del inventario)](#api-reference)
6. [Cómo conectarlo a una UI](#connect-ui)
7. [Cómo aplicar effectos](#effects)

---
<a id="intro"></a>
## 1. Introducción

**GB Inventory System** es un framework de inventario para Unity **desacoplado de la UI**, **polivalente** y **escalable**. Se configura con **ScriptableObjects** (items, perfiles de slot) y se rige por **politicas** (stacking y filtros inyectables). Su objetivo es reutilizarse entre proyectos **sin tocar el núcleo**, adaptándose por configuración.

---
<a id="dictionary"></a>
## 2. Diccionario de conceptos

- **Slot:** celda del inventario. Puede estar vacía (sin `Stack`). Mantiene `Index` y `SlotProfileId`.
- **Stack:** pila homogénea de items con el mismo `DefinitionId`. Tiene `Count`.
- **ItemMeta:** metadatos de un item (TypeId, Tags, overrides de stack).
- **SlotProfile:** reglas de slot (AllowedTypes, RequiredTags, BannedTags, overrides de stack y/o stackeable).
- **IStackingPolicy:** política que determina el **máximo base por item** (p.ej., `sword=1`, `stone=50`).
- **ISlotFilterPolicy:** determina si un slot (perfil) **acepta** un item y el **máx efectivo** (min entre item y perfil).
- **InventoryModel:** implementación de `IInventory`. Lógica central: tryAdd/move/split/clear/capacity/perfiles.
- **IInventoryService / InventoryService:** fachada de uso para gameplay/UI. Incluye `TryUse` (efectos/fases).
- **IEffectRegistry:** resuelve efectos de uso para items (opcional).
- **IUsagePhasePolicy:** comprueba si un item puede usarse según la fase/turno (opcional).
- **UseResult:** resultado de `TryUse` (Success, ConsumeOne, Message).
- **Installer (`InventoryInstaller`):** componente de escena que compone providers, policies, modelo y servicio.
- **Samples:** escenas de ejemplo para validar integración (2D con perfiles / libres).

---
<a id="architecture"></a>
## 3. Arquitectura

El sistema se organiza en **tres capas**:

- **Domain (núcleo):**
    - Lógica pura del invenrario: slots, stacks, añadir/mover/dividir/limpiar, capacidad.
    - Políticas: `IStackingPolicy` (máx por item) y `ISlotFilterPolicy` (qué acepta un slot y max efectivo).
    - Tipos principales: `InventoryModel`, `Islot`, `IStack`, `ItemMeta`, `SlotProfile`.

- **Application (servicio):**
    - Fachada de alto nivel para gameplay/UI: `IInventoryService` / `InventoryService`.
    - Expone la API pública (add/move/split/clear/capacity/perfiles) y **TryUse** (opcional).
    - Integra **efectos** (`IEffectRegistry`) y **fases** (`IUsagePolicy`) si las usas.

- **Infrastructure (Unity):**
    - Bases de datos `ScriptableObjects`: `ItemDatabase`, `SlotProfileDatabase`.
    - Providers: `SoItemMetadataProvider`, `SoSlotProfileProvider`.
    - Instaladores de escena: `InventoryInstaller`.
    - (Opcional) `ScriptableEffectRegistry`.

**Principios:**
- Núcleo **libre de UI** y dependencias de Engine.
- **Inyección** de políticas para cambiar comportamiento sin tocar el core.
- **Configuración por datos** (SO) orientada a diseñadores y usuarios.

---
<a id="implementation"></a>
## 4. Cómo implementarlo
<a id="folders"></a>
### Estructura de carpetas sugerida.

> **Regla de oro**: no metas tus assets de juego (items, perfiles, escenas, iconos) dentro de `Assets/InventorySystem`.
> Esta capa es del framework. Tu contenido debería vivir fuera, para que puedas actualizar el sistema sin conflictos.

```bash
Assets/
├─ InventorySystem/                  ← (framework) NO toques aquí tu contenido
│  └─ … (core del sistema)
│
├─ Data/
│  └─ Inventory/
│     ├─ Items/
│     │  ├─ Definitions/            ← ItemDefinition*.asset (uno por ítem)
│     │  ├─ Icons/                  ← Sprites/Atlases para ítems
│     │  ├─ Payloads/               ← JSON/TextAssets por ítem (opc.)
│     │  └─ Types/                  ← (opc.) ScriptableObject de tipos si los usas
│     │
│     ├─ SlotProfiles/
│     │  └─ Definitions/            ← SlotProfileDefinition*.asset (uno por perfil)
│     │
│     └─ Databases/
│        ├─ ItemDatabase.asset
│        └─ SlotProfileDatabase.asset
│
├─ Game/
│  └─ Inventory/
│     ├─ Runtime/
│     │  ├─ Installers/             ← Prefab(s) con InventoryInstaller configurado
│     │  ├─ Effects/                ← IEffect concretos + (opc.) EffectRegistry.asset
│     ├─ UI/                        ← HUD, SlotView, icon db, etc. propios del juego
│
└─ Tests/                           ← (opc.) tests de tu juego que usan el sistema
```
<a id="conventions"></a>
### Convenciones y organización

1. `Assets/Data/Inventory/Items/Definitions/`
- **Qué**: un `ItemDefinition.asset` por cada ítem del juego.
- **Campos típicos del ItemDefinition (según tu sistema):**
    - `DefinitionId` → **ID única** (recomendado: kebab-case minúsculas, p. ej. `card-memory`, `wood`, `iron-sword`)
    - `Type` → "Card", "Object", "Resource", "Tool"... (si usas SO de tipo, referencia aquí)
    - `Tags[]` → "Consumable", "Material", "Tool", etc..
    - (Opcional) `EffectKey` → clave para el `EffectRegistry`
    - (Opcional) `PayloadJson` → `TextAsset` con JSON para el efecto
    - (Opcional) `hasStackOverride`/`MaxStack` → si este ítem fuerza su propio stack (ej. espada = 1)
- **Convenciones:**
    - **Nombre del asset** = `Item_<DefinitionId>.asset` (ej. `Item_wood.asset`)
    - `DefinitionId` **unico y estático**

2. `Assets/Data/Inventory/Items/Icons/`
- **Qué:** Sprites/atlas para la iconografía de ítems.
- **Convención recomendada:** `sprite.name == DefinitionId` (así tu ItemIconDatabase los resuelve directo).
- Si usas atlas: añade un `ItemIconDatabase.asset` que mapee `DefinitionId + Sprite`.

3. `Assets/Data/Inventory/Items/Payloads/`
- **Qué:** `TextAsset` JSON por ítem con parámetro de efectos (si los usas).
- **Nombre**: `<DefinitionId>.json` (ej. `card-memory.json`).
- **Ejemplo contenido:**
```json
{"amount": 10, "radius": 2}
```

4. `Assets/Data/Inventory/Items/Types` (opcional)
- **Qué:** ScriptableObjects para tipos (si no usas strings).
Por ejemplo: `ItemType_Resource.asset` con `TypeId = "Resource"`.

5. `Assets/Data/Inventory/SlotProfiles/Definitions/`
- **Qué:** un `SlotProfileDefinition.asset` por perfil de slot.
- **Campos típicos:**
    - `Id` → "Any", "Materials", "Consumables", "Weapons"... (PascalCase)
    - `AllowedTypes[]` (vacío = cualquiera)
    - `RequiredTags[]` (todas deben estar)
    - `BannedTags[]`
    - Overrides:
        - `HasStackableOverride` + `StackableOverride` (si `false` → no stackeable → max 1)
        - `MaxStackOverride` (>0 clampa el stack por slot)
    - **Convención:** `SlotProfile_<Id>.assset` (ej. `SlotProfile_Materials.asset`)

6. `Assets/Data/Inventory/Database/`
- **Qué:** las bases de datos que utiliza el `Installer`:
    - `ItemDatabase.asset` → lista de `ItemDefinition`.
    - `SlotProfileDatabase.asset` → lista de `SlotProfileDefinition`.

    Estas DBs genera índices en `OnValidate/OnEnable`.

7. `Assets/Game/Inventory/Runtime/Installers/`
- **Qué:** Prefab con `InventoryInstaller` **configurado:**
    - `ItemDatabase` → referencias al asset anterior
    - `SlotProfileDatabase`
    - `initialCapacity` → capacidad inicial del inventario
    - `defaultSlotProfileId` → profileId dado por defecto cuándo se construye el inventario. (ej. `"Any"` o `"Consumables"`)
    - (*Opc.*) `EffectRegistry` si usas `TryUse`
- **Cómo usar:** coloca ese prefab en todas tus escenas que tengan inventario.

8. `Assets/Game/Inventory/Runtime/Effects/`
- **Qué:** tus clases `IEffect` concretas (C#) y, si lo deseas, un `ScriptableEffectRegistry.asset`.
- **Convención:** `Effect_<Nombre>.cs` y `EffectRegistry.asset` con mapeo `EffectKey → IEffect`.

9. `Assets/Game/Inventory/UI/`
- **Qué:** tu UI real de HUD:
- **Nota:** esta carpeta es tuya; el sistema no te impone estructura.
<a id="instalation"></a>
### Instalación por escena (Installer)
1. Crea un **GameObject** vacío en tu escena (p.ej. `InventoryRoot`).
2. Añade el componente **InventoryInstaller** (o el tuyo propio)
3. Asigna:
    - `ItemDatabase` (asset)  
    - `SlotProfileDatabase` (asset)  
    - `initialCapacity` (p.ej. 3)  
    - `defaultSlotProfileId` (p.ej. `"Any"` o `"Consumables"`)  
    - (Opcional) `EffectRegistry` (SO) si usarás `TryUse`
4. En runtime, accende al servicio:
```csharp
var svc = FindObjectOfType<InventoryInstaller>().Service;
```

---
<a id="api-reference"></a>
## 5. API Reference (uso del inventario)

Interfaz principal: IInventoryService (Application)
Operaciones clave (resumen funcional):
```csharp
int Capacity { get; }
IReadOnlyList<GB.Inventory.Domain.Abstractions.ISlot> SlotsView { get; } 
// (en algunas versiones: vista de stacks o de slots; usa la que exponga tu build)

bool TryAdd(string definitionId, int count, out int slotIndex, out string reason);
// Merge-first, luego ubicación en primer slot vacío compatible.
// Devuelve true si entra TODO; false si entra parcial o no entra.

bool TryMove(int srcSlot, int dstSlot, out string reason);
// Mismo item → merge hasta el máximo efectivo.
// Distinto item → swap si ambos perfiles aceptan.

bool TrySplit(int slotIndex, int count, out int newSlotIndex, out string reason);
// Separa 'count' unidades a un primer slot vacío compatible. Revierta si no hay espacio.

bool TryClear(int slotIndex, out GB.Inventory.Domain.Stack removed); // o out string reason, según versión
// Vacía un slot (si tiene contenido). 

bool TrySetSlotProfile(int slotIndex, string slotProfileId, out string reason);
// Cambia el perfil del slot (afecta a operaciones futuras y max efectivo).

string GetSlotProfileId(int slotIndex);
// Devuelve el perfil actual del slot.

bool SetCapacity(int newCapacity, out string reason);
bool IncreaseCapacity(int delta, out string reason);
// Aumenta añade slots vacíos; reducir requiere vaciar los slots fuera del nuevo rango.

bool TryUse(int slotIndex, ITurnContext ctx, out UseResult result, out string reason);
// (Opcional) Resuelve y ejecuta el efecto del item, valida fase si procede,
// y consume 1 unidad si el efecto lo indica (result.ConsumeOne).
```
**Notas de uso:**
- **Slots y stack vacíos:**según tu build, un slot vacío puede ser:
    - `Islot` no-nulo con `Stack == null`, o
    - `null` en una vista de stacks.
    Usa checks seguros: `if(slot == null || slot.Stack == null) . . .
- **Perfiles y stacking efectivos:** `IslotFilterPolicy` combina:
    - Máximo base por item (`IStackingPolicy.GetMaxPerStack`)
    - Overrides de perfil (no stackeable o `MaxStackOverride`)
    - Resultado → **max efectivo** (clamp final)
- **Mensaje de error:** los métodos devuelven `reason` explicando rechazos o parciales.

---
<a id="connect-ui"></a>
## 6. Cómo conectarlo a una UI

El sistema **no impone** una UI. Flujo típico:
1. **HUD Presenter (Monobehaviour)**:
    - Expone referencias a un `Grid/Transform` y un prefab `SlotView`.
    - Método `Bind(IInventoryService svc)`: cachea el servicio y construye la rejilla.
    - Método `Refresh()`: recorre `svc.SlotsView` y:
        - Si vacío → `SlotView.RenderEmpty()`
        - Si con stack → `SlotView.Render(defId, count, icon)
2. **SlotView** (MonoBehaviour del prefab por slot):
    - Muestra `icon`, `count` (si > 1)
    - Implementa `IDragHandler/IBeginDrag/IEndDrag` para drag & drop (opcional).
    - En `OnDrop`, traduce a `TryMove(srcIndex, dstIndex, out reason)`.
3. **Iconos:**
    - `ItemIconDatabase` (SO simple) que mapea `DefinitionId → Sprite`.

**Ejemplo de binding mínimo:**
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
        // Instanciar N = _svc.Capacity
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

**Drag & Drop (resumen):**
- `BeginDrag`: crea un **ghost** (imagen) siguiendo el ratón.
- `OnDrop` en `SlotView` destino: llama a `_svc.TryMove(srcIndex, dstIndex, out reason)`.
- `EndDrag`: destruye el ghost y `Refresh()`.

---
<a id="effects"></a>
## 7. Cómo aplicar efectos

**Objetivo**: que `TryUse(slotIndex, ctx, out result, out reason)` ejecute lógica asociada a un item.
Se apoya en:
- **IEffectRegistry**: dado un `DefinitionId`, devuelve un `IEffect` y (opcional) su payload.
- **IEffect**: interfaz con `Apply(ITurnContext ctx, string definitionId, object payload) -> UseResult`.
- **IUsagePhasePolicy** (opcional): valida si puede usarse en la fase/turno actual (si tu juego tiene fases).
- **UseResult**: indica éxito/fracaso y si se debe consumir una unidad (`ConsumeOne`).

Flujo en `InventoryService.TryUse`:
1. Valida índice y que el slot no esté vacío.
2. (Opcional) Pregunta al policy de fases si se puede usar ahora.
3. Resuelve el efecto en el registry (`TryResolve(defId, oyt effect)`).
4. Obtiene `payload` (si existe) con `TryGetPayload(defId, out payload)`.
5. Ejecuta `effect.Apply(ctx, defId, payload)`.
6. Si `Success && ConsumeOne` → consume una unidad (clear o split + clear).

**Cómo configurar:**
- En **ItemDatabase:** rellena el `EffectKey` para el item que tenga efecto.
- En tu implementación de `IEffectRegistry`:
    - Mapea `EffectKey` → clase `IEffect`.
    - `TryGetPayload(defId, out payload)`: puede devolver el contenido de un JSON (string) o un objeto ya parseado.
- **Ejemplo** de `payload`: si `ItemDefinition.PayloadJson` tiene un TextAsset JSON:
```json
{"amount": 10, "radius": 2}
```
Tu efecto puede castear/parsing ese texto y usarlo.

**Ejemplo pseudo-código**:
```csharp
public sealed class TestEffect : IEffect
{
    public UseResult Apply(ITurnContext ctx, string definitionId, object payload)
    {
        // payload puede ser string JSON o un objeto ya construido
        // Lógica del efecto aquí...
        return UseResult.Ok(consumeOne: true, message: "Test effect applied");
    }
}
```

**Notas:**
- Si no quieres efectos → no asignes `EffectRegistry` en tu `InventoryInstaller` y no uses `TryUse`.
- Si quieres fases, implementa `IUsagePolicy.CanUse(defId, allowedPhases, ctx, out reason)`.