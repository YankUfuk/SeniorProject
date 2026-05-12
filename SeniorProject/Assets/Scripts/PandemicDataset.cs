using System;
using System.Collections.Generic;

[Serializable]
public class PandemicDataset
{
    public string datasetName;
    public string sourceDescription;
    public List<PandemicDataRecord> records = new List<PandemicDataRecord>();
}
