using TMPro;
using UnityEngine;

public class DemoPanelToggle : MonoBehaviour
{
    public GameObject panelToToggle;
    public TextMeshProUGUI toggleButtonText;
    public string showText = "Show Panel";
    public string hideText = "Hide Panel";
    public bool panelStartsVisible = true;

    private void Start()
    {
        if (panelToToggle == null)
        {
            Debug.LogWarning("DemoPanelToggle is missing a panel reference.");
            RefreshButtonText();
            return;
        }

        panelToToggle.SetActive(panelStartsVisible);
        RefreshButtonText();
    }

    public void TogglePanel()
    {
        if (panelToToggle == null)
        {
            Debug.LogWarning("DemoPanelToggle cannot toggle because panelToToggle is missing.");
            return;
        }

        panelToToggle.SetActive(!panelToToggle.activeSelf);
        RefreshButtonText();
    }

    public void ShowPanel()
    {
        if (panelToToggle == null)
        {
            Debug.LogWarning("DemoPanelToggle cannot show panel because panelToToggle is missing.");
            return;
        }

        panelToToggle.SetActive(true);
        RefreshButtonText();
    }

    public void HidePanel()
    {
        if (panelToToggle == null)
        {
            Debug.LogWarning("DemoPanelToggle cannot hide panel because panelToToggle is missing.");
            return;
        }

        panelToToggle.SetActive(false);
        RefreshButtonText();
    }

    public void RefreshButtonText()
    {
        if (toggleButtonText == null)
        {
            return;
        }

        bool isPanelVisible = panelToToggle != null && panelToToggle.activeSelf;
        toggleButtonText.text = isPanelVisible ? hideText : showText;
    }
}
