using System.Collections.Generic;
using UnityEngine;

public class EconomyTicker : MonoBehaviour
{
    public CityResources resources;
    public float tickSeconds = 5f;

    private readonly List<BuildingInstance> buildings = new List<BuildingInstance>();
    private float timer;

    public void Register(BuildingInstance building)
    {
        if (building != null && !buildings.Contains(building))
            buildings.Add(building);
    }

    public void Unregister(BuildingInstance building)
    {
        buildings.Remove(building);
    }

    private void Awake()
    {
        BuildManager buildManager = FindObjectOfType<BuildManager>();

        if (buildManager != null)
            buildManager.BuildingPlaced += Register;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer < tickSeconds)
            return;

        timer -= tickSeconds;
        Tick();
    }

    private void Tick()
    {
        if (resources == null)
            return;

        foreach (BuildingInstance building in buildings)
        {
            if (building == null || !building.IsComplete || building.Definition == null)
                continue;

            if (resources.CanAfford(building.Definition.upkeepPerTick))
            {
                resources.TrySpend(building.Definition.upkeepPerTick);
                resources.Add(building.Definition.productionPerTick);
            }
        }
    }
}
