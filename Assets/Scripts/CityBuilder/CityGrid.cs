using UnityEngine;

public class CityGrid : MonoBehaviour
{
    [Header("Grid")]
    public Vector2Int size = new Vector2Int(64, 64);
    public float cellSize = 1f;
    public LayerMask terrainMask = ~0;

    private BuildingInstance[,] occupied;

    private void Awake()
    {
        occupied = new BuildingInstance[size.x, size.y];
    }

    public bool TryGetCellFromWorld(Vector3 worldPosition, out Vector2Int cell)
    {
        Vector3 local = transform.InverseTransformPoint(worldPosition);
        cell = new Vector2Int(Mathf.FloorToInt(local.x / cellSize), Mathf.FloorToInt(local.z / cellSize));
        return IsInBounds(cell);
    }

    public Vector3 CellToWorld(Vector2Int cell)
    {
        Vector3 local = new Vector3((cell.x + 0.5f) * cellSize, 0f, (cell.y + 0.5f) * cellSize);
        return transform.TransformPoint(local);
    }

    public bool CanPlace(Vector2Int origin, Vector2Int footprint)
    {
        for (int x = 0; x < footprint.x; x++)
        {
            for (int y = 0; y < footprint.y; y++)
            {
                Vector2Int cell = origin + new Vector2Int(x, y);

                if (!IsInBounds(cell) || occupied[cell.x, cell.y] != null)
                    return false;
            }
        }

        return true;
    }

    public void Occupy(BuildingInstance building)
    {
        SetCells(building, building.Origin, building.Definition.footprint);
    }

    public void Clear(BuildingInstance building)
    {
        SetCells(null, building.Origin, building.Definition.footprint);
    }

    private void SetCells(BuildingInstance building, Vector2Int origin, Vector2Int footprint)
    {
        for (int x = 0; x < footprint.x; x++)
        {
            for (int y = 0; y < footprint.y; y++)
            {
                Vector2Int cell = origin + new Vector2Int(x, y);

                if (IsInBounds(cell))
                    occupied[cell.x, cell.y] = building;
            }
        }
    }

    private bool IsInBounds(Vector2Int cell)
    {
        return cell.x >= 0 && cell.y >= 0 && cell.x < size.x && cell.y < size.y;
    }
}
