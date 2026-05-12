using System.Collections.Generic;
using UnityEngine;

public static class ScenarioFromDatasetBuilder
{
    public static List<RegionData> BuildRegions(PandemicDataset dataset)
    {
        List<RegionData> builtRegions = new List<RegionData>();

        if (dataset == null || dataset.records == null)
        {
            Debug.LogWarning("Dataset build failed: dataset or records list is missing.");
            return builtRegions;
        }

        for (int i = 0; i < dataset.records.Count; i++)
        {
            PandemicDataRecord record = dataset.records[i];
            if (!IsValidRecord(record, i))
            {
                continue;
            }

            builtRegions.Add(new RegionData
            {
                RegionName = record.regionName,
                Population = record.population,
                Susceptible = record.susceptible,
                Exposed = record.exposed,
                Infected = record.infected,
                Recovered = record.recovered,
                HospitalCapacity = record.hospitalCapacity,
                IsLockdownActive = false
            });
        }

        return builtRegions;
    }

    private static bool IsValidRecord(PandemicDataRecord record, int index)
    {
        if (record == null)
        {
            Debug.LogWarning($"Dataset record {index} skipped: record is null.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(record.regionName))
        {
            Debug.LogWarning($"Dataset record {index} skipped: regionName is empty.");
            return false;
        }

        if (record.population <= 0)
        {
            Debug.LogWarning($"Dataset record {record.regionName} skipped: population must be greater than 0.");
            return false;
        }

        if (record.susceptible < 0f || record.exposed < 0f || record.infected < 0f || record.recovered < 0f)
        {
            Debug.LogWarning($"Dataset record {record.regionName} skipped: disease values cannot be negative.");
            return false;
        }

        if (record.hospitalCapacity < 0)
        {
            Debug.LogWarning($"Dataset record {record.regionName} skipped: hospital capacity cannot be negative.");
            return false;
        }

        float diseaseTotal = record.susceptible + record.exposed + record.infected + record.recovered;
        if (diseaseTotal > record.population)
        {
            Debug.LogWarning($"Dataset record {record.regionName} skipped: SEIR total exceeds population.");
            return false;
        }

        return true;
    }
}
