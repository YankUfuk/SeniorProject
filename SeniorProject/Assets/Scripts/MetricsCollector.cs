using System.Collections.Generic;

public class MetricsCollector
{
    private readonly List<SimulationSnapshot> snapshots = new List<SimulationSnapshot>();

    public IReadOnlyList<SimulationSnapshot> Snapshots => snapshots;

    public void Clear()
    {
        snapshots.Clear();
    }

    public void AddSnapshot(SimulationSnapshot snapshot)
    {
        if (snapshot != null)
        {
            snapshots.Add(snapshot);
        }
    }

    public SimulationSnapshot GetLatestSnapshot()
    {
        if (snapshots.Count == 0)
        {
            return null;
        }

        return snapshots[snapshots.Count - 1];
    }

    public float GetPeakInfected()
    {
        float peakInfected = 0f;

        for (int i = 0; i < snapshots.Count; i++)
        {
            if (snapshots[i].totalInfected > peakInfected)
            {
                peakInfected = snapshots[i].totalInfected;
            }
        }

        return peakInfected;
    }

    public float GetFinalRecovered()
    {
        SimulationSnapshot latestSnapshot = GetLatestSnapshot();
        return latestSnapshot != null ? latestSnapshot.totalRecovered : 0f;
    }

    public int GetHospitalOverloadDays()
    {
        int overloadDays = 0;

        for (int i = 0; i < snapshots.Count; i++)
        {
            if (snapshots[i].overloadedRegionCount > 0)
            {
                overloadDays++;
            }
        }

        return overloadDays;
    }

    public float GetLatestAverageHospitalLoad()
    {
        SimulationSnapshot latestSnapshot = GetLatestSnapshot();
        return latestSnapshot != null ? latestSnapshot.averageHospitalLoad : 0f;
    }

    public int GetLatestOverloadedRegionCount()
    {
        SimulationSnapshot latestSnapshot = GetLatestSnapshot();
        return latestSnapshot != null ? latestSnapshot.overloadedRegionCount : 0;
    }
}
