using UnityEngine;

// Setup: add a cube or cylinder above each district, assign a transparent
// risk material, connect its Renderer here, then link this component from
// the matching DemoRegionVisual.
public class RiskVolumeVisual : MonoBehaviour
{
    [Header("Volume References")]
    public Renderer volumeRenderer;
    public Transform volumeTransform;

    [Header("Risk Volume Shape")]
    public float minHeight = 0.15f;
    public float maxHeight = 3.0f;
    public float baseAlpha = 0.22f;

    [Header("Pulse Animation")]
    public bool animatePulse = true;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.08f;

    private MaterialPropertyBlock propertyBlock;
    private Vector3 baseScale;
    private Color currentColor = Color.green;
    private float currentRiskLevel;
    private bool isVisible = true;

    private void Awake()
    {
        if (volumeRenderer == null)
        {
            volumeRenderer = GetComponent<Renderer>();
        }

        if (volumeTransform == null)
        {
            volumeTransform = transform;
        }

        propertyBlock = new MaterialPropertyBlock();
        baseScale = volumeTransform != null ? volumeTransform.localScale : Vector3.one;
        SetRisk(currentRiskLevel);
    }

    private void Update()
    {
        if (animatePulse && isVisible)
        {
            ApplyVisual();
        }
    }

    public void SetRisk(float riskLevel)
    {
        currentRiskLevel = Mathf.Clamp01(riskLevel);
        SetColor(GetRiskColor(currentRiskLevel));
    }

    public void SetColor(Color color)
    {
        currentColor = color;
        ApplyVisual();
    }

    public void SetVisible(bool visible)
    {
        isVisible = visible;

        if (volumeRenderer != null)
        {
            volumeRenderer.enabled = visible;
        }
    }

    private void ApplyVisual()
    {
        if (volumeTransform != null)
        {
            float height = Mathf.Lerp(minHeight, maxHeight, currentRiskLevel);
            float pulseScale = animatePulse ? 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount : 1f;
            volumeTransform.localScale = new Vector3(baseScale.x, height * pulseScale, baseScale.z);
        }

        if (volumeRenderer == null)
        {
            return;
        }

        float pulseAlpha = animatePulse ? Mathf.Sin(Time.time * pulseSpeed) * pulseAmount : 0f;
        Color transparentColor = currentColor;
        transparentColor.a = Mathf.Clamp01(baseAlpha + pulseAlpha);

        volumeRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", transparentColor);
        propertyBlock.SetColor("_BaseColor", transparentColor);
        volumeRenderer.SetPropertyBlock(propertyBlock);
    }

    private Color GetRiskColor(float riskLevel)
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
