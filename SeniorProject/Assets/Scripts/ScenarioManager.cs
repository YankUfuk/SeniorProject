using System.Collections.Generic;
using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    [SerializeField] private List<ScenarioData> availableScenarios = new List<ScenarioData>();
    [SerializeField] private int selectedScenarioIndex;

    public ScenarioData GetCurrentScenario()
    {
        if (availableScenarios == null || availableScenarios.Count == 0)
        {
            return null;
        }

        ClampSelectedIndex();
        return availableScenarios[selectedScenarioIndex];
    }

    public string GetCurrentScenarioName()
    {
        ScenarioData currentScenario = GetCurrentScenario();
        if (currentScenario == null)
        {
            return "No Scenario";
        }

        return string.IsNullOrWhiteSpace(currentScenario.scenarioName)
            ? "Unnamed Scenario"
            : currentScenario.scenarioName;
    }

    public void SelectScenario(int index)
    {
        if (availableScenarios == null || availableScenarios.Count == 0)
        {
            selectedScenarioIndex = 0;
            Debug.LogWarning("Scenario selection failed: no scenarios are available.");
            return;
        }

        selectedScenarioIndex = Mathf.Clamp(index, 0, availableScenarios.Count - 1);
        Debug.Log($"Selected scenario: {GetCurrentScenarioName()}");
    }

    public void SelectNextScenario()
    {
        if (!HasValidScenario())
        {
            Debug.LogWarning("Next scenario selection failed: no scenarios are available.");
            return;
        }

        SelectScenario(selectedScenarioIndex + 1);
    }

    public void SelectPreviousScenario()
    {
        if (!HasValidScenario())
        {
            Debug.LogWarning("Previous scenario selection failed: no scenarios are available.");
            return;
        }

        SelectScenario(selectedScenarioIndex - 1);
    }

    public bool HasValidScenario()
    {
        if (availableScenarios == null || availableScenarios.Count == 0)
        {
            return false;
        }

        ClampSelectedIndex();
        return availableScenarios[selectedScenarioIndex] != null;
    }

    private void ClampSelectedIndex()
    {
        if (availableScenarios == null || availableScenarios.Count == 0)
        {
            selectedScenarioIndex = 0;
            return;
        }

        selectedScenarioIndex = Mathf.Clamp(selectedScenarioIndex, 0, availableScenarios.Count - 1);
    }
}
