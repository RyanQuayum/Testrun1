using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceTopBar : MonoBehaviour
{
    [Serializable]
    public struct ResourceEntry
    {
        public ResourceType type;
        public string displayName;
        public Sprite icon;
        public ResourceCounterView viewOverride;
    }

    [Header("Data")]
    [SerializeField] private CityResources resources;
    [SerializeField] private ResourceEntry[] trackedResources =
    {
        new ResourceEntry { type = ResourceType.Gold, displayName = "Gold" },
        new ResourceEntry { type = ResourceType.Wood, displayName = "Wood" },
        new ResourceEntry { type = ResourceType.Stone, displayName = "Stone" },
        new ResourceEntry { type = ResourceType.Food, displayName = "Food" }
    };

    [Header("Views")]
    [SerializeField] private Transform container;
    [SerializeField] private ResourceCounterView counterPrefab;
    [SerializeField] private bool buildViewsOnEnable = true;

    private readonly Dictionary<ResourceType, ResourceCounterView> viewsByType = new Dictionary<ResourceType, ResourceCounterView>();
    private readonly List<ResourceCounterView> generatedViews = new List<ResourceCounterView>();

    private void Awake()
    {
        if (resources == null)
            resources = FindAnyObjectByType<CityResources>();

        if (container == null)
            container = transform;
    }

    private void OnEnable()
    {
        if (buildViewsOnEnable)
            BuildViews();

        Subscribe();
        RefreshAll();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void SetResources(CityResources cityResources)
    {
        if (resources == cityResources)
            return;

        Unsubscribe();
        resources = cityResources;
        Subscribe();
        RefreshAll();
    }

    public void BuildViews()
    {
        ClearGeneratedViews();
        viewsByType.Clear();

        foreach (ResourceEntry entry in trackedResources)
        {
            ResourceCounterView view = ResolveView(entry);

            if (view == null)
                continue;

            view.Configure(entry.type, entry.icon, entry.displayName);
            viewsByType[entry.type] = view;
        }
    }

    public void RefreshAll()
    {
        if (resources == null)
            return;

        foreach (ResourceEntry entry in trackedResources)
            UpdateResource(entry.type, resources.Get(entry.type));
    }

    private ResourceCounterView ResolveView(ResourceEntry entry)
    {
        if (entry.viewOverride != null)
            return entry.viewOverride;

        if (counterPrefab == null)
            return null;

        ResourceCounterView view = Instantiate(counterPrefab, container);
        generatedViews.Add(view);
        return view;
    }

    private void ClearGeneratedViews()
    {
        foreach (ResourceCounterView view in generatedViews)
        {
            if (view == null)
                continue;

            if (Application.isPlaying)
                Destroy(view.gameObject);
            else
                DestroyImmediate(view.gameObject);
        }

        generatedViews.Clear();
    }

    private void Subscribe()
    {
        if (resources != null)
            resources.ResourceChanged += UpdateResource;
    }

    private void Unsubscribe()
    {
        if (resources != null)
            resources.ResourceChanged -= UpdateResource;
    }

    private void UpdateResource(ResourceType type, int amount)
    {
        if (viewsByType.TryGetValue(type, out ResourceCounterView view))
            view.SetAmount(amount);
    }
}
