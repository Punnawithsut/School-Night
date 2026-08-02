using System.Collections.Generic;
using UnityEngine;

public class AnomalyManager : MonoBehaviour
{
    [Header("Anomaly Spawn Settings")]
    [Range(0f, 1f)] 
    public float anomalyChance = 0f;

    [Header("Registered Points")]
    public List<AnomalyPoint> spawnPoints = new List<AnomalyPoint>();

    private GameObject currentSpawnedAnomaly;
    private AnomalyPoint currentActivePoint;

    public bool CurrentFloorHasAnomaly { get; private set; }

    private void Start()
    {
        SetupFloor();
    }

    // Automatically triggers when you modify values in the Inspector during Play Mode
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            SetupFloor();
        }
    }

    public void SetupFloor()
    {
        ClearCurrentAnomaly();

        if (Random.value < anomalyChance && spawnPoints.Count > 0)
        {
            AnomalyPoint selectedPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

            if (selectedPoint.anomalyPrefabs != null && selectedPoint.anomalyPrefabs.Length > 0)
            {
                int randomIndex = Random.Range(0, selectedPoint.anomalyPrefabs.Length);
                GameObject selectedPrefab = selectedPoint.anomalyPrefabs[randomIndex];

                selectedPoint.SetNormalObjectActive(false);
                currentActivePoint = selectedPoint;

                currentSpawnedAnomaly = Instantiate(
                    selectedPrefab, 
                    selectedPoint.transform.position, 
                    selectedPoint.transform.rotation, 
                    selectedPoint.transform
                );

                CurrentFloorHasAnomaly = true;
                Debug.Log($"<color=red>[Anomaly System]</color> Anomaly SPAWNED! Point: <b>{selectedPoint.gameObject.name}</b> | Prefab: <b>{selectedPrefab.name}</b>");
                return;
            }
        }

        CurrentFloorHasAnomaly = false;
        Debug.Log("<color=green>[Anomaly System]</color> Normal Floor - No Anomaly.");
    }

    public void SetupNewLoop()
    {
        SetupFloor();
    }

    public void ClearCurrentAnomaly()
    {
        if (currentSpawnedAnomaly != null)
        {
            Destroy(currentSpawnedAnomaly);
            currentSpawnedAnomaly = null;
        }

        if (currentActivePoint != null)
        {
            currentActivePoint.SetNormalObjectActive(true);
            currentActivePoint = null;
        }

        CurrentFloorHasAnomaly = false;
    }
}