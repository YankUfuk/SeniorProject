using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScenarioData", menuName = "SimuCrisis/Scenario Data")]
public class ScenarioData : ScriptableObject
{
    // Scenarios let us test different pandemic setups without changing source code.
    [Header("Scenario Setup")]
    [Tooltip("Simple label shown in logs and inspector.")]
    public string scenarioName = "Medium Outbreak";

    [Tooltip("Budget available at the beginning of the simulation.")]
    public int startingBudget = 1000;

    [Header("Disease Parameters")]
    [Tooltip("Daily infection pressure from infected to susceptible people.")]
    public float infectionRate = 0.25f;

    [Tooltip("Daily share of exposed people who become infected.")]
    public float exposedToInfectedRate = 0.20f;

    [Tooltip("Daily share of infected people who recover.")]
    public float recoveryRate = 0.10f;

    [Header("Initial Regions")]
    [Tooltip("Region-level starting values for this scenario.")]
    public List<RegionInitialData> regions = new List<RegionInitialData>();
}
