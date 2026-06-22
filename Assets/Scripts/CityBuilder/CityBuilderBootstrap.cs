using UnityEngine;

public class CityBuilderBootstrap : MonoBehaviour
{
    [Header("Required")]
    public Camera mainCamera;
    public CityGrid grid;
    public CityResources resources;
    public BuildManager buildManager;
    public EconomyTicker economyTicker;

    [Header("Optional Starter Selection")]
    public BuildingDefinition firstBuildingToPlace;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (grid == null)
            grid = FindAnyObjectByType<CityGrid>();

        if (resources == null)
            resources = FindAnyObjectByType<CityResources>();

        if (buildManager == null)
            buildManager = FindAnyObjectByType<BuildManager>();

        if (economyTicker == null)
            economyTicker = FindAnyObjectByType<EconomyTicker>();

        WireBuildManager();
        WireEconomy();
    }

    private void Start()
    {
        if (firstBuildingToPlace != null && buildManager != null)
            buildManager.SelectBuilding(firstBuildingToPlace);
    }

    private void WireBuildManager()
    {
        if (buildManager == null)
            return;

        buildManager.mainCamera = mainCamera;
        buildManager.grid = grid;
        buildManager.resources = resources;
    }

    private void WireEconomy()
    {
        if (economyTicker == null)
            return;

        economyTicker.resources = resources;
    }
}
