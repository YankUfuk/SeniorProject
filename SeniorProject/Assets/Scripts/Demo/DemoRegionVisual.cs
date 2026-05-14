using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DemoRegionVisual : MonoBehaviour
{
    public string regionName = "District";
    public List<Renderer> renderersToTint = new List<Renderer>();
    public TextMeshPro regionLabel;
    public RiskVolumeVisual riskVolumeVisual;

    [Range(0f, 1f)]
    public float riskLevel;

    public bool isSelected;

    private Vector3 defaultScale;
    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        defaultScale = transform.localScale;
        propertyBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        RefreshVisual();
    }

    public void SetRiskLevel(float value)
    {
        riskLevel = Mathf.Clamp01(value);
        RefreshVisual();
    }

    public void AddRisk(float amount)
    {
        SetRiskLevel(riskLevel + amount);
    }

    public void ReduceRisk(float amount)
    {
        SetRiskLevel(riskLevel - amount);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        RefreshVisual();
    }

    public void RefreshVisual()
    {
        riskLevel = Mathf.Clamp01(riskLevel);
        Color riskColor = GetRiskColor();

        if (renderersToTint != null)
        {
            for (int i = 0; i < renderersToTint.Count; i++)
            {
                Renderer targetRenderer = renderersToTint[i];
                if (targetRenderer == null)
                {
                    continue;
                }

                targetRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_BaseColor", riskColor);
                propertyBlock.SetColor("_Color", riskColor);
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        transform.localScale = isSelected ? defaultScale * 1.08f : defaultScale;

        if (regionLabel != null)
        {
            regionLabel.text =
                $"{regionName.ToUpper()}\nRISK {riskLevel:P0}\n{GetRiskLabel()}";
        }

        if (riskVolumeVisual != null)
        {
            riskVolumeVisual.SetRisk(riskLevel);
        }
    }

    public string GetRiskLabel()
    {
        if (riskLevel < 0.25f)
        {
            return "LOW";
        }

        if (riskLevel < 0.5f)
        {
            return "MODERATE";
        }

        if (riskLevel < 0.75f)
        {
            return "HIGH";
        }

        return "CRITICAL";
    }

    private Color GetRiskColor()
    {
        if (riskLevel < 0.25f)
        {
            return Color.green;
        }

        if (riskLevel < 0.5f)
        {
            return Color.yellow;
        }

        if (riskLevel < 0.75f)
        {
            return new Color(1f, 0.5f, 0f);
        }

        return Color.red;
    }
}
