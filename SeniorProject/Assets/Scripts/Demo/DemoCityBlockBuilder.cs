using UnityEngine;

public class DemoCityBlockBuilder : MonoBehaviour
{
    public Transform parent;
    public Vector2 districtSize = new Vector2(10f, 10f);
    public int buildingCount = 18;
    public Vector2 buildingHeightRange = new Vector2(0.8f, 3f);
    public Material groundMaterial;
    public Material buildingMaterial;
    public Material hospitalMaterial;
    public bool generateOnStart;

    private const string GeneratedPrefix = "DemoGenerated_";

    private void Start()
    {
        if (generateOnStart)
        {
            Generate();
        }
    }

    [ContextMenu("Generate District")]
    public void Generate()
    {
        ClearGenerated();

        Transform targetParent = parent != null ? parent : transform;
        DemoRegionVisual regionVisual = GetComponent<DemoRegionVisual>();

        CreateGround(targetParent, regionVisual);

        int safeBuildingCount = Mathf.Max(0, buildingCount);
        for (int i = 0; i < safeBuildingCount; i++)
        {
            CreateBuilding(targetParent, regionVisual, i);
        }

        CreateHospital(targetParent, regionVisual);
    }

    [ContextMenu("Clear Generated")]
    public void ClearGenerated()
    {
        Transform targetParent = parent != null ? parent : transform;

        for (int i = targetParent.childCount - 1; i >= 0; i--)
        {
            Transform child = targetParent.GetChild(i);
            if (!child.name.StartsWith(GeneratedPrefix))
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }

        DemoRegionVisual regionVisual = GetComponent<DemoRegionVisual>();
        if (regionVisual != null && regionVisual.renderersToTint != null)
        {
            regionVisual.renderersToTint.RemoveAll(rendererItem => rendererItem == null);
        }
    }

    private void CreateGround(Transform targetParent, DemoRegionVisual regionVisual)
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = $"{GeneratedPrefix}Ground";
        ground.transform.SetParent(targetParent, false);
        ground.transform.localPosition = new Vector3(0f, -0.05f, 0f);
        ground.transform.localScale = new Vector3(districtSize.x, 0.1f, districtSize.y);
        ApplyMaterialAndRegister(ground, groundMaterial, regionVisual);
    }

    private void CreateBuilding(Transform targetParent, DemoRegionVisual regionVisual, int index)
    {
        float minHeight = Mathf.Min(buildingHeightRange.x, buildingHeightRange.y);
        float maxHeight = Mathf.Max(buildingHeightRange.x, buildingHeightRange.y);
        float height = Random.Range(minHeight, maxHeight);
        float width = Random.Range(0.5f, 1.2f);
        float depth = Random.Range(0.5f, 1.2f);
        float x = Random.Range(-districtSize.x * 0.45f, districtSize.x * 0.45f);
        float z = Random.Range(-districtSize.y * 0.45f, districtSize.y * 0.45f);

        GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
        building.name = $"{GeneratedPrefix}Building_{index + 1}";
        building.transform.SetParent(targetParent, false);
        building.transform.localPosition = new Vector3(x, height * 0.5f, z);
        building.transform.localScale = new Vector3(width, height, depth);
        ApplyMaterialAndRegister(building, buildingMaterial, regionVisual);
    }

    private void CreateHospital(Transform targetParent, DemoRegionVisual regionVisual)
    {
        GameObject hospital = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hospital.name = $"{GeneratedPrefix}Hospital";
        hospital.transform.SetParent(targetParent, false);
        hospital.transform.localPosition = new Vector3(districtSize.x * 0.3f, 1.25f, districtSize.y * 0.3f);
        hospital.transform.localScale = new Vector3(1.5f, 2.5f, 1.5f);
        ApplyMaterialAndRegister(hospital, hospitalMaterial != null ? hospitalMaterial : buildingMaterial, regionVisual);
    }

    private void ApplyMaterialAndRegister(GameObject targetObject, Material material, DemoRegionVisual regionVisual)
    {
        Renderer targetRenderer = targetObject.GetComponent<Renderer>();
        if (targetRenderer == null)
        {
            return;
        }

        if (material != null)
        {
            targetRenderer.sharedMaterial = material;
        }

        if (regionVisual == null)
        {
            return;
        }

        if (regionVisual.renderersToTint == null)
        {
            regionVisual.renderersToTint = new System.Collections.Generic.List<Renderer>();
        }

        if (!regionVisual.renderersToTint.Contains(targetRenderer))
        {
            regionVisual.renderersToTint.Add(targetRenderer);
        }
    }
}
