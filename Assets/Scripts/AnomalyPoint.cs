using UnityEngine;

public enum AnomalyType
{
    Wall,
    Ceiling,
    Floor,
    Object,
    NPC
}

public class AnomalyPoint : MonoBehaviour
{
    [Header("Point Settings")]
    public AnomalyType type;

    [Tooltip("The normal object in the base map that should be hidden when an anomaly spawns")]
    public GameObject normalObject;
    
    [Tooltip("List of Anomaly Prefabs that can spawn at this specific point")]
    public GameObject[] anomalyPrefabs;

    // Show or hide the normal base object
    public void SetNormalObjectActive(bool isActive)
    {
        if (normalObject != null)
        {
            normalObject.SetActive(isActive);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * 0.8f);
    }
}