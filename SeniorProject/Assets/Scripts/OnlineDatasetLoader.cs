using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class OnlineDatasetLoader : MonoBehaviour
{
    [SerializeField] private string datasetUrl;
    [SerializeField] private float requestTimeoutSeconds = 5f;
    [SerializeField] private bool loadOnStart = false;

    public PandemicDataset LoadedDataset { get; private set; }
    public bool HasLoadedDataset => LoadedDataset != null;
    public string LastStatusMessage { get; private set; } = "No online dataset loaded.";
    public bool LoadOnStart => loadOnStart;

    public void LoadFromUrl(Action<PandemicDataset> onSuccess = null, Action<string> onFail = null)
    {
        if (string.IsNullOrWhiteSpace(datasetUrl))
        {
            Fail("Online dataset load failed: URL is empty.", onFail);
            return;
        }

        StartCoroutine(LoadFromUrlRoutine(onSuccess, onFail));
    }

    private IEnumerator LoadFromUrlRoutine(Action<PandemicDataset> onSuccess, Action<string> onFail)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(datasetUrl))
        {
            request.timeout = Mathf.Max(1, Mathf.CeilToInt(requestTimeoutSeconds));

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Fail($"Online dataset load failed: {request.error}", onFail);
                yield break;
            }

            string json = request.downloadHandler.text;
            if (!DatasetParser.TryParseJson(json, out PandemicDataset dataset))
            {
                Fail("Online dataset load failed: response JSON could not be parsed.", onFail);
                yield break;
            }

            LoadedDataset = dataset;
            LastStatusMessage = "Online dataset loaded successfully.";
            Debug.Log(LastStatusMessage);
            onSuccess?.Invoke(dataset);
        }
    }

    private void Fail(string message, Action<string> onFail)
    {
        LastStatusMessage = message;
        Debug.LogWarning(message);
        onFail?.Invoke(message);
    }
}
