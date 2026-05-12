using System;
using UnityEngine;

[Serializable]
public class PandemicDataRecord
{
    [Tooltip("Display name for the region represented by this data row.")]
    public string regionName;

    [Tooltip("Total population for this region.")]
    public int population;

    [Tooltip("Initial number of susceptible people.")]
    public float susceptible;

    [Tooltip("Initial number of exposed people.")]
    public float exposed;

    [Tooltip("Initial number of infected people.")]
    public float infected;

    [Tooltip("Initial number of recovered people.")]
    public float recovered;

    [Tooltip("Available hospital capacity for this region.")]
    public int hospitalCapacity;
}
