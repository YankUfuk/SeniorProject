using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DemoPresentationController : MonoBehaviour
{
    [Header("Demo Regions")]
    public List<DemoRegionVisual> regions = new List<DemoRegionVisual>();

    [Header("Dashboard Text")]
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI selectedRegionText;
    public TextMeshProUGUI riskText;
    public TextMeshProUGUI budgetText;
    public TextMeshProUGUI hospitalStatusText;
    public TextMeshProUGUI lastActionText;
    public TextMeshProUGUI goalText;
    public TextMeshProUGUI publicTrustText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI outcomeText;

    [Header("Demo Settings")]
    public int dayLimit = 30;
    public int startingBudget = 1000;
    public int startingPublicTrust = 100;
    public int lockdownCost = 180;
    public int vaccinationCost = 140;
    public int hospitalSupportCost = 120;

    private readonly string[] startingRegionNames = { "North", "South", "East", "West", "Central" };
    private readonly float[] startingRisks = { 0.35f, 0.45f, 0.55f, 0.30f, 0.70f };
    private int selectedRegionIndex;
    private int day;
    private int budget;
    private int publicTrust;
    private bool isDemoFinished;
    private int hospitalInvestmentLevel;
    private string lastActionMessage = "Stabilize all districts before time runs out.";
    private string outcomeMessage = string.Empty;

    private void Start()
    {
        ClampSettings();
        ResetDemo();
    }

    private void OnValidate()
    {
        ClampSettings();
    }

    [ContextMenu("Apply Default Demo Settings")]
    private void ApplyDefaultDemoSettings()
    {
        dayLimit = 30;
        startingBudget = 1000;
        startingPublicTrust = 100;
        lockdownCost = 180;
        vaccinationCost = 140;
        hospitalSupportCost = 120;
    }

    public void SelectRegion(int index)
    {
        if (regions == null || index < 0 || index >= regions.Count || regions[index] == null)
        {
            lastActionMessage = $"Cannot select region {index}.";
            RefreshUI();
            return;
        }

        selectedRegionIndex = index;
        lastActionMessage = $"Selected {regions[selectedRegionIndex].regionName}.";
        RefreshUI();
    }

    public void NextDay()
    {
        if (isDemoFinished)
        {
            lastActionMessage = "Demo finished. Press Reset to play again.";
            RefreshUI();
            return;
        }

        day++;

        int criticalRegionCount = CountCriticalRegions();
        float globalPressure = criticalRegionCount > 0 ? 0.01f : 0f;

        if (regions != null)
        {
            for (int i = 0; i < regions.Count; i++)
            {
                if (regions[i] == null)
                {
                    continue;
                }

                float dailyIncrease = regions[i].riskLevel < 0.25f ? 0.02f : 0.03f;
                regions[i].AddRisk(dailyIncrease + globalPressure);
            }
        }

        publicTrust = Mathf.Max(0, publicTrust - 1);
        lastActionMessage = $"Day {day}: citywide risk pressure increased.";
        EvaluateOutcome();
        RefreshUI();
    }

    public void ApplyLockdown()
    {
        if (isDemoFinished)
        {
            lastActionMessage = "Demo finished. Press Reset to play again.";
            RefreshUI();
            return;
        }

        DemoRegionVisual region = GetSelectedRegion();
        if (region == null)
        {
            lastActionMessage = "Lockdown failed: no region selected.";
            RefreshUI();
            return;
        }

        if (!TrySpendBudget(lockdownCost, "Lockdown"))
        {
            return;
        }

        region.ReduceRisk(0.25f);
        publicTrust = Mathf.Max(0, publicTrust - 8);
        lastActionMessage = $"Lockdown strongly reduced risk in {region.regionName}, but public trust fell.";
        EvaluateOutcome();
        RefreshUI();
    }

    public void ApplyVaccination()
    {
        if (isDemoFinished)
        {
            lastActionMessage = "Demo finished. Press Reset to play again.";
            RefreshUI();
            return;
        }

        DemoRegionVisual region = GetSelectedRegion();
        if (region == null)
        {
            lastActionMessage = "Vaccination failed: no region selected.";
            RefreshUI();
            return;
        }

        if (!TrySpendBudget(vaccinationCost, "Vaccination"))
        {
            return;
        }

        region.ReduceRisk(0.18f);
        publicTrust = Mathf.Min(100, publicTrust + 3);
        lastActionMessage = $"Vaccination reduced risk in {region.regionName} and improved trust.";
        EvaluateOutcome();
        RefreshUI();
    }

    public void IncreaseHospitalCapacity()
    {
        if (isDemoFinished)
        {
            lastActionMessage = "Demo finished. Press Reset to play again.";
            RefreshUI();
            return;
        }

        DemoRegionVisual region = GetSelectedRegion();
        if (region == null)
        {
            lastActionMessage = "Hospital support failed: no region selected.";
            RefreshUI();
            return;
        }

        if (!TrySpendBudget(hospitalSupportCost, "Hospital support"))
        {
            return;
        }

        hospitalInvestmentLevel++;
        region.ReduceRisk(0.10f);
        lastActionMessage = $"Hospital support reduced risk in {region.regionName}.";
        EvaluateOutcome();
        RefreshUI();
    }

    public void ResetDemo()
    {
        ClampSettings();
        day = 0;
        budget = startingBudget;
        publicTrust = startingPublicTrust;
        hospitalInvestmentLevel = 0;
        isDemoFinished = false;
        outcomeMessage = string.Empty;
        selectedRegionIndex = regions != null && regions.Count > 0 ? Mathf.Clamp(selectedRegionIndex, 0, regions.Count - 1) : 0;

        if (regions != null)
        {
            for (int i = 0; i < regions.Count; i++)
            {
                if (regions[i] == null)
                {
                    continue;
                }

                regions[i].regionName = startingRegionNames[Mathf.Min(i, startingRegionNames.Length - 1)];
                regions[i].SetRiskLevel(startingRisks[Mathf.Min(i, startingRisks.Length - 1)]);
            }
        }

        lastActionMessage = "Medium outbreak scenario loaded. Make all districts green.";
        RefreshUI();
    }

    private void ClampSettings()
    {
        dayLimit = Mathf.Max(1, dayLimit);
        startingBudget = Mathf.Max(100, startingBudget);
        startingPublicTrust = Mathf.Clamp(startingPublicTrust, 0, 100);
        lockdownCost = Mathf.Max(0, lockdownCost);
        vaccinationCost = Mathf.Max(0, vaccinationCost);
        hospitalSupportCost = Mathf.Max(0, hospitalSupportCost);
    }

    public void RefreshUI()
    {
        RefreshRegionSelection();

        if (dayText != null)
        {
            dayText.text = $"Day: {day} / {dayLimit}";
        }

        DemoRegionVisual selectedRegion = GetSelectedRegion();
        if (selectedRegionText != null)
        {
            selectedRegionText.text = selectedRegion != null
                ? $"Selected Region: {selectedRegion.regionName}"
                : "Selected Region: None";
        }

        if (riskText != null)
        {
            riskText.text =
                $"Average Risk: {GetAverageRisk():P0}\n" +
                $"Green Districts: {CountGreenRegions()} / {GetValidRegionCount()}";
        }

        if (budgetText != null)
        {
            budgetText.text = $"Budget: {budget}";
        }

        if (publicTrustText != null)
        {
            publicTrustText.text = $"Public Trust: {publicTrust}";
        }

        if (hospitalStatusText != null)
        {
            hospitalStatusText.text = GetHospitalStatus();
        }

        if (goalText != null)
        {
            goalText.text = $"Goal: Make all districts GREEN before Day {dayLimit}.";
        }

        if (scoreText != null)
        {
            scoreText.text = $"Score: {CalculateScore()}";
        }

        if (outcomeText != null)
        {
            outcomeText.text = string.IsNullOrEmpty(outcomeMessage)
                ? "Outcome: In Progress"
                : outcomeMessage;
        }

        if (lastActionText != null)
        {
            lastActionText.text = $"Last Action: {lastActionMessage}";
        }
    }

    public void OnNextDayClicked()
    {
        NextDay();
    }

    public void OnLockdownClicked()
    {
        ApplyLockdown();
    }

    public void OnVaccinationClicked()
    {
        ApplyVaccination();
    }

    public void OnHospitalClicked()
    {
        IncreaseHospitalCapacity();
    }

    public void OnResetClicked()
    {
        ResetDemo();
    }

    public void OnSelectRegion0()
    {
        SelectRegion(0);
    }

    public void OnSelectRegion1()
    {
        SelectRegion(1);
    }

    public void OnSelectRegion2()
    {
        SelectRegion(2);
    }

    public void OnSelectRegion3()
    {
        SelectRegion(3);
    }

    public void OnSelectRegion4()
    {
        SelectRegion(4);
    }

    public bool AreAllRegionsGreen()
    {
        int validRegionCount = GetValidRegionCount();
        return validRegionCount > 0 && CountGreenRegions() == validRegionCount;
    }

    public int CountGreenRegions()
    {
        if (regions == null)
        {
            return 0;
        }

        int greenCount = 0;
        for (int i = 0; i < regions.Count; i++)
        {
            if (regions[i] != null && regions[i].riskLevel < 0.25f)
            {
                greenCount++;
            }
        }

        return greenCount;
    }

    public float GetAverageRisk()
    {
        if (regions == null || regions.Count == 0)
        {
            return 0f;
        }

        float totalRisk = 0f;
        int validRegionCount = 0;

        for (int i = 0; i < regions.Count; i++)
        {
            if (regions[i] == null)
            {
                continue;
            }

            totalRisk += regions[i].riskLevel;
            validRegionCount++;
        }

        return validRegionCount > 0 ? totalRisk / validRegionCount : 0f;
    }

    public string GetHospitalStatus()
    {
        if (hospitalInvestmentLevel >= 3)
        {
            return "Hospital Status: Strong";
        }

        if (hospitalInvestmentLevel >= 1)
        {
            return "Hospital Status: Improved";
        }

        return "Hospital Status: Basic";
    }

    public int CalculateScore()
    {
        float totalRiskPercent = GetAverageRisk() * 100f;
        return Mathf.RoundToInt(publicTrust + (budget / 10f) - totalRiskPercent);
    }

    public void EvaluateOutcome()
    {
        if (isDemoFinished)
        {
            return;
        }

        if (AreAllRegionsGreen())
        {
            isDemoFinished = true;
            outcomeMessage = "SUCCESS: All districts stabilized.";
            lastActionMessage = outcomeMessage;
            return;
        }

        if (day >= dayLimit)
        {
            isDemoFinished = true;
            outcomeMessage = "FAILED: Crisis was not stabilized in time.";
            lastActionMessage = outcomeMessage;
            return;
        }

        if (budget == 0 && HasOrangeOrRedRegion())
        {
            isDemoFinished = true;
            outcomeMessage = "FAILED: Resources exhausted while high-risk districts remain.";
            lastActionMessage = outcomeMessage;
        }
    }

    private bool TrySpendBudget(int cost, string actionName)
    {
        if (budget < cost)
        {
            lastActionMessage = $"{actionName} failed: not enough budget.";
            RefreshUI();
            return false;
        }

        budget = Mathf.Max(0, budget - cost);
        return true;
    }

    private DemoRegionVisual GetSelectedRegion()
    {
        if (regions == null || selectedRegionIndex < 0 || selectedRegionIndex >= regions.Count)
        {
            return null;
        }

        return regions[selectedRegionIndex];
    }

    private void RefreshRegionSelection()
    {
        if (regions == null)
        {
            return;
        }

        for (int i = 0; i < regions.Count; i++)
        {
            if (regions[i] != null)
            {
                regions[i].SetSelected(i == selectedRegionIndex);
            }
        }
    }

    private int GetValidRegionCount()
    {
        if (regions == null)
        {
            return 0;
        }

        int validRegionCount = 0;
        for (int i = 0; i < regions.Count; i++)
        {
            if (regions[i] != null)
            {
                validRegionCount++;
            }
        }

        return validRegionCount;
    }

    private int CountCriticalRegions()
    {
        if (regions == null)
        {
            return 0;
        }

        int criticalCount = 0;
        for (int i = 0; i < regions.Count; i++)
        {
            if (regions[i] != null && regions[i].riskLevel >= 0.75f)
            {
                criticalCount++;
            }
        }

        return criticalCount;
    }

    private bool HasOrangeOrRedRegion()
    {
        if (regions == null)
        {
            return false;
        }

        for (int i = 0; i < regions.Count; i++)
        {
            if (regions[i] != null && regions[i].riskLevel >= 0.5f)
            {
                return true;
            }
        }

        return false;
    }
}
