using UnityEngine;

public class BuildingInstance : MonoBehaviour
{
    public BuildingDefinition Definition { get; private set; }
    public Vector2Int Origin { get; private set; }
    public bool IsComplete { get; private set; }
    public float BuildProgress01 => Definition == null || Definition.buildSeconds <= 0f ? 1f : Mathf.Clamp01(buildTimer / Definition.buildSeconds);

    private float buildTimer;

    public void Initialize(BuildingDefinition definition, Vector2Int origin)
    {
        Definition = definition;
        Origin = origin;
        IsComplete = definition.buildSeconds <= 0f;
        buildTimer = IsComplete ? definition.buildSeconds : 0f;
        name = definition.displayName + " (" + origin.x + "," + origin.y + ")";
    }

    private void Update()
    {
        if (IsComplete || Definition == null)
            return;

        buildTimer += Time.deltaTime;

        if (buildTimer >= Definition.buildSeconds)
            IsComplete = true;
    }
}
