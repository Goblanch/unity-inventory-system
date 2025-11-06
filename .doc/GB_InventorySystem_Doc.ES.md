# GB Inventory System - Documentación Técnica

> **Estado**: beta pública
> **Resumen**: sistema de inventario modular, desacoplado y configurable para Unity.
> **Capas**: Domain (núcleo), Application (servicio), Infrastructure (SO/Installers).
> **UI**: No acoplada (el sisteme expone vistas de slot/stacks para que puedas construir tu propia UI).

---

## Índice

1. [Introducción](#1-introducción)
2. [Diccionario de conceptos](#3-diccionario-de-conceptos)
3. [Arquitectura](#2-arquitectura)
4. [Cómo implementarlo](#4-cómo-implementarlo)
    - [Estructura de carpetas sugeridas](#estructura-de-carpetas-sugerida-assets)
    - [ScriptableObjects a crear](#scriptableobjects-a-crear)
    - [Instalación por escena (Installer)](#instalación-por-escena-installer)
5. [API Reference (uso del inventario)](#5-api-reference-uso-del-inventario)
6. [Cómo conectarlo a una UI](#6-cómo-conectarlo-a-una-ui)
7. [Cómo aplicar effectos](#7-cómo-aplicar-efectos)

---

## 1. Introducción

**GB Inventory System** es un framework de inventario para Unity **desacoplado de la UI**, **polivalente** y **escalable**. Se configura con **ScriptableObjects** (items, perfiles de slot) y se rige por **politicas** (stacking y filtros inyectables). Su objetivo es reutilizarse entre proyectos **sin tocar el núcleo**, adaptándose por configuración.

---

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

## 4. Cómo implementarlo

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