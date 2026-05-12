public static class EducationalInfoProvider
{
    public static string GetSusceptibleExplanation()
    {
        return "Susceptible people can still catch the disease. In this simplified educational model, they may move to Exposed when infection spreads.";
    }

    public static string GetExposedExplanation()
    {
        return "Exposed people have been infected but are not counted as actively infected yet. This keeps the SEIR flow easy to understand for students.";
    }

    public static string GetInfectedExplanation()
    {
        return "Infected people are currently sick or infectious in the simulation. More infected people can increase spread in a region.";
    }

    public static string GetRecoveredExplanation()
    {
        return "Recovered people are no longer part of active spread in this simplified model. Vaccination also moves people into this protected group.";
    }

    public static string GetLockdownExplanation()
    {
        return "Lockdown reduces contacts in a region. In SimuCrisis, it is simplified as a 50% reduction to the infection rate.";
    }

    public static string GetVaccinationExplanation()
    {
        return "Vaccination moves some susceptible people to recovered/protected. This is an educational simplification, not a medical prediction.";
    }

    public static string GetHospitalCapacityExplanation()
    {
        return "Hospital capacity shows how many infected people a region can handle before overload warnings appear.";
    }

    public static string GetBudgetExplanation()
    {
        return "Budget limits how many interventions can be used. This helps demonstrate tradeoffs in disaster management decisions.";
    }

    public static string GetSEIRExplanation()
    {
        return "SEIR stands for Susceptible, Exposed, Infected, and Recovered. SimuCrisis uses a simplified SEIR model for learning and presentation, not real-world forecasting.";
    }
}
