using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildManager : MonoBehaviour
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

    private void Update()
    {
        UpdatePreview();

        if (WasPrimaryReleasedThisFrame())
            TryPlaceSelected();
    }

    public void SelectBuilding(BuildingDefinition definition)
    {
        selectedBuilding = definition;
        RebuildPreview();
    }

    public void CancelPlacement()
    {
        selectedBuilding = null;
        DestroyPreview();
    }

    public bool TryPlaceSelected()
    {
        if (selectedBuilding == null || selectedBuilding.prefab == null || !previewIsValid)
            return false;

        if (resources != null && !resources.TrySpend(selectedBuilding.buildCost))
        {
            PlacementFailed?.Invoke("Not enough resources.");
            return false;
        }

        GameObject placedObject = Instantiate(selectedBuilding.prefab, grid.CellToWorld(previewCell), Quaternion.identity, buildingsParent);
        BuildingInstance instance = placedObject.GetComponent<BuildingInstance>();

        if (instance == null)
            instance = placedObject.AddComponent<BuildingInstance>();

        instance.Initialize(selectedBuilding, previewCell);
        grid.Occupy(instance);
        BuildingPlaced?.Invoke(instance);
        return true;
    }

    private void UpdatePreview()
    {
        if (selectedBuilding == null)
        {
            DestroyPreview();
            return;
        }

        if (preview == null)
            RebuildPreview();

        if (preview == null)
            return;

        bool hasPointerCell = TryGetPointerCell(out previewCell);
        previewIsValid = hasPointerCell && grid.CanPlace(previewCell, selectedBuilding.footprint) && (resources == null || resources.CanAfford(selectedBuilding.buildCost));
        preview.SetActive(hasPointerCell);

        if (!preview.activeSelf)
            return;

        preview.transform.position = grid.CellToWorld(previewCell);
        ApplyPreviewMaterial(previewIsValid ? validPreviewMaterial : invalidPreviewMaterial);
    }

    private bool TryGetPointerCell(out Vector2Int cell)
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
}
