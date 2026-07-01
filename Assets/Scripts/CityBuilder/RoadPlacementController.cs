using System;
using System.Collections.Generic;
using UnityEngine;

public class RoadPlacementController : MonoBehaviour
{

    [Header("Scene References")]
    public CityGrid grid;
    public CityResources resources;
    public Transform buildingsParent;

    [Header("Preview")]
    public bool showPreview = true;
    public float previewYOffset = 0.04f;
    public Material validPreviewMaterial;
    public Material invalidPreviewMaterial;

    private readonly List<Vector2Int> previewCells = new List<Vector2Int>();
    private readonly List<GameObject> previewMarkers = new List<GameObject>();

    private bool hasStartCell;
    private Vector2Int startCell;
    private bool previewIsValid;

    public event Action<BuildingInstance> RoadPlaced;
    public event Action<string> PlacementFailed;

    public void Tick(BuildingDefinition roadDefinition, Vector2Int? pointerCell, bool clicked)
    {
        if (roadDefinition == null || roadDefinition.prefab == null || pointerCell == null)
        {
            HidePreview();
            return;
        }

        UpdatePreview(roadDefinition, pointerCell.Value);

        if (clicked)
            HandleClick(roadDefinition, pointerCell.Value);
    }

    public void Cancel()
    {
        hasStartCell = false;
        previewCells.Clear();
        HidePreview();
    }

    public void HidePreview()
    {
        for (int i = 0; i < previewMarkers.Count; i++)
        {
            if (previewMarkers[i] != null)
                previewMarkers[i].SetActive(false);
        }
    }

    private void PlaceRoadCell(BuildingDefinition roadDefinition, Vector2Int cell)
    {
        GameObject placedObject = Instantiate(
            roadDefinition.prefab,
            GetCellCenterWorld(cell),
            Quaternion.identity,
            buildingsParent
        );

        BuildingInstance instance = placedObject.GetComponent<BuildingInstance>();

        if (instance == null)
            instance = placedObject.AddComponent<BuildingInstance>();

        instance.Initialize(roadDefinition, cell);
        grid.Occupy(instance);
        RoadPlaced?.Invoke(instance);
    }

    private void HandleClick(BuildingDefinition roadDefinition, Vector2Int pointerCell)
    {
        if (!hasStartCell)
        {
            startCell = pointerCell;
            hasStartCell = true;
            return;
        } // If starting cell hasnt been defined yet (first click), init start cell.

        previewCells.Clear();
        BuildStraightRoadCells(startCell, pointerCell, previewCells);

        if (!CanPlaceRoadCells(previewCells))
        {
            PlacementFailed?.Invoke("Road path is blocked.");
            return;
        }

        if (!TrySpendRoadCost(roadDefinition, previewCells.Count))
        {
            PlacementFailed?.Invoke("Not enough resources.");
            return;
        }

        PlaceRoadCells(roadDefinition, previewCells);

        hasStartCell = false;
        previewCells.Clear();
        HidePreview(); // Clear Up
    }

        private void UpdatePreview(BuildingDefinition roadDefinition, Vector2Int pointerCell)
    {
        previewCells.Clear();

        if (hasStartCell)
            BuildStraightRoadCells(startCell, pointerCell, previewCells);
        else
            previewCells.Add(pointerCell);

        previewIsValid = CanPlaceRoadCells(previewCells) &&
                         CanAffordRoadCells(roadDefinition, previewCells.Count);

        UpdatePreviewMarkers();
    }

        private void BuildStraightRoadCells(Vector2Int start, Vector2Int end, List<Vector2Int> cells)
    {
        Vector2Int delta = end - start;
        bool horizontal = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y); // if x direction is greater than y direction, is HORIZONTAL, like me n ur gf.

        if (horizontal)
        {
            int step = end.x >= start.x ? 1 : -1;

            for (int x = start.x; x != end.x + step; x += step)
                cells.Add(new Vector2Int(x, start.y)); // add cells from start to end
        }
        else
        {
            int step = end.y >= start.y ? 1 : -1;

            for (int y = start.y; y != end.y + step; y += step)
                cells.Add(new Vector2Int(start.x, y));
        }
    }

        private void PlaceRoadCells(BuildingDefinition roadDefinition, List<Vector2Int> cells)
    {
        foreach (Vector2Int cell in cells)
            PlaceRoadCell(roadDefinition, cell);
    }

    private bool CanPlaceRoadCells(List<Vector2Int> cells)
    {
        if (grid == null)
            return false;

        foreach (Vector2Int cell in cells)
        {
            if (!grid.CanPlace(cell, Vector2Int.one))
                return false;
        }

        return true;
    }

    private bool CanAffordRoadCells(BuildingDefinition roadDefinition, int cellCount)
    {
        if (resources == null || roadDefinition == null || roadDefinition.buildCost == null)
            return true;

        foreach (ResourceAmount cost in roadDefinition.buildCost)
        {
            if (resources.Get(cost.type) < cost.amount * cellCount)
                return false;
        }

        return true;
    }

    private bool TrySpendRoadCost(BuildingDefinition roadDefinition, int cellCount)
    {
        if (resources == null || roadDefinition == null || roadDefinition.buildCost == null)
            return true;

        if (!CanAffordRoadCells(roadDefinition, cellCount))
            return false;

        resources.Add(roadDefinition.buildCost, -cellCount);
        return true;
    }


     private Vector3 GetCellCenterWorld(Vector2Int cell)
    {
        return grid.CellToWorld(cell);
    }

    private void UpdatePreviewMarkers()
    {
        if (!showPreview || grid == null)
        {
            HidePreview();
            return;
        }

        while (previewMarkers.Count < previewCells.Count)
            previewMarkers.Add(CreatePreviewMarker());

        Material material = previewIsValid ? validPreviewMaterial : invalidPreviewMaterial;

        for (int i = 0; i < previewCells.Count; i++)
        {
            GameObject marker = previewMarkers[i];
            marker.SetActive(true);

            Vector3 position = grid.CellToWorld(previewCells[i]);
            position.y += previewYOffset;

            marker.transform.position = position;
            marker.transform.localScale = new Vector3(
                grid.cellSize * 0.95f,
                0.02f,
                grid.cellSize * 0.95f
            );

            Renderer markerRenderer = marker.GetComponent<Renderer>();

            if (markerRenderer != null && material != null)
                markerRenderer.sharedMaterial = material;
        }

        for (int i = previewCells.Count; i < previewMarkers.Count; i++)
            previewMarkers[i].SetActive(false);
    }

    private GameObject CreatePreviewMarker()
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = "Road Preview Cell";
        marker.transform.SetParent(transform, true);

        Collider markerCollider = marker.GetComponent<Collider>();

        if (markerCollider != null)
            Destroy(markerCollider);

        return marker;
    }
}
