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
    [SerializeField] private int startStage = 8;

    [Header("Weighted Anomaly Pool")]
    [SerializeField] private List<WeightedAnomaly> anomalyPool;

    public int CurrentStage { get; private set; } = 8;
    public bool IsAnomalyActive { get; private set; } = false;
    public bool CurrentFloorHasAnomaly => IsAnomalyActive;

    private GameObject currentActiveAnomaly;
    private int currentTestIndex = 0;

    void Start()
    {
        CurrentStage = startStage;
    }

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
        if (CurrentStage == startStage)
        {
            IsAnomalyActive = false;
        }
        else
        {
            IsAnomalyActive = Random.value < anomalyChance;
        }

        if (IsAnomalyActive && anomalyPool != null && anomalyPool.Count > 0)
        {
            int selectedIndex = GetRandomWeightedAnomalyIndex();
            if (selectedIndex >= 0)
            {
                SpawnAnomalyByIndex(selectedIndex);
            }
        }
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
                    item.normalObject.SetActive(true);
                }
            }
        }
    }

    private int GetRandomWeightedAnomalyIndex()
    {
        float totalWeight = 0f;

        // 1. Sum total weight, ignoring items with weight <= 0
        foreach (var item in anomalyPool)
        {
            if (item.prefab != null && item.weight > 0f)
            {
                totalWeight += item.weight;
            }
        }

        if (totalWeight <= 0f) return -1;

        // 2. Roll a random weight value
        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        // 3. Find the selected anomaly based on cumulative weight
        for (int i = 0; i < anomalyPool.Count; i++)
        {
            var item = anomalyPool[i];
            if (item.prefab == null || item.weight <= 0f) continue;

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

        // 1. Hide normal object in the scene
        if (anomalyData.normalObject != null)
        {
            anomalyData.normalObject.SetActive(false);
        }

        // 2. Position strictly from anomalyMark (fallback to normalObject if mark missing)
        Vector3 spawnPos = anomalyData.anomalyMark != null 
            ? anomalyData.anomalyMark.position 
            : (anomalyData.normalObject != null ? anomalyData.normalObject.transform.position : anomalyData.prefab.transform.position);

        // 3. Rotation strictly from the Prefab itself
        Quaternion spawnRot = anomalyData.prefab.transform.rotation;

        // 4. Instantiate prefab with Mark Position & Prefab Rotation
        currentActiveAnomaly = Instantiate(anomalyData.prefab, spawnPos, spawnRot);

        // 5. Parent to Scene Hierarchy maintaining world transform
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

    public void SubmitPlayerChoice(bool choseToTurnBack)
    {
        bool isCorrectChoice = (IsAnomalyActive && choseToTurnBack) || (!IsAnomalyActive && !choseToTurnBack);

        if (isCorrectChoice)
        {
            CurrentStage--;
            Debug.Log($"Correct choice! Current Exit sign: Exit {CurrentStage}");

            if (CurrentStage <= 0)
            {
                TriggerWinSequence();
                return;
            }
        }
        else
        {
            CurrentStage = startStage;
            Debug.Log("Wrong choice! Loop reset back to Exit 8.");
        }

        SetupFloor();
    }

    private void TriggerWinSequence()
    {
        Debug.Log("Congratulations! Successfully escaped through Exit 0.");
    }
}