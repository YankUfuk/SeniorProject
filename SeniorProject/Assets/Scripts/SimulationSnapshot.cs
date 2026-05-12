using System;

[Serializable]
public class SimulationSnapshot
{
    public int day;
    public float totalSusceptible;
    public float totalExposed;
    public float totalInfected;
    public float totalRecovered;
    public float budget;
    public float averageHospitalLoad;
    public int overloadedRegionCount;
}
