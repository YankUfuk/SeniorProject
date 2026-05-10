using UnityEngine;

public static class DiseaseModel
{
    public static void UpdateOneDay(
        RegionData region,
        float infectionRate,
        float exposedToInfectedRate,
        float recoveryRate)
    {
        if (region == null || region.Population <= 0)
        {
            return;
        }

        float effectiveInfectionRate = region.IsLockdownActive ? infectionRate * 0.5f : infectionRate;

        // Simple SEIR flow:
        // S -> E depends on infected pressure and current susceptible people.
        // E -> I and I -> R are simple percentages per day.
        float newExposed = effectiveInfectionRate * region.Susceptible * (region.Infected / region.Population);
        float newInfected = exposedToInfectedRate * region.Exposed;
        float newRecovered = recoveryRate * region.Infected;

        region.Susceptible -= newExposed;
        region.Exposed += newExposed - newInfected;
        region.Infected += newInfected - newRecovered;
        region.Recovered += newRecovered;

        // Keep every state in a valid range.
        region.Susceptible = Mathf.Clamp(region.Susceptible, 0f, region.Population);
        region.Exposed = Mathf.Clamp(region.Exposed, 0f, region.Population);
        region.Infected = Mathf.Clamp(region.Infected, 0f, region.Population);
        region.Recovered = Mathf.Clamp(region.Recovered, 0f, region.Population);

        // Normalize if small floating-point drift moves total away from population.
        float total = region.Susceptible + region.Exposed + region.Infected + region.Recovered;
        if (total > 0f)
        {
            float scale = region.Population / total;
            region.Susceptible *= scale;
            region.Exposed *= scale;
            region.Infected *= scale;
            region.Recovered *= scale;
        }
    }
}
