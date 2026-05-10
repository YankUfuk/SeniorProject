using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleBarChartUI : MonoBehaviour
{
    [SerializeField] private Image susceptibleBar;
    [SerializeField] private Image exposedBar;
    [SerializeField] private Image infectedBar;
    [SerializeField] private Image recoveredBar;
    [SerializeField] private TextMeshProUGUI susceptibleLabel;
    [SerializeField] private TextMeshProUGUI exposedLabel;
    [SerializeField] private TextMeshProUGUI infectedLabel;
    [SerializeField] private TextMeshProUGUI recoveredLabel;
    [SerializeField] private TextMeshProUGUI titleText;

    private float susceptibleBarMaxWidth;
    private float exposedBarMaxWidth;
    private float infectedBarMaxWidth;
    private float recoveredBarMaxWidth;

    private void Awake()
    {
        susceptibleBarMaxWidth = GetCurrentWidth(susceptibleBar);
        exposedBarMaxWidth = GetCurrentWidth(exposedBar);
        infectedBarMaxWidth = GetCurrentWidth(infectedBar);
        recoveredBarMaxWidth = GetCurrentWidth(recoveredBar);
    }

    public void Refresh(float susceptible, float exposed, float infected, float recovered)
    {
        float total = susceptible + exposed + infected + recovered;
        if (total <= 0f)
        {
            total = 1f; // prevent division by zero
        }

        float susceptibleRatio = Mathf.Clamp01(susceptible / total);
        float exposedRatio = Mathf.Clamp01(exposed / total);
        float infectedRatio = Mathf.Clamp01(infected / total);
        float recoveredRatio = Mathf.Clamp01(recovered / total);

        SetBar(susceptibleBar, susceptibleRatio, susceptibleBarMaxWidth);
        SetBar(exposedBar, exposedRatio, exposedBarMaxWidth);
        SetBar(infectedBar, infectedRatio, infectedBarMaxWidth);
        SetBar(recoveredBar, recoveredRatio, recoveredBarMaxWidth);

        SetLabel(susceptibleLabel, "Susceptible", susceptibleRatio);
        SetLabel(exposedLabel, "Exposed", exposedRatio);
        SetLabel(infectedLabel, "Infected", infectedRatio);
        SetLabel(recoveredLabel, "Recovered", recoveredRatio);

        if (titleText != null)
        {
            titleText.text = "Pandemic States";
        }
    }

    private void SetBar(Image bar, float ratio, float maxWidth)
    {
        if (bar == null)
        {
            return;
        }

        if (bar.type == Image.Type.Filled)
        {
            bar.fillAmount = ratio;
            return;
        }

        RectTransform rectTransform = bar.rectTransform;
        Vector2 size = rectTransform.sizeDelta;
        size.x = maxWidth * ratio;
        rectTransform.sizeDelta = size;
    }

    private void SetLabel(TextMeshProUGUI label, string stateName, float ratio)
    {
        if (label == null)
        {
            return;
        }

        label.text = $"{stateName}: {(ratio * 100f):F1}%";
    }

    private float GetCurrentWidth(Image bar)
    {
        if (bar == null)
        {
            return 0f;
        }

        return bar.rectTransform.sizeDelta.x;
    }
}
