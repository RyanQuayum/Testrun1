# Medieval City Builder Framework

This folder contains a lightweight Unity gameplay framework for a mobile-friendly medieval city builder inspired by SimCity BuildIt.

## Core loop

1. Create `BuildingDefinition` assets from **Assets > Create > Medieval City Builder > Building Definition**.
2. Assign a prefab, footprint, build cost, production, upkeep, population, and category.
3. Add `CityGrid`, `CityResources`, `BuildManager`, `EconomyTicker`, and `CityBuilderBootstrap` to scene objects.
4. Assign the main camera, terrain layer mask, preview materials, and optional first building.
5. Call `BuildManager.SelectBuilding(definition)` from UI buttons to enter placement mode.

## Suggested medieval building set

- **Road:** Dirt Road, Stone Road
- **Housing:** Peasant Hut, Townhouse, Manor
- **Production:** Lumber Camp, Quarry, Farm, Blacksmith
- **Storage:** Granary, Warehouse
- **Service:** Well, Market, Chapel, Watchtower
- **Decoration:** Statue, Garden, Fountain
- **Wonder:** Castle Keep, Cathedral

## Scene setup notes

- The placement terrain needs a collider and should be included in `CityGrid.terrainMask`.
- Building prefabs can be simple cubes at first; add `BuildingInstance` manually or let `BuildManager` add it during placement.
- The framework is UI-agnostic so it can support both editor prototypes and mobile UI canvases.
