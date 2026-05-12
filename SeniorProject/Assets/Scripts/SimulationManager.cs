using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [Header("Simulation References")]
    [SerializeField] private List<RegionView> regionViews = new List<RegionView>();
    [SerializeField] private DashboardUI dashboardUI;
    [SerializeField] private SimpleBarChartUI simpleBarChartUI;
    [SerializeField] private TextAsset localDatasetJson;
    [SerializeField] private OnlineDatasetLoader onlineDatasetLoader;
    [SerializeField] private ScenarioManager scenarioManager;
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
    private readonly MetricsCollector metricsCollector = new MetricsCollector();
    private readonly List<PolicyEvent> policyHistory = new List<PolicyEvent>();
    private bool isSimulationRunning;
    private int currentDay;
    private float budget;
    private float dayTimer;
    private string loadedDataSourceLabel = "Fallback Sample (Medium Outbreak)";
    private string lastDataLoadMessage = "No data loaded yet.";
    private string lastActionMessage = "Ready.";
    private SimulationRunSummary baselineSummary;
    private ScenarioData activeScenarioData;
    private bool preferLoadedOnlineDataset;
    private bool hasWarnedMissingDashboard;
    private bool hasWarnedMissingRegionViews;
    private bool hasWarnedShortRegionViews;

    public IReadOnlyList<RegionData> Regions => regions;
    public int CurrentDay => currentDay;
    public float Budget => budget;
    public int SelectedRegionIndex => selectedRegionIndex;
    public string SelectedRegionName => GetSelectedRegionName();
    public string LoadedDataSourceLabel => loadedDataSourceLabel;
    public string LastDataLoadMessage => lastDataLoadMessage;
    public string LastActionMessage => lastActionMessage;
    public IReadOnlyList<PolicyEvent> PolicyHistory => policyHistory;
    public bool HasBaselineSummary => baselineSummary != null;

    private void Start()
    {
        InitializeDataSourceOrFallback();
        currentDay = 0;
        selectedRegionIndex = regions.Count > 0
            ? Mathf.Clamp(selectedRegionIndex, 0, regions.Count - 1)
            : -1;
        lastActionMessage = $"Loaded {loadedDataSourceLabel}.";
        Debug.Log($"Simulation startup data source: {loadedDataSourceLabel}");
        BindRegionViews();
        RefreshAllViews();

        // Online loading is optional; local or built-in data keeps the final demo reliable.
        if (onlineDatasetLoader != null && onlineDatasetLoader.LoadOnStart)
        {
            RequestOnlineDatasetLoad(false);
        }
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
        lastActionMessage = "Simulation started.";
        Debug.Log(lastActionMessage);
        BindRegionViews();
        RefreshAllViews();
    }

    public void PauseSimulation()
    {
        isSimulationRunning = false;
        dayTimer = 0f;
        lastActionMessage = "Simulation paused.";
        Debug.Log(lastActionMessage);
        RefreshAllViews();
    }

    public void AdvanceOneDay()
    {
        if (maxDays > 0 && currentDay >= maxDays)
        {
            if (isSimulationRunning)
            {
                PauseSimulation();
            }

            lastActionMessage = $"Simulation paused at max day limit ({maxDays}).";
            Debug.Log(lastActionMessage);
            RefreshAllViews();
            return;
        }

        float effectiveInfectionRate = activeScenarioData != null ? activeScenarioData.infectionRate : infectionRate;
        float effectiveExposedToInfectedRate = activeScenarioData != null ? activeScenarioData.exposedToInfectedRate : exposedToInfectedRate;
        float effectiveRecoveryRate = activeScenarioData != null ? activeScenarioData.recoveryRate : recoveryRate;

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
        RecordSnapshot();
        lastActionMessage = $"Advanced to day {currentDay}.";
        RefreshAllViews();

        if (maxDays > 0 && currentDay >= maxDays)
        {
            PauseSimulation();
            lastActionMessage = $"Simulation paused at max day limit ({maxDays}).";
            Debug.Log(lastActionMessage);
            RefreshAllViews();
        }
    }

    public void ApplyLockdownToRegion(int index)
    {
        if (!TryGetRegion(index, out RegionData region))
        {
            lastActionMessage = $"Lockdown failed: invalid region index {index}.";
            Debug.LogWarning(lastActionMessage);
            RefreshAllViews();
            return;
        }

        if (region.IsLockdownActive)
        {
            lastActionMessage = $"Lockdown skipped: {region.RegionName} is already in lockdown.";
            Debug.Log(lastActionMessage);
            RefreshAllViews();
            return;
        }

        if (budget < lockdownCost)
        {
            lastActionMessage = $"Lockdown failed: not enough budget. Needed {lockdownCost:F0}, current {budget:F0}.";
            Debug.LogWarning(lastActionMessage);
            RefreshAllViews();
            return;
        }

        budget -= lockdownCost;
        region.IsLockdownActive = true;
        lastActionMessage = $"Lockdown applied to {region.RegionName}. Cost: {lockdownCost:F0}.";
        AddPolicyEvent(region.RegionName, "Lockdown", $"Lockdown applied to {region.RegionName}.", lockdownCost);
        Debug.Log($"{lastActionMessage} Budget left: {budget:F0}.");
        RefreshAllViews();
    }

    public void ApplyVaccinationToRegion(int index, float amount)
    {
        if (!TryGetRegion(index, out RegionData region))
        {
            lastActionMessage = $"Vaccination failed: invalid region index {index}.";
            Debug.LogWarning(lastActionMessage);
            RefreshAllViews();
            return;
        }

        if (amount <= 0f)
        {
            lastActionMessage = $"Vaccination failed in {region.RegionName}: amount must be positive.";
            Debug.LogWarning(lastActionMessage);
            RefreshAllViews();
            return;
        }

        float peopleToVaccinate = Mathf.Min(amount, region.Susceptible);
        if (peopleToVaccinate <= 0f)
        {
            lastActionMessage = $"Vaccination skipped in {region.RegionName}: no susceptible people available.";
            Debug.Log(lastActionMessage);
            RefreshAllViews();
            return;
        }

        float cost = peopleToVaccinate * vaccinationCostPerPerson;

        if (budget < cost)
        {
            lastActionMessage = $"Vaccination failed in {region.RegionName}: needed {cost:F0}, current budget {budget:F0}.";
            Debug.LogWarning(lastActionMessage);
            RefreshAllViews();
            return;
        }

        budget -= cost;
        region.Susceptible -= peopleToVaccinate;
        region.Recovered += peopleToVaccinate;
        lastActionMessage = $"Vaccinated {peopleToVaccinate:F0} people in {region.RegionName}. Cost: {cost:F0}.";
        AddPolicyEvent(region.RegionName, "Vaccination", $"Vaccinated {peopleToVaccinate:F0} people.", cost);
        Debug.Log($"{lastActionMessage} Budget left: {budget:F0}.");
        RefreshAllViews();
    }

    public void IncreaseHospitalCapacity(int index, int amount)
    {
        if (!TryGetRegion(index, out RegionData region))
        {
            lastActionMessage = $"Hospital capacity increase failed: invalid region index {index}.";
            Debug.LogWarning(lastActionMessage);
            RefreshAllViews();
            return;
        }

        if (amount <= 0)
        {
            lastActionMessage = $"Hospital capacity increase failed in {region.RegionName}: amount must be positive.";
            Debug.LogWarning(lastActionMessage);
            RefreshAllViews();
            return;
        }

        float cost = amount * hospitalCapacityUnitCost;
        if (budget < cost)
        {
            lastActionMessage = $"Hospital increase failed in {region.RegionName}: needed {cost:F0}, current budget {budget:F0}.";
            Debug.LogWarning(lastActionMessage);
            RefreshAllViews();
            return;
        }

        budget -= cost;
        region.HospitalCapacity += amount;
        lastActionMessage = $"Hospital capacity increased in {region.RegionName} by {amount}. Cost: {cost:F0}.";
        AddPolicyEvent(region.RegionName, "Hospital Capacity", $"Increased hospital capacity by {amount}.", cost);
        Debug.Log($"{lastActionMessage} Budget left: {budget:F0}.");
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

    public void OnLoadOnlineDatasetButtonClicked()
    {
        RequestOnlineDatasetLoad(true);
    }

    public void OnNextScenarioButtonClicked()
    {
        if (scenarioManager == null)
        {
            lastDataLoadMessage = "Scenario selection failed: ScenarioManager is not assigned.";
            lastActionMessage = lastDataLoadMessage;
            Debug.LogWarning(lastDataLoadMessage);
            RefreshAllViews();
            return;
        }

        scenarioManager.SelectNextScenario();
        lastDataLoadMessage = $"Selected scenario: {scenarioManager.GetCurrentScenarioName()}";
        lastActionMessage = lastDataLoadMessage;
        RefreshAllViews();
    }

    public void OnPreviousScenarioButtonClicked()
    {
        if (scenarioManager == null)
        {
            lastDataLoadMessage = "Scenario selection failed: ScenarioManager is not assigned.";
            lastActionMessage = lastDataLoadMessage;
            Debug.LogWarning(lastDataLoadMessage);
            RefreshAllViews();
            return;
        }

        scenarioManager.SelectPreviousScenario();
        lastDataLoadMessage = $"Selected scenario: {scenarioManager.GetCurrentScenarioName()}";
        lastActionMessage = lastDataLoadMessage;
        RefreshAllViews();
    }

    public void OnLoadSelectedScenarioButtonClicked()
    {
        if (scenarioManager == null || !scenarioManager.HasValidScenario())
        {
            lastDataLoadMessage = "Selected scenario load failed: no valid ScenarioManager scenario is available.";
            lastActionMessage = lastDataLoadMessage;
            Debug.LogWarning(lastDataLoadMessage);
            RefreshAllViews();
            return;
        }

        ScenarioData selectedScenario = scenarioManager.GetCurrentScenario();
        if (!IsValidScenario(selectedScenario))
        {
            lastDataLoadMessage = "Selected scenario load failed: scenario has no region data.";
            lastActionMessage = lastDataLoadMessage;
            Debug.LogWarning(lastDataLoadMessage);
            RefreshAllViews();
            return;
        }

        LoadScenarioData(selectedScenario, "ScenarioManager", true);
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

    public void OnSaveBaselineButtonClicked()
    {
        SaveCurrentRunAsBaseline();
    }

    public void OnClearBaselineButtonClicked()
    {
        ClearBaseline();
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
        policyHistory.Clear();
        PauseSimulation();
        InitializeDataSourceOrFallback();
        metricsCollector.Clear();
        currentDay = 0;
        selectedRegionIndex = regions.Count > 0 ? 0 : -1;

        for (int i = 0; i < regions.Count; i++)
        {
            regions[i].IsLockdownActive = false;
        }

        BindRegionViews();
        lastActionMessage = "Simulation reset to initial state.";
        RefreshAllViews();
        Debug.Log(lastActionMessage);
    }

    public void SelectRegion(int index)
    {
        if (index < 0 || index >= regions.Count)
        {
            lastActionMessage = $"Region selection failed: invalid index {index}.";
            Debug.LogWarning(lastActionMessage);
            RefreshAllViews();
            return;
        }

        selectedRegionIndex = index;
        lastActionMessage = $"Selected region: {regions[selectedRegionIndex].RegionName}.";
        Debug.Log(lastActionMessage);
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

    public float GetPeakInfected()
    {
        return metricsCollector.GetPeakInfected();
    }

    public int GetHospitalOverloadDays()
    {
        return metricsCollector.GetHospitalOverloadDays();
    }

    public float GetFinalRecovered()
    {
        return metricsCollector.GetFinalRecovered();
    }

    public float GetLatestAverageHospitalLoad()
    {
        return metricsCollector.GetLatestAverageHospitalLoad();
    }

    public int GetLatestOverloadedRegionCount()
    {
        return metricsCollector.GetLatestOverloadedRegionCount();
    }

    public SimulationRunSummary GetCurrentRunSummary(string runName = "Current Run")
    {
        return new SimulationRunSummary
        {
            runName = string.IsNullOrWhiteSpace(runName) ? "Current Run" : runName,
            finalDay = currentDay,
            peakInfected = GetPeakInfected(),
            finalRecovered = GetFinalRecovered(),
            hospitalOverloadDays = GetHospitalOverloadDays(),
            budgetRemaining = budget
        };
    }

    public void SaveCurrentRunAsBaseline()
    {
        baselineSummary = GetCurrentRunSummary("Baseline Run");
        lastActionMessage = $"Saved baseline run at day {baselineSummary.finalDay}.";
        Debug.Log(lastActionMessage);
        RefreshAllViews();
    }

    public void ClearBaseline()
    {
        baselineSummary = null;
        lastActionMessage = "Baseline comparison cleared.";
        Debug.Log(lastActionMessage);
        RefreshAllViews();
    }

    public string GetComparisonText()
    {
        if (!HasBaselineSummary)
        {
            return "No baseline run saved.";
        }

        SimulationRunSummary currentSummary = GetCurrentRunSummary();
        float peakInfectedDifference = currentSummary.peakInfected - baselineSummary.peakInfected;
        float finalRecoveredDifference = currentSummary.finalRecovered - baselineSummary.finalRecovered;
        int overloadDaysDifference = currentSummary.hospitalOverloadDays - baselineSummary.hospitalOverloadDays;
        float budgetDifference = currentSummary.budgetRemaining - baselineSummary.budgetRemaining;

        return
            $"Baseline: {baselineSummary.runName}\n" +
            $"Peak infected difference: {FormatSignedFloat(peakInfectedDifference)}\n" +
            $"Recovered difference: {FormatSignedFloat(finalRecoveredDifference)}\n" +
            $"Hospital overload days difference: {FormatSignedInt(overloadDaysDifference)}\n" +
            $"Budget remaining difference: {FormatSignedFloat(budgetDifference)}";
    }

    public string GetRecentPolicyHistoryText(int maxEvents = 3)
    {
        if (policyHistory.Count == 0)
        {
            return "No policy actions yet.";
        }

        int safeMaxEvents = Mathf.Max(1, maxEvents);
        int startIndex = Mathf.Max(0, policyHistory.Count - safeMaxEvents);
        string historyText = string.Empty;

        for (int i = startIndex; i < policyHistory.Count; i++)
        {
            PolicyEvent policyEvent = policyHistory[i];
            if (!string.IsNullOrEmpty(historyText))
            {
                historyText += "\n";
            }

            historyText += $"Day {policyEvent.day}: {policyEvent.policyType} in {policyEvent.regionName} - {policyEvent.description} Cost: {policyEvent.cost:F0}";
        }

        return historyText;
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

    private void RequestOnlineDatasetLoad(bool applyAfterSuccess)
    {
        if (onlineDatasetLoader == null)
        {
            lastDataLoadMessage = "Online dataset load failed: loader is not assigned.";
            lastActionMessage = lastDataLoadMessage;
            Debug.LogWarning(lastDataLoadMessage);
            RefreshAllViews();
            return;
        }

        lastDataLoadMessage = "Online dataset load requested.";
        lastActionMessage = "Online dataset load requested.";
        RefreshAllViews();

        onlineDatasetLoader.LoadFromUrl(
            dataset =>
            {
                if (applyAfterSuccess)
                {
                    ApplyOnlineDataset(dataset, true);
                    return;
                }

                lastDataLoadMessage = onlineDatasetLoader.LastStatusMessage;
                lastActionMessage = lastDataLoadMessage;
                RefreshAllViews();
            },
            message =>
            {
                lastDataLoadMessage = message;
                lastActionMessage = $"Online dataset load failed: {message}";
                Debug.LogWarning("Online dataset failed. Current simulation state was kept unchanged.");
                RefreshAllViews();
            });
    }

    private bool TryApplyLoadedOnlineDataset()
    {
        if (!preferLoadedOnlineDataset || onlineDatasetLoader == null || !onlineDatasetLoader.HasLoadedDataset)
        {
            return false;
        }

        return ApplyOnlineDataset(onlineDatasetLoader.LoadedDataset, false);
    }

    private bool ApplyOnlineDataset(PandemicDataset dataset, bool markAsPreferred)
    {
        List<RegionData> onlineRegions = ScenarioFromDatasetBuilder.BuildRegions(dataset);
        if (onlineRegions.Count == 0)
        {
            lastDataLoadMessage = "Online dataset loaded, but it had no valid records. Current simulation state was kept unchanged.";
            lastActionMessage = lastDataLoadMessage;
            Debug.LogWarning(lastDataLoadMessage);
            RefreshAllViews();
            return false;
        }

        PauseSimulation();
        if (markAsPreferred)
        {
            // A new explicit dataset can represent different conditions, so old baselines are cleared to avoid misleading comparisons.
            baselineSummary = null;
        }

        regions.Clear();
        metricsCollector.Clear();
        policyHistory.Clear();
        regions.AddRange(onlineRegions);
        currentDay = 0;
        budget = startingBudget;
        selectedRegionIndex = regions.Count > 0 ? 0 : -1;
        activeScenarioData = null;
        preferLoadedOnlineDataset = markAsPreferred || preferLoadedOnlineDataset;

        string datasetName = string.IsNullOrWhiteSpace(dataset.datasetName) ? "Unnamed Online Dataset" : dataset.datasetName;
        string sourceDescription = string.IsNullOrWhiteSpace(dataset.sourceDescription) ? "No source description provided" : dataset.sourceDescription;
        loadedDataSourceLabel = $"Online Dataset: {datasetName}";
        lastDataLoadMessage = $"Online dataset loaded: {datasetName}";
        lastActionMessage = $"Loaded online dataset: {datasetName}.";

        Debug.Log($"Online dataset applied: {datasetName}. Source: {sourceDescription}");
        BindRegionViews();
        RefreshAllViews();
        return true;
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

    private void InitializeDataSourceOrFallback()
    {
        if (TryApplyLoadedOnlineDataset())
        {
            return;
        }

        if (TryInitializeFromLocalDataset())
        {
            return;
        }

        if (TryInitializeFromScenarioManager())
        {
            return;
        }

        if (TryInitializeFromDirectScenarioData())
        {
            return;
        }

        InitializeFallbackSampleData();
    }

    // Local datasets let us demonstrate educational data variations without online loading.
    private bool TryInitializeFromLocalDataset()
    {
        if (localDatasetJson == null)
        {
            return false;
        }

        if (!DatasetParser.TryParseJson(localDatasetJson.text, out PandemicDataset dataset))
        {
            lastDataLoadMessage = "Local dataset could not be loaded. Trying ScenarioData or fallback sample data.";
            lastActionMessage = lastDataLoadMessage;
            Debug.LogWarning("Local dataset could not be loaded. Trying ScenarioData or fallback sample data.");
            return false;
        }

        List<RegionData> datasetRegions = ScenarioFromDatasetBuilder.BuildRegions(dataset);
        if (datasetRegions.Count == 0)
        {
            lastDataLoadMessage = "Local dataset has no valid records. Trying ScenarioData or fallback sample data.";
            lastActionMessage = lastDataLoadMessage;
            Debug.LogWarning("Local dataset has no valid records. Trying ScenarioData or fallback sample data.");
            return false;
        }

        regions.Clear();
        metricsCollector.Clear();
        policyHistory.Clear();
        regions.AddRange(datasetRegions);
        budget = startingBudget;
        activeScenarioData = null;
        preferLoadedOnlineDataset = false;

        string datasetName = string.IsNullOrWhiteSpace(dataset.datasetName) ? "Unnamed Local Dataset" : dataset.datasetName;
        string sourceDescription = string.IsNullOrWhiteSpace(dataset.sourceDescription) ? "No source description provided" : dataset.sourceDescription;
        loadedDataSourceLabel = $"Local Dataset: {datasetName}";
        lastDataLoadMessage = $"Local dataset loaded: {datasetName}";
        lastActionMessage = $"Loaded local dataset: {datasetName}.";
        Debug.Log($"Local dataset loaded: {datasetName}. Source: {sourceDescription}");
        return true;
    }

    private bool TryInitializeFromScenarioManager()
    {
        if (scenarioManager == null || !scenarioManager.HasValidScenario())
        {
            return false;
        }

        ScenarioData selectedScenario = scenarioManager.GetCurrentScenario();
        if (!IsValidScenario(selectedScenario))
        {
            lastActionMessage = "ScenarioManager current scenario is missing region data. Trying direct ScenarioData or fallback sample data.";
            Debug.LogWarning(lastActionMessage);
            return false;
        }

        LoadScenarioData(selectedScenario, "ScenarioManager", false);
        return true;
    }

    private bool TryInitializeFromDirectScenarioData()
    {
        if (!IsValidScenario(scenarioData))
        {
            return false;
        }

        LoadScenarioData(scenarioData, "ScenarioData", false);
        return true;
    }

    // Scenario assets let us test different pandemic conditions without changing code.
    private void LoadScenarioData(ScenarioData sourceScenario, string sourceLabel, bool resetSimulation)
    {
        if (resetSimulation)
        {
            PauseSimulation();
            metricsCollector.Clear();
            currentDay = 0;
            // Loading a new selected scenario clears the baseline because comparisons should stay within the same scenario.
            baselineSummary = null;
        }

        regions.Clear();
        metricsCollector.Clear();
        policyHistory.Clear();

        for (int i = 0; i < sourceScenario.regions.Count; i++)
        {
            RegionInitialData source = sourceScenario.regions[i];
            regions.Add(CreateRegion(
                source.regionName,
                source.population,
                source.susceptible,
                source.exposed,
                source.infected,
                source.recovered,
                source.hospitalCapacity));
        }

        budget = sourceScenario.startingBudget;
        activeScenarioData = sourceScenario;
        preferLoadedOnlineDataset = false;

        string scenarioName = string.IsNullOrWhiteSpace(sourceScenario.scenarioName) ? "Unnamed Scenario" : sourceScenario.scenarioName;
        loadedDataSourceLabel = $"{sourceLabel}: {scenarioName}";
        lastDataLoadMessage = $"{sourceLabel} loaded: {scenarioName}";
        lastActionMessage = $"{sourceLabel} loaded: {scenarioName}.";
        Debug.Log($"{sourceLabel} loaded: {scenarioName}");

        if (resetSimulation)
        {
            selectedRegionIndex = regions.Count > 0 ? 0 : -1;
            BindRegionViews();
            RefreshAllViews();
        }
    }

    private bool IsValidScenario(ScenarioData sourceScenario)
    {
        return sourceScenario != null && sourceScenario.regions != null && sourceScenario.regions.Count > 0;
    }

    private void InitializeFallbackSampleData()
    {
        CreateSampleRegions();
        metricsCollector.Clear();
        policyHistory.Clear();
        budget = startingBudget;
        activeScenarioData = null;
        preferLoadedOnlineDataset = false;
        loadedDataSourceLabel = "Fallback Sample (Medium Outbreak)";
        lastDataLoadMessage = "Using built-in fallback sample data.";
        lastActionMessage = "Using built-in fallback sample data.";
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

    private void RecordSnapshot()
    {
        metricsCollector.AddSnapshot(CreateSnapshot());
    }

    private void AddPolicyEvent(string regionName, string policyType, string description, float cost)
    {
        policyHistory.Add(new PolicyEvent
        {
            day = currentDay,
            regionName = regionName,
            policyType = policyType,
            description = description,
            cost = cost
        });
    }

    private string FormatSignedFloat(float value)
    {
        return value >= 0f ? $"+{value:F0}" : value.ToString("F0");
    }

    private string FormatSignedInt(int value)
    {
        return value >= 0 ? $"+{value}" : value.ToString();
    }

    private SimulationSnapshot CreateSnapshot()
    {
        float totalSusceptible = 0f;
        float totalExposed = 0f;
        float totalInfected = 0f;
        float totalRecovered = 0f;
        float totalHospitalLoad = 0f;
        int overloadedRegionCount = 0;

        for (int i = 0; i < regions.Count; i++)
        {
            RegionData region = regions[i];
            totalSusceptible += region.Susceptible;
            totalExposed += region.Exposed;
            totalInfected += region.Infected;
            totalRecovered += region.Recovered;
            totalHospitalLoad += region.HospitalLoadRatio;

            if (region.HospitalLoadRatio > 1f)
            {
                overloadedRegionCount++;
            }
        }

        float averageHospitalLoad = regions.Count > 0 ? totalHospitalLoad / regions.Count : 0f;

        return new SimulationSnapshot
        {
            day = currentDay,
            totalSusceptible = totalSusceptible,
            totalExposed = totalExposed,
            totalInfected = totalInfected,
            totalRecovered = totalRecovered,
            budget = budget,
            averageHospitalLoad = averageHospitalLoad,
            overloadedRegionCount = overloadedRegionCount
        };
    }
}
