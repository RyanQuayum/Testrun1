using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BuildManager : MonoBehaviour
/*
    Building Placement System.
*/
{
    [Header("Scene References")]
    public Camera mainCamera;
    public CityGrid grid;
    public CityResources resources;
    public Transform buildingsParent;

    [Header("Placement")]
    public BuildingDefinition selectedBuilding;
    public Material validPreviewMaterial;
    public Material invalidPreviewMaterial;

    private GameObject preview;
    private Vector2Int previewCell;
    private bool previewIsValid;

    public event Action<BuildingInstance> BuildingPlaced;
    public event Action<string> PlacementFailed;

    private bool pointerStartedOverUI;

    [Header("Footprint Highlight")]
    public bool showFootprintCells = true;
    public float footprintYOffset = 0.04f;
    public Material validFootprintMaterial;
    public Material invalidFootprintMaterial;

    private readonly List<GameObject> footprintMarkers = new List<GameObject>();

    [Header("Placement Modes")]
    public RoadPlacementController roadPlacementController;

    private void Update()
    {
        bool pressedThisFrame = WasPrimaryPressedThisFrame();
        bool releasedThisFrame = WasPrimaryReleasedThisFrame();

        if (pressedThisFrame)
        pointerStartedOverUI = IsPointerOverUI();

        if (pointerStartedOverUI)
        {
            HidePreview();
            if (releasedThisFrame)
                pointerStartedOverUI = false;

            return;
        }

        if (IsRoadSelected())
        {
            DestroyPreview();
            HideFootprintCells();

            bool hasPointerCell = TryGetPointerCell(out Vector2Int mouseCell);

            roadPlacementController.Tick(
                selectedBuilding,
                hasPointerCell ? mouseCell : (Vector2Int?)null,
                releasedThisFrame
            );

            return;
        }

        UpdatePreview();

        if (WasPrimaryReleasedThisFrame())
            TryPlaceSelected();
    }

    private bool IsRoadSelected()
    {
        return selectedBuilding != null &&
            selectedBuilding.category == BuildingCategory.Road;
    }

    public void SelectBuilding(BuildingDefinition definition)
    {
        selectedBuilding = definition;

        if (roadPlacementController != null)
        {
            roadPlacementController.Cancel();
        }
        if (IsRoadSelected())
        {
            DestroyPreview();
            HideFootprintCells();
            return;
        }
        
        RebuildPreview();
    
    }

    
    private bool WasPrimaryPressedThisFrame()
    {
        return (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame);
    }

    public void CancelPlacement()
    {
        selectedBuilding = null;
        DestroyPreview();
        HideFootprintCells();

        if (roadPlacementController != null)
            roadPlacementController.Cancel();
    }
    
    private void HidePreview()
    {
        if (preview != null)
            preview.SetActive(false);

        previewIsValid = false;

        HideFootprintCells();
    }

    public bool TryPlaceSelected()
    // Try initalise object and place on grid.
    {
        if (selectedBuilding == null || selectedBuilding.prefab == null || !previewIsValid)
            return false;

        if (resources != null && !resources.TrySpend(selectedBuilding.buildCost))
        {
            PlacementFailed?.Invoke("Not enough resources.");
            return false;
        }

        GameObject placedObject = Instantiate(
            selectedBuilding.prefab, 
            GetFootprintCenterWorld(previewCell, selectedBuilding.footprint), 
            Quaternion.identity, 
            buildingsParent
        );
        BuildingInstance instance = placedObject.GetComponent<BuildingInstance>();

        if (instance == null)
            instance = placedObject.AddComponent<BuildingInstance>();

        instance.Initialize(selectedBuilding, previewCell);
        grid.Occupy(instance);
        BuildingPlaced?.Invoke(instance);
        return true;
    }

    private void UpdatePreview()
    /*
        Checks if cell is valid, whether user can place and afford place
        then sets preview with transform position and correct material.
    */
    {
        if (selectedBuilding == null)
        {
            DestroyPreview();
            HideFootprintCells();
            return;
        }

        if (preview == null)
            RebuildPreview();

        if (preview == null)
            return;

        bool hasPointerCell = TryGetPointerCell(out Vector2Int mouseCell);
        if (hasPointerCell)
        {
            previewCell = GetCenteredOriginCell(
                mouseCell,
                selectedBuilding.footprint
            );
        }

        previewIsValid = 
        hasPointerCell && 
        grid.CanPlace(previewCell, selectedBuilding.footprint) && 
        (resources == null || resources.CanAfford(selectedBuilding.buildCost));

        preview.SetActive(hasPointerCell);

        if (!preview.activeSelf)
            {
                HideFootprintCells();
                return;
            }

        preview.transform.position = GetFootprintCenterWorld(
            previewCell,
            selectedBuilding.footprint
        ); // Now uses centred Footprint.

        ApplyPreviewMaterial(previewIsValid ? validPreviewMaterial : invalidPreviewMaterial);

        UpdateFootprintCells(
            previewCell,
            selectedBuilding.footprint,
            previewIsValid
        );
    }

    private bool TryGetPointerCell(out Vector2Int cell)
    // Get position of cell from grid relative to camera by casting a ray. 
    {
        cell = default;

        if (mainCamera == null || grid == null)
            return false;

        Vector2 screenPosition;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        else if (Mouse.current != null)
            screenPosition = Mouse.current.position.ReadValue();
        else
            return false;

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, grid.terrainMask))
            return grid.TryGetCellFromWorld(hit.point, out cell);

        return false;
    }

    private bool WasPrimaryReleasedThisFrame()
    {
        return (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame) ||
               (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame);
    }

    private void RebuildPreview()
    /*
        Creates preview (ghost) of selected building.
        Initialises preview with name and material.
    */
    {
        DestroyPreview();

        if (selectedBuilding == null || selectedBuilding.prefab == null)
            return;

        preview = Instantiate(selectedBuilding.prefab);
        preview.name = selectedBuilding.displayName + " Preview";
        ApplyPreviewMaterial(invalidPreviewMaterial);
        DisablePreviewBehaviours(preview);
    }

    private void DestroyPreview()
    {
        if (preview != null)
            Destroy(preview);
    }

    private void DisablePreviewBehaviours(GameObject root)
    {
        foreach (Collider collider in root.GetComponentsInChildren<Collider>())
            collider.enabled = false;

        foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>())
            behaviour.enabled = false;
    }

    private void ApplyPreviewMaterial(Material material)
    {
        if (material == null || preview == null)
            return;

        foreach (Renderer renderer in preview.GetComponentsInChildren<Renderer>())
            renderer.sharedMaterial = material;
    }

    private bool IsPointerOverUI()
{
    if (EventSystem.current == null)
        return false;

    if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        return EventSystem.current.IsPointerOverGameObject();

    if (Touchscreen.current != null)
    {
        foreach (var touch in Touchscreen.current.touches)
        {
            if (touch.press.isPressed)
                return EventSystem.current.IsPointerOverGameObject(touch.touchId.ReadValue());
        }
    }

    return false;
}

    private void UpdateFootprintCells(Vector2Int origin, Vector2Int footprint, bool isValid)
    {
        if (!showFootprintCells || grid == null)
        {
            HideFootprintCells();
            return;
        }

        int requiredMarkerCount = footprint.x * footprint.y;

        while (footprintMarkers.Count < requiredMarkerCount)
            footprintMarkers.Add(CreateFootprintMarker());

        Material material = isValid ? validFootprintMaterial : invalidFootprintMaterial;

        int markerIndex = 0;

        for (int x = 0; x < footprint.x; x++)
        {
            for (int y = 0; y < footprint.y; y++)
            {
                Vector2Int cell = origin + new Vector2Int(x, y); // Cells origin + footprint size

                GameObject marker = footprintMarkers[markerIndex];
                marker.SetActive(true);

                Vector3 worldPosition = grid.CellToWorld(cell);
                worldPosition.y += footprintYOffset; // Remember Y is up/down-axis in Unity. Markers will be raised up fromt the ground.

                marker.transform.position = worldPosition;
                marker.transform.localScale = new Vector3(
                    grid.cellSize * 0.95f,
                    0.02f,
                    grid.cellSize * 0.95f
                ); // Values are to make marker footprint slightly smaller than grid

                Renderer markerRenderer = marker.GetComponent<Renderer>();

                if (markerRenderer != null && material != null)
                    markerRenderer.sharedMaterial = material;

                markerIndex++;
            }
        }

        for (int i = markerIndex; i < footprintMarkers.Count; i++)
            footprintMarkers[i].SetActive(false);
    }

    private GameObject CreateFootprintMarker()
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = "Preview Footprint Cell";
        marker.transform.SetParent(transform, true);

        Collider markerCollider = marker.GetComponent<Collider>();

        if (markerCollider != null)
            Destroy(markerCollider);

        return marker;
    }

    private void HideFootprintCells()
    {
        for (int i = 0; i < footprintMarkers.Count; i++)
        {
            if (footprintMarkers[i] != null)
                footprintMarkers[i].SetActive(false);
        }
    }

    private Vector2Int GetCenteredOriginCell(Vector2Int mouseCell, Vector2Int footprint) // Calcs Centred origin from footprint and mouselocation.
    {
        return new Vector2Int(
            mouseCell.x - footprint.x / 2,
            mouseCell.y - footprint.y / 2
        );
    }

    private Vector3 GetFootprintCenterWorld(Vector2Int origin, Vector2Int footprint) // Returns centre of entire footprint.
    /*
        1 x 1: center is the origin cell
        2 x 2: center is between 4 cells
        3 x 3: center is the middle cell
        4 x 4: center is between 4 middle cells
    */
    {
        Vector3 originWorld = grid.CellToWorld(origin);

        Vector3 localOffset = new Vector3(
            (footprint.x - 1) * grid.cellSize * 0.5f,
            0f,
            (footprint.y - 1) * grid.cellSize * 0.5f
        );

        return originWorld + grid.transform.TransformVector(localOffset);
    }

}
