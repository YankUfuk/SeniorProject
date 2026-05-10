using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [Header("Simulation References")]
    [SerializeField] private List<RegionView> regionViews = new List<RegionView>();
    [SerializeField] private DashboardUI dashboardUI;
    [SerializeField] private SimpleBarChartUI simpleBarChartUI;
    [SerializeField] private ScenarioData scenarioData;

    [Header("SEIR Parameters")]
    [SerializeField] private float infectionRate = 0.25f;
    [SerializeField] private float exposedToInfectedRate = 0.20f;
    [SerializeField] private float recoveryRate = 0.10f;
    [SerializeField] private float secondsPerDay = 1f;
    [SerializeField] private int maxDays = 120;

    [Header("Budget and Costs")]
    [SerializeField] private float startingBudget = 1000f;
    [SerializeField] private float lockdownCost = 120f;
    [SerializeField] private float vaccinationCostPerPerson = 1f;
    [SerializeField] private float hospitalCapacityUnitCost = 2f;

    [Header("Button Defaults")]
    [SerializeField] private float vaccinationAmountPerClick = 100f;
    [SerializeField] private int hospitalIncreasePerClick = 50;
    [SerializeField] private int selectedRegionIndex = 0;

    private readonly List<RegionData> regions = new List<RegionData>();
    private bool isSimulationRunning;
    private int currentDay;
    private float budget;
    private float dayTimer;
    private string loadedScenarioLabel = "Fallback";
    private bool hasWarnedMissingDashboard;
    private bool hasWarnedMissingRegionViews;
    private bool hasWarnedShortRegionViews;

    public IReadOnlyList<RegionData> Regions => regions;
    public int CurrentDay => currentDay;
    public float Budget => budget;
    public int SelectedRegionIndex => selectedRegionIndex;
    public string SelectedRegionName => GetSelectedRegionName();

    private void Start()
    {
        InitializeScenarioOrFallback();
        currentDay = 0;
        selectedRegionIndex = Mathf.Clamp(selectedRegionIndex, 0, Mathf.Max(0, regions.Count - 1));
        Debug.Log($"Simulation startup scenario: {loadedScenarioLabel}");
        BindRegionViews();
        RefreshAllViews();
    }

    private void Update()
    {
        if (!isSimulationRunning)
        {
            return;
        }

        float dayLength = Mathf.Max(0.01f, secondsPerDay);
        dayTimer += Time.deltaTime;

        while (dayTimer >= dayLength && isSimulationRunning)
        {
            dayTimer -= dayLength;
            AdvanceOneDay();
        }
    }

    public void StartSimulation()
    {
        isSimulationRunning = true;
        BindRegionViews();
        RefreshAllViews();
    }

    public void PauseSimulation()
    {
        isSimulationRunning = false;
        dayTimer = 0f;
    }

    public void AdvanceOneDay()
    {
        if (maxDays > 0 && currentDay >= maxDays)
        {
            if (isSimulationRunning)
            {
                PauseSimulation();
                Debug.Log($"Simulation paused: reached max day limit ({maxDays}).");
            }
            return;
        }

        float effectiveInfectionRate = scenarioData != null ? scenarioData.infectionRate : infectionRate;
        float effectiveExposedToInfectedRate = scenarioData != null ? scenarioData.exposedToInfectedRate : exposedToInfectedRate;
        float effectiveRecoveryRate = scenarioData != null ? scenarioData.recoveryRate : recoveryRate;

        currentDay++;

        for (int i = 0; i < regions.Count; i++)
        {
            DiseaseModel.UpdateOneDay(
                regions[i],
                effectiveInfectionRate,
                effectiveExposedToInfectedRate,
                effectiveRecoveryRate);
        }

        LogDailyTotals();
        RefreshAllViews();

        if (maxDays > 0 && currentDay >= maxDays)
        {
            PauseSimulation();
            Debug.Log($"Simulation paused: reached max day limit ({maxDays}).");
        }
    }

    public void ApplyLockdownToRegion(int index)
    {
        if (!TryGetRegion(index, out RegionData region))
        {
            Debug.LogWarning($"Lockdown failed: invalid region index {index}.");
            return;
        }

        if (region.IsLockdownActive)
        {
            Debug.Log($"Lockdown skipped: {region.RegionName} is already in lockdown.");
            return;
        }

        if (budget < lockdownCost)
        {
            Debug.LogWarning($"Lockdown failed: not enough budget. Needed {lockdownCost:F0}, current {budget:F0}.");
            return;
        }

        budget -= lockdownCost;
        region.IsLockdownActive = true;
        Debug.Log($"Lockdown applied to {region.RegionName}. Cost: {lockdownCost:F0}. Budget left: {budget:F0}.");
        RefreshAllViews();
    }

    public void ApplyVaccinationToRegion(int index, float amount)
    {
        if (!TryGetRegion(index, out RegionData region))
        {
            Debug.LogWarning($"Vaccination failed: invalid region index {index}.");
            return;
        }

        if (amount <= 0f)
        {
            Debug.LogWarning($"Vaccination failed in {region.RegionName}: amount must be positive.");
            return;
        }

        float peopleToVaccinate = Mathf.Min(amount, region.Susceptible);
        float cost = peopleToVaccinate * vaccinationCostPerPerson;

        if (budget < cost)
        {
            Debug.LogWarning($"Vaccination failed in {region.RegionName}: needed {cost:F0}, current budget {budget:F0}.");
            return;
        }

        budget -= cost;
        region.Susceptible -= peopleToVaccinate;
        region.Recovered += peopleToVaccinate;
        Debug.Log($"Vaccinated {peopleToVaccinate:F0} people in {region.RegionName}. Cost: {cost:F0}. Budget left: {budget:F0}.");
        RefreshAllViews();
    }

    public void IncreaseHospitalCapacity(int index, int amount)
    {
        if (!TryGetRegion(index, out RegionData region))
        {
            Debug.LogWarning($"Hospital capacity increase failed: invalid region index {index}.");
            return;
        }

        if (amount <= 0)
        {
            Debug.LogWarning($"Hospital capacity increase failed in {region.RegionName}: amount must be positive.");
            return;
        }

        float cost = amount * hospitalCapacityUnitCost;
        if (budget < cost)
        {
            Debug.LogWarning($"Hospital increase failed in {region.RegionName}: needed {cost:F0}, current budget {budget:F0}.");
            return;
        }

        budget -= cost;
        region.HospitalCapacity += amount;
        Debug.Log($"Hospital capacity increased in {region.RegionName} by {amount}. Cost: {cost:F0}. Budget left: {budget:F0}.");
        RefreshAllViews();
    }

    public void OnStartButtonClicked()
    {
        StartSimulation();
    }

    public void OnPauseButtonClicked()
    {
        PauseSimulation();
    }

    public void OnNextDayButtonClicked()
    {
        AdvanceOneDay();
    }

    public void OnResetButtonClicked()
    {
        ResetSimulation();
    }

    public void OnLockdownButtonClicked(int regionIndex)
    {
        ApplyLockdownToRegion(regionIndex);
    }

    public void OnVaccinationButtonClicked(int regionIndex)
    {
        ApplyVaccinationToRegion(regionIndex, vaccinationAmountPerClick);
    }

    public void OnHospitalButtonClicked(int regionIndex)
    {
        IncreaseHospitalCapacity(regionIndex, hospitalIncreasePerClick);
    }

    public void OnLockdownSelectedRegionClicked()
    {
        ApplyLockdownToRegion(selectedRegionIndex);
    }

    public void OnVaccinationSelectedRegionClicked()
    {
        ApplyVaccinationToRegion(selectedRegionIndex, vaccinationAmountPerClick);
    }

    public void OnHospitalSelectedRegionClicked()
    {
        IncreaseHospitalCapacity(selectedRegionIndex, hospitalIncreasePerClick);
    }

    public void ResetSimulation()
    {
        PauseSimulation();
        InitializeScenarioOrFallback();
        currentDay = 0;
        selectedRegionIndex = regions.Count > 0 ? 0 : -1;

        for (int i = 0; i < regions.Count; i++)
        {
            regions[i].IsLockdownActive = false;
        }

        BindRegionViews();
        RefreshAllViews();
        Debug.Log("Simulation reset to initial state.");
    }

    public void SelectRegion(int index)
    {
        if (index < 0 || index >= regions.Count)
        {
            Debug.LogWarning($"Region selection failed: invalid index {index}.");
            return;
        }

        selectedRegionIndex = index;
        Debug.Log($"Selected region: {regions[selectedRegionIndex].RegionName}");
        RefreshAllViews();
    }

    public string GetSelectedRegionName()
    {
        if (selectedRegionIndex >= 0 && selectedRegionIndex < regions.Count)
        {
            return regions[selectedRegionIndex].RegionName;
        }

        return "None";
    }

    public void RefreshAllViews()
    {
        if (regionViews == null)
        {
            if (!hasWarnedMissingRegionViews)
            {
                Debug.LogWarning("SimulationManager: Region views list is not assigned.");
                hasWarnedMissingRegionViews = true;
            }
        }
        else
        {
            for (int i = 0; i < regionViews.Count && i < regions.Count; i++)
            {
                if (regionViews[i] != null)
                {
                    regionViews[i].SetSelected(i == selectedRegionIndex);
                    regionViews[i].RefreshView();
                }
            }

            if (regionViews.Count < regions.Count)
            {
                if (!hasWarnedShortRegionViews)
                {
                    Debug.LogWarning($"SimulationManager: Only {regionViews.Count} RegionViews assigned for {regions.Count} regions.");
                    hasWarnedShortRegionViews = true;
                }
            }
        }

        RefreshSimpleBarChart();

        if (dashboardUI != null)
        {
            dashboardUI.Refresh(this);
        }
        else if (!hasWarnedMissingDashboard)
        {
            Debug.LogWarning("SimulationManager: DashboardUI reference is missing.");
            hasWarnedMissingDashboard = true;
        }
    }

    private void RefreshSimpleBarChart()
    {
        if (simpleBarChartUI == null)
        {
            return;
        }

        float totalSusceptible = 0f;
        float totalExposed = 0f;
        float totalInfected = 0f;
        float totalRecovered = 0f;

        for (int i = 0; i < regions.Count; i++)
        {
            totalSusceptible += regions[i].Susceptible;
            totalExposed += regions[i].Exposed;
            totalInfected += regions[i].Infected;
            totalRecovered += regions[i].Recovered;
        }

        simpleBarChartUI.Refresh(totalSusceptible, totalExposed, totalInfected, totalRecovered);
    }

    private void BindRegionViews()
    {
        if (regionViews == null)
        {
            if (!hasWarnedMissingRegionViews)
            {
                Debug.LogWarning("SimulationManager: Region views list is not assigned.");
                hasWarnedMissingRegionViews = true;
            }
            return;
        }

        for (int i = 0; i < regionViews.Count && i < regions.Count; i++)
        {
            if (regionViews[i] != null)
            {
                regionViews[i].SetSimulationContext(this, i);
                regionViews[i].Bind(regions[i]);
            }
        }
    }

    private void LogDailyTotals()
    {
        float totalSusceptible = 0f;
        float totalExposed = 0f;
        float totalInfected = 0f;
        float totalRecovered = 0f;

        for (int i = 0; i < regions.Count; i++)
        {
            totalSusceptible += regions[i].Susceptible;
            totalExposed += regions[i].Exposed;
            totalInfected += regions[i].Infected;
            totalRecovered += regions[i].Recovered;
        }

        Debug.Log(
            $"Day {currentDay} | S: {totalSusceptible:F0}, E: {totalExposed:F0}, I: {totalInfected:F0}, R: {totalRecovered:F0}, Budget: {budget:F0}");
    }

    private bool TryGetRegion(int index, out RegionData region)
    {
        if (index < 0 || index >= regions.Count)
        {
            region = null;
            return false;
        }

        region = regions[index];
        return true;
    }

    private void CreateSampleRegions()
    {
        regions.Clear();
        regions.Add(CreateRegion("North District", 8000, 7760f, 120f, 80f, 40f, 220));
        regions.Add(CreateRegion("South District", 10500, 10140f, 160f, 140f, 60f, 300));
        regions.Add(CreateRegion("East District", 12000, 11510f, 220f, 180f, 90f, 360));
        regions.Add(CreateRegion("West District", 9500, 9150f, 170f, 130f, 50f, 260));
        regions.Add(CreateRegion("Central District", 14000, 13410f, 260f, 210f, 120f, 450));
    }

    // Scenario assets let us test different pandemic conditions without changing code.
    private void InitializeScenarioOrFallback()
    {
        if (scenarioData != null && scenarioData.regions != null && scenarioData.regions.Count > 0)
        {
            regions.Clear();

            for (int i = 0; i < scenarioData.regions.Count; i++)
            {
                RegionInitialData source = scenarioData.regions[i];
                regions.Add(CreateRegion(
                    source.regionName,
                    source.population,
                    source.susceptible,
                    source.exposed,
                    source.infected,
                    source.recovered,
                    source.hospitalCapacity));
            }

            budget = scenarioData.startingBudget;
            loadedScenarioLabel = string.IsNullOrWhiteSpace(scenarioData.scenarioName) ? "Unnamed Scenario" : scenarioData.scenarioName;
            Debug.Log($"Scenario loaded: {loadedScenarioLabel}");
            return;
        }

        CreateSampleRegions();
        budget = startingBudget;
        loadedScenarioLabel = "Fallback Sample (Medium Outbreak)";
        Debug.Log("ScenarioData missing or empty. Using built-in sample regions and default parameters.");
    }

    private RegionData CreateRegion(
        string regionName,
        int population,
        float susceptible,
        float exposed,
        float infected,
        float recovered,
        int hospitalCapacity)
    {
        return new RegionData
        {
            RegionName = regionName,
            Population = population,
            Susceptible = susceptible,
            Exposed = exposed,
            Infected = infected,
            Recovered = recovered,
            HospitalCapacity = hospitalCapacity,
            IsLockdownActive = false
        };
    }
}
