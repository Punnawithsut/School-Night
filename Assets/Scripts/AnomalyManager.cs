using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;

public class AnomalyManager : MonoBehaviour
{
    [Header("Anomaly List")]
    public List<Anomaly> anomalies = new List<Anomaly>();

    [Header("Chance")]
    public float anomalyChance = 0.5f;

    private Anomaly currentActiveAnomaly = null;
    public bool CurrentFloorHasAnomaly { get; private set; }

    public void SetupFloor()
    {
        ClearAnomaly();

        CurrentFloorHasAnomaly = Random.value < anomalyChance;

        if (CurrentFloorHasAnomaly && anomalies.Count > 0)
        {
            int index = Random.Range(0, anomalies.Count);
            currentActiveAnomaly = anomalies[index];
            currentActiveAnomaly.Activate();
        }
    }

    private void ClearAnomaly()
    {
        if (currentActiveAnomaly != null)
        {
            currentActiveAnomaly.Deactivate();
            currentActiveAnomaly = null;
        }
    }
}
