using UnityEngine;

[RequireComponent(typeof(CityGrid))]
public class CityGridVisualizer : MonoBehaviour
{
    [Header("Runtime Grid")]
    public bool showGrid = false;
    public float yOffset = 0.02f;
    public Color lineColor = new Color(1f, 1f, 1f, 0.35f);
    public Material lineMaterial;

    private const string GridLinesName = "Runtime Grid Lines";

    private CityGrid grid;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh gridMesh;
    private Material generatedMaterial;

    private void Awake()
    {
        grid = GetComponent<CityGrid>();
        EnsureGridRenderer();
        RebuildGrid();
        Debug.Log($"showGrid = {showGrid}");
    }

    // private void OnEnable()
    // {
    //     SetGridVisible(showGrid);
    // }

    public void disableGrid()
    {
        SetGridVisible(false);
    }

    public void enableGrid()
    {
        SetGridVisible(true);
    }

    private void OnValidate()
    {
        grid = GetComponent<CityGrid>();

        if (!Application.isPlaying)
            return;

        EnsureGridRenderer();
        RebuildGrid();
    }

    private void OnDrawGizmos()
    {
        CityGrid editorGrid = GetComponent<CityGrid>();

        if (editorGrid == null)
        return;

        // if (!showGrid)
        // return;

        Gizmos.color = lineColor;

        float width = editorGrid.size.x * editorGrid.cellSize;
        float depth = editorGrid.size.y * editorGrid.cellSize;


        for (int x = 0; x <= editorGrid.size.x; x++)
        {
            float xPosition = x * editorGrid.cellSize;

            Vector3 start = new Vector3(xPosition, yOffset, 0f);
            Vector3 end = new Vector3(xPosition, yOffset, depth);

            Gizmos.DrawLine(
                editorGrid.transform.TransformPoint(start),
                editorGrid.transform.TransformPoint(end)
            );
        }

    for (int y = 0; y <= editorGrid.size.y; y++)
        {
            float zPosition = y * editorGrid.cellSize;

            Vector3 start = new Vector3(0f, yOffset, zPosition);
            Vector3 end = new Vector3(width, yOffset, zPosition);

            Gizmos.DrawLine(
                editorGrid.transform.TransformPoint(start),
                editorGrid.transform.TransformPoint(end)
            );
        }
    }

    public void RebuildGrid()
    {
        if (grid == null)
            return;

        EnsureGridRenderer();

        int verticalLines = grid.size.x + 1;
        int horizontalLines = grid.size.y + 1;
        int lineCount = verticalLines + horizontalLines;

        Vector3[] vertices = new Vector3[lineCount * 2];
        int[] indices = new int[vertices.Length];

        int vertexIndex = 0;

        float width = grid.size.x * grid.cellSize;
        float depth = grid.size.y * grid.cellSize;

        for (int x = 0; x <= grid.size.x; x++)
        {
            float xPosition = x * grid.cellSize;

            vertices[vertexIndex] = new Vector3(xPosition, yOffset, 0f);
            indices[vertexIndex] = vertexIndex;
            vertexIndex++;

            vertices[vertexIndex] = new Vector3(xPosition, yOffset, depth);
            indices[vertexIndex] = vertexIndex;
            vertexIndex++;
        }

        for (int y = 0; y <= grid.size.y; y++)
        {
            float zPosition = y * grid.cellSize;

            vertices[vertexIndex] = new Vector3(0f, yOffset, zPosition);
            indices[vertexIndex] = vertexIndex;
            vertexIndex++;

            vertices[vertexIndex] = new Vector3(width, yOffset, zPosition);
            indices[vertexIndex] = vertexIndex;
            vertexIndex++;
        }

        if (gridMesh == null)
        {
            gridMesh = new Mesh();
            gridMesh.name = "City Grid Lines";
        }

        gridMesh.Clear();
        gridMesh.vertices = vertices;
        gridMesh.SetIndices(indices, MeshTopology.Lines, 0);
        gridMesh.RecalculateBounds();

        meshFilter.sharedMesh = gridMesh;

        ApplyMaterial();
        SetGridVisible(showGrid);
    }

    private void EnsureGridRenderer()
    {
        Transform existing = transform.Find(GridLinesName);

        GameObject gridLines = existing != null
            ? existing.gameObject
            : new GameObject(GridLinesName);

        gridLines.transform.SetParent(transform, false);
        gridLines.transform.localPosition = Vector3.zero;
        gridLines.transform.localRotation = Quaternion.identity;
        gridLines.transform.localScale = Vector3.one;

        meshFilter = gridLines.GetComponent<MeshFilter>();

        if (meshFilter == null)
            meshFilter = gridLines.AddComponent<MeshFilter>();

        meshRenderer = gridLines.GetComponent<MeshRenderer>();

        if (meshRenderer == null)
            meshRenderer = gridLines.AddComponent<MeshRenderer>();
    }

    private void ApplyMaterial()
    {
        if (lineMaterial != null)
        {
            meshRenderer.sharedMaterial = lineMaterial;
            return;
        }

        if (generatedMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

            if (shader == null)
                shader = Shader.Find("Sprites/Default");

            if (shader == null)
                shader = Shader.Find("Hidden/Internal-Colored");

            generatedMaterial = new Material(shader);
            generatedMaterial.name = "Generated City Grid Material";
        }

        generatedMaterial.color = lineColor;
        meshRenderer.sharedMaterial = generatedMaterial;
    }

    private void SetGridVisible(bool visible)
    {
        if (meshRenderer != null)
            meshRenderer.enabled = visible;
    }

    private void OnDestroy()
    {
        if (gridMesh != null)
            Destroy(gridMesh);

        if (generatedMaterial != null)
            Destroy(generatedMaterial);
    }
}