using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class DashboardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentDayText;
    [SerializeField] private TextMeshProUGUI selectedRegionText;
    [SerializeField] private TextMeshProUGUI susceptibleText;
    [SerializeField] private TextMeshProUGUI exposedText;
    [SerializeField] private TextMeshProUGUI infectedText;
    [SerializeField] private TextMeshProUGUI recoveredText;
    [SerializeField] private TextMeshProUGUI budgetText;
    [SerializeField] private TextMeshProUGUI overallInfectionText;
    [SerializeField] private TextMeshProUGUI hospitalWarningText;
    [SerializeField] private SimpleBarChartUI simpleBarChartUI;

    public void Refresh(SimulationManager manager)
    {
        if (manager == null)
        {
            return;
        }

        float totalSusceptible = 0f;
        float totalExposed = 0f;
        float totalInfected = 0f;
        float totalRecovered = 0f;
        int totalPopulation = 0;
        List<string> overloadedRegions = new List<string>();

        for (int i = 0; i < manager.Regions.Count; i++)
        {
            RegionData region = manager.Regions[i];
            totalSusceptible += region.Susceptible;
            totalExposed += region.Exposed;
            totalInfected += region.Infected;
            totalRecovered += region.Recovered;
            totalPopulation += region.Population;

            if (region.HospitalLoadRatio > 1f)
            {
                overloadedRegions.Add(region.RegionName);
            }
        }

        if (currentDayText != null)
        {
            currentDayText.text = $"Day: {manager.CurrentDay}";
        }

        if (susceptibleText != null)
        {
            susceptibleText.text = $"Susceptible: {totalSusceptible:F0}";
        }

        if (exposedText != null)
        {
            exposedText.text = $"Exposed: {totalExposed:F0}";
        }

        if (infectedText != null)
        {
            infectedText.text = $"Infected: {totalInfected:F0}";
        }

        if (recoveredText != null)
        {
            recoveredText.text = $"Recovered: {totalRecovered:F0}";
        }

        if (budgetText != null)
        {
            budgetText.text = $"Budget: {manager.Budget:F0}";
        }

        float overallInfectionPercent = totalPopulation > 0
            ? (totalInfected / totalPopulation) * 100f
            : 0f;

        if (overallInfectionText != null)
        {
            overallInfectionText.text = $"Overall Infection: {overallInfectionPercent:F1}%";
        }

        if (selectedRegionText != null)
        {
            selectedRegionText.text = $"Selected Region: {manager.GetSelectedRegionName()}";
        }

        if (hospitalWarningText != null)
        {
            hospitalWarningText.text = overloadedRegions.Count == 0
                ? "Hospital status: Stable"
                : $"Warning: Hospital capacity exceeded in {string.Join(", ", overloadedRegions)}";
        }

        if (simpleBarChartUI != null)
        {
            simpleBarChartUI.Refresh(totalSusceptible, totalExposed, totalInfected, totalRecovered);
        }
    }
}
