using UnityEngine;
using System.Collections.Generic;

public class AnomalyManager : MonoBehaviour
{
    [System.Serializable]
    public struct WeightedAnomaly
    {
        public string anomalyName;
        public GameObject prefab;
        [Tooltip("Drag the original object from Scene Hierarchy that this anomaly replaces")]
        public GameObject normalObject;
        [Tooltip("Drag the 'Mark Anomaly' transform from Scene Hierarchy for exact position")]
        public Transform anomalyMark;
        [Tooltip("Higher weight = higher relative spawn probability (0 = disabled)")]
        [Min(0f)] public float weight;
    }

    [Header("Debug Settings")]
    [Tooltip("Enable for sequential testing. Disable for real gameplay with weighted spawns.")]
    [SerializeField] private bool debugMode = false;

    [Header("Game Settings")]
    [SerializeField] private float anomalyChance = 0.6f;

    [Header("Weighted Anomaly Pool")]
    [SerializeField] private List<WeightedAnomaly> anomalyPool;

    // Floor tracking lives in GameManager. This class just reads it.
    public bool IsAnomalyActive { get; private set; } = false;
    public bool CurrentFloorHasAnomaly => IsAnomalyActive;

    private GameObject currentActiveAnomaly;
    private int currentTestIndex = 0;

    // Stores indices of anomalies that have already spawned across runs
    private HashSet<int> spawnedAnomalyIndices = new HashSet<int>();

    public void SetupFloor()
    {
        GenerateNextRoom();
    }

    public void GenerateNextRoom()
    {
        ResetRoomObjects();

        // --- DEBUG TEST MODE (Sequential Cycling) ---
        if (debugMode && anomalyPool != null && anomalyPool.Count > 0)
        {
            IsAnomalyActive = true;
            SpawnAnomalyByIndex(currentTestIndex);
            currentTestIndex = (currentTestIndex + 1) % anomalyPool.Count;
            return;
        }

        // --- NORMAL GAMEPLAY LOGIC (Weighted Random) ---
        int currentFloor = GameManager.Instance.CurrentFloor;
        int startFloor = GameManager.Instance.startingFloor;

        if (currentFloor == startFloor)
        {
            // First floor of a loop is always safe/normal. No reset of anomaly history here.
            IsAnomalyActive = false;
        }
        else
        {
            IsAnomalyActive = Random.value < anomalyChance;
        }

        Debug.Log($"[DEBUG] Floor {currentFloor} Anomaly Active: {IsAnomalyActive}");

        if (IsAnomalyActive && anomalyPool != null && anomalyPool.Count > 0)
        {
            int selectedIndex = GetRandomWeightedAnomalyIndex();
            if (selectedIndex >= 0)
            {
                SpawnAnomalyByIndex(selectedIndex);
            }
        }
    }

    /// <summary>
    /// Call this when the player wins the game or starts a fresh play session.
    /// </summary>
    public void ResetAnomalyHistory()
    {
        spawnedAnomalyIndices.Clear();
        Debug.Log("[DEBUG] Anomaly spawn history cleared.");
    }

    private void ResetRoomObjects()
    {
        if (currentActiveAnomaly != null)
        {
            Destroy(currentActiveAnomaly);
        }

        if (anomalyPool != null)
        {
            foreach (var item in anomalyPool)
            {
                if (item.normalObject != null)
                {
                    item.normalObject.SetActive(false);
                    item.normalObject.SetActive(true);

                    if (item.normalObject.TryGetComponent<NormalGuardAI>(out var guard))
                    {
                        guard.ResetGuard();
                    }
                }
            }
        }
    }

    private int GetRandomWeightedAnomalyIndex()
    {
        float totalWeight = 0f;

        // 1. Sum weight of anomalies that haven't spawned yet
        for (int i = 0; i < anomalyPool.Count; i++)
        {
            var item = anomalyPool[i];
            if (item.prefab != null && item.weight > 0f && !spawnedAnomalyIndices.Contains(i))
            {
                totalWeight += item.weight;
            }
        }

        // Fallback: If ALL anomalies in the pool have been shown, reset history so they can spawn again
        if (totalWeight <= 0f)
        {
            Debug.Log("[DEBUG] All anomalies shown! Resetting pool history.");
            spawnedAnomalyIndices.Clear();

            for (int i = 0; i < anomalyPool.Count; i++)
            {
                var item = anomalyPool[i];
                if (item.prefab != null && item.weight > 0f)
                {
                    totalWeight += item.weight;
                }
            }
            if (totalWeight <= 0f) return -1;
        }

        // 2. Roll random weight
        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        // 3. Find selected index
        for (int i = 0; i < anomalyPool.Count; i++)
        {
            var item = anomalyPool[i];
            if (item.prefab == null || item.weight <= 0f || spawnedAnomalyIndices.Contains(i)) continue;

            cumulativeWeight += item.weight;
            if (randomValue < cumulativeWeight)
            {
                return i;
            }
        }

        return -1;
    }

    private void SpawnAnomalyByIndex(int index)
    {
        if (anomalyPool == null || index < 0 || index >= anomalyPool.Count) return;

        var anomalyData = anomalyPool[index];
        if (anomalyData.prefab == null) return;

        // Mark this anomaly index as shown
        spawnedAnomalyIndices.Add(index);

        // 1. Hide normal object in scene
        if (anomalyData.normalObject != null)
        {
            anomalyData.normalObject.SetActive(false);
        }

        // 2. Position strictly from anomalyMark
        Vector3 spawnPos = anomalyData.anomalyMark != null
            ? anomalyData.anomalyMark.position
            : (anomalyData.normalObject != null ? anomalyData.normalObject.transform.position : anomalyData.prefab.transform.position);

        // 3. Rotation strictly from the Prefab itself
        Quaternion spawnRot = anomalyData.prefab.transform.rotation;

        // 4. Instantiate anomaly prefab
        currentActiveAnomaly = Instantiate(anomalyData.prefab, spawnPos, spawnRot);

        // 5. Parent maintaining world transform
        if (anomalyData.anomalyMark != null)
        {
            currentActiveAnomaly.transform.SetParent(anomalyData.anomalyMark.parent, true);
        }
        else if (anomalyData.normalObject != null)
        {
            currentActiveAnomaly.transform.SetParent(anomalyData.normalObject.transform.parent, true);
        }

        currentActiveAnomaly.SetActive(true);

        Debug.Log($"[DEBUG] Spawning Anomaly [{index}]: {anomalyData.anomalyName}");
    }
}