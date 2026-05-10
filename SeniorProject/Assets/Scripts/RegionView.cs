using TMPro;
using UnityEngine;

public class RegionView : MonoBehaviour
{
    [SerializeField] private Renderer regionRenderer;
    [SerializeField] private TextMeshPro regionText;
    [SerializeField] private int regionIndex;
    [SerializeField] private SimulationManager simulationManager;

    private RegionData boundData;
    private bool isSelected;
    private Vector3 defaultScale;

    private void Awake()
    {
        defaultScale = transform.localScale;
    }

    public void SetSimulationContext(SimulationManager manager, int index)
    {
        simulationManager = manager;
        regionIndex = index;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }

    public void Bind(RegionData data)
    {
        boundData = data;
        RefreshView();
    }

    public void RefreshView()
    {
        if (boundData == null)
        {
            return;
        }

        if (regionRenderer != null && regionRenderer.material != null)
        {
            float infectionRatio = boundData.InfectionRatio;
            Color targetColor;

            if (infectionRatio < 0.05f)
            {
                targetColor = Color.green;
            }
            else if (infectionRatio < 0.15f)
            {
                targetColor = new Color(1f, 0.6f, 0f); // orange/yellow
            }
            else
            {
                targetColor = Color.red;
            }

            if (isSelected)
            {
                targetColor = Color.Lerp(targetColor, Color.white, 0.25f);
            }

            regionRenderer.material.color = targetColor;
        }

        transform.localScale = isSelected ? defaultScale * 1.08f : defaultScale;

        if (regionText != null)
        {
            regionText.text =
                $"{boundData.RegionName}\nInfected: {boundData.Infected:F0}\nLockdown: {(boundData.IsLockdownActive ? "ON" : "OFF")}";
        }
    }

    private void OnMouseDown()
    {
        if (simulationManager != null)
        {
            simulationManager.SelectRegion(regionIndex);
        }
    }
}
