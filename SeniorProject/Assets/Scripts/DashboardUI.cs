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
    [SerializeField] private TextMeshProUGUI dataSourceText;
    [SerializeField] private TextMeshProUGUI dataLoadStatusText;
    [SerializeField] private TextMeshProUGUI scenarioText;
    [SerializeField] private TextMeshProUGUI lastActionText;
    [SerializeField] private TextMeshProUGUI hospitalWarningText;
    [SerializeField] private TextMeshProUGUI hospitalStatusText;
    [SerializeField] private TextMeshProUGUI selectedRegionDetailsText;
    [SerializeField] private TextMeshProUGUI peakInfectedText;
    [SerializeField] private TextMeshProUGUI overloadDaysText;
    [SerializeField] private TextMeshProUGUI finalRecoveredText;
    [SerializeField] private TextMeshProUGUI averageHospitalLoadText;
    [SerializeField] private TextMeshProUGUI overloadedRegionCountText;
    [SerializeField] private TextMeshProUGUI policyHistoryText;
    [SerializeField] private TextMeshProUGUI comparisonText;
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
        bool hasHospitalWarning = false;
        bool hasHospitalOverCapacity = false;
        List<string> overloadedRegions = new List<string>();

        for (int i = 0; i < manager.Regions.Count; i++)
        {
            RegionData region = manager.Regions[i];
            totalSusceptible += region.Susceptible;
            totalExposed += region.Exposed;
            totalInfected += region.Infected;
            totalRecovered += region.Recovered;
            totalPopulation += region.Population;

            if (region.HospitalLoadRatio >= 0.75f)
            {
                hasHospitalWarning = true;
            }

            if (region.HospitalLoadRatio > 1f)
            {
                hasHospitalOverCapacity = true;
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

        if (dataSourceText != null)
        {
            dataSourceText.text = $"Data Source: {manager.LoadedDataSourceLabel}";
        }

        if (dataLoadStatusText != null)
        {
            dataLoadStatusText.text = $"Data Load: {manager.LastDataLoadMessage}";
        }

        if (scenarioText != null)
        {
            scenarioText.text = $"Scenario/Data: {manager.LoadedDataSourceLabel}";
        }

        if (lastActionText != null)
        {
            lastActionText.text = $"Last Action: {manager.LastActionMessage}";
        }

        if (selectedRegionText != null)
        {
            selectedRegionText.text = $"Selected Region: {manager.GetSelectedRegionName()}";
        }

        if (selectedRegionDetailsText != null)
        {
            selectedRegionDetailsText.text = GetSelectedRegionDetailsText(manager);
        }

        if (hospitalWarningText != null)
        {
            hospitalWarningText.text = overloadedRegions.Count == 0
                ? "Hospital status: Stable"
                : $"Warning: Hospital capacity exceeded in {string.Join(", ", overloadedRegions)}";
        }

        if (hospitalStatusText != null)
        {
            if (hasHospitalOverCapacity)
            {
                hospitalStatusText.text = "Hospital Status: Over Capacity";
            }
            else if (hasHospitalWarning)
            {
                hospitalStatusText.text = "Hospital Status: Warning";
            }
            else
            {
                hospitalStatusText.text = "Hospital Status: Stable";
            }
        }

        if (peakInfectedText != null)
        {
            peakInfectedText.text = $"Peak Infected: {manager.GetPeakInfected():F0}";
        }

        if (overloadDaysText != null)
        {
            overloadDaysText.text = $"Hospital Overload Days: {manager.GetHospitalOverloadDays()}";
        }

        if (finalRecoveredText != null)
        {
            finalRecoveredText.text = $"Recovered So Far: {manager.GetFinalRecovered():F0}";
        }

        if (averageHospitalLoadText != null)
        {
            averageHospitalLoadText.text = $"Average Hospital Load: {manager.GetLatestAverageHospitalLoad():P0}";
        }

        if (overloadedRegionCountText != null)
        {
            overloadedRegionCountText.text = $"Overloaded Regions: {manager.GetLatestOverloadedRegionCount()}";
        }

        if (policyHistoryText != null)
        {
            policyHistoryText.text = manager.GetRecentPolicyHistoryText(3);
        }

        if (comparisonText != null)
        {
            comparisonText.text = manager.GetComparisonText();
        }

        if (simpleBarChartUI != null)
        {
            simpleBarChartUI.Refresh(totalSusceptible, totalExposed, totalInfected, totalRecovered);
        }
    }

    private string GetSelectedRegionDetailsText(SimulationManager manager)
    {
        int selectedIndex = manager.SelectedRegionIndex;
        if (selectedIndex < 0 || selectedIndex >= manager.Regions.Count)
        {
            return "Selected Region Details: None";
        }

        RegionData selectedRegion = manager.Regions[selectedIndex];
        string lockdownStatus = selectedRegion.IsLockdownActive ? "Active" : "Inactive";

        return
            $"Region Name: {selectedRegion.RegionName}\n" +
            $"Population: {selectedRegion.Population}\n" +
            $"Infected: {selectedRegion.Infected:F0}\n" +
            $"Hospital Capacity: {selectedRegion.HospitalCapacity}\n" +
            $"Hospital Load: {FormatHospitalLoad(selectedRegion)}\n" +
            $"Lockdown Status: {lockdownStatus}";
    }

    private string FormatHospitalLoad(RegionData region)
    {
        if (region.HospitalCapacity <= 0)
        {
            return region.Infected > 0f ? "Over Capacity" : "0%";
        }

        return region.HospitalLoadRatio.ToString("P0");
    }
}
