# Changelog

All notable changes to this project will be documented in this file. Format follows [Semantic Versioning](https://semver.org).

---

## [0.1.0-beta.1] - 2025-11-04
### Added
- Initial public beta release of the GB Inventory System.
- Core domain (`InventoryModel`, `Slot`, `Stack`).
- Application layer with `IInventoryService` facade.
- Infrastructure layer with `InventoryInstaller`, `SoProviders`, and ScriptableObject databases.
- Support for:
    - Per-item and per-profile stacking.
    - Slot filtering by allowed types/tags.
    - Dynamic capacity resizing.
    - Clear, Move, Split and Merge operations.
- Integrated effect registry hooks for item usage.
- Two sample scenes:
    -`2D_ProfilesDemo`
    -`2D_AnyInventory`
- Documentation (`README.md`) and full unit test suite.

### Known Issues
- Effect payloads currently support only text-based JSON.
- Runtime UI samples are basic prototypes and may require manual configuration.
- Minor inspector refresh bugs when changing profiles at runtime.

### Notes
> This is a **beta version** - expect breaking changes and refactors in upcoming updates.
> Feedback and bug reports are welcome via GitHub Issues.