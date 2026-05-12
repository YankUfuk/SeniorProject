using UnityEngine;

public static class DatasetParser
{
    public static bool TryParseJson(string json, out PandemicDataset dataset)
    {
        dataset = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning("Dataset parsing failed: JSON text is empty.");
            return false;
        }

        try
        {
            dataset = JsonUtility.FromJson<PandemicDataset>(json);
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"Dataset parsing failed: {exception.Message}");
            return false;
        }

        if (dataset == null)
        {
            Debug.LogWarning("Dataset parsing failed: dataset is null.");
            return false;
        }

        if (dataset.records == null)
        {
            Debug.LogWarning("Dataset parsing failed: records list is missing.");
            return false;
        }

        return true;
    }
}
