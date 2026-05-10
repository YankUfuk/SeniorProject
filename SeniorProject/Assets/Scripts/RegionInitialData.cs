using System;

[Serializable]
public class RegionInitialData
{
    public string regionName;
    public int population;
    public float susceptible;
    public float exposed;
    public float infected;
    public float recovered;
    public int hospitalCapacity;
}
