using System;
using System.Collections.Generic;
using UnityEngine;

public class CityResources : MonoBehaviour
{
    [SerializeField] private ResourceAmount[] startingResources =
    {
        new ResourceAmount(ResourceType.Gold, 250),
        new ResourceAmount(ResourceType.Wood, 100),
        new ResourceAmount(ResourceType.Stone, 50),
        new ResourceAmount(ResourceType.Food, 75)
    };

    private readonly Dictionary<ResourceType, int> amounts = new Dictionary<ResourceType, int>();

    public event Action<ResourceType, int> ResourceChanged;

    public int Get(ResourceType type)
    {
        return amounts.TryGetValue(type, out int amount) ? amount : 0;
    }

    public bool CanAfford(ResourceAmount[] cost)
    {
        if (cost == null)
            return true;

        foreach (ResourceAmount resource in cost)
        {
            if (Get(resource.type) < resource.amount)
                return false;
        }

        return true;
    }

    public bool TrySpend(ResourceAmount[] cost)
    {
        if (!CanAfford(cost))
            return false;

        Add(cost, -1);
        return true;
    }

    public void Add(ResourceAmount[] resources, int multiplier = 1)
    {
        if (resources == null)
            return;

        foreach (ResourceAmount resource in resources)
            Add(resource.type, resource.amount * multiplier);
    }

    public void Add(ResourceType type, int amount)
    {
        int newAmount = Mathf.Max(0, Get(type) + amount);
        amounts[type] = newAmount;
        ResourceChanged?.Invoke(type, newAmount);
    }

    private void Awake()
    {
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            amounts[type] = 0;

        Add(startingResources);
    }
}
