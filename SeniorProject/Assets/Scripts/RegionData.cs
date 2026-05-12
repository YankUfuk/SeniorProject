using System;

[Serializable]
public class RegionData
{
    public string RegionName;
    public int Population;
    public float Susceptible;
    public float Exposed;
    public float Infected;
    public float Recovered;
    public int HospitalCapacity;
    public bool IsLockdownActive;

    public float InfectionRatio
    {
        get
        {
            if (Population <= 0)
            {
                return 0f;
            }

            return Infected / Population;
        }
    }

    public float HospitalLoadRatio
    {
        get
        {
            if (HospitalCapacity <= 0)
            {
                return Infected > 0f ? float.PositiveInfinity : 0f;
            }

            return Infected / HospitalCapacity;
        }
    }
}
