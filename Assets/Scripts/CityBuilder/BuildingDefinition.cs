using UnityEngine;

[CreateAssetMenu(menuName = "Medieval City Builder/Building Definition")]
public class BuildingDefinition : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;
    [TextArea]
    public string description;
    public BuildingCategory category;
    public Sprite icon;

    [Header("Placement")]
    public GameObject prefab;
    public Vector2Int footprint = Vector2Int.one;
    public bool requiresRoadAccess = true;
    public int unlockLevel = 1;

    [Header("Economy")]
    public ResourceAmount[] buildCost;
    public ResourceAmount[] storageProvided;
    public ResourceAmount[] productionPerTick;
    public ResourceAmount[] upkeepPerTick;
    public int populationCapacity;
    public int happinessImpact;
    public float buildSeconds = 3f;
}
