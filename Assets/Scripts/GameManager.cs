using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public AnomalyManager anomalyManager;
    public Transform playerStartPoint;

    [Header("Floor settings")]
    public int startingFloor = 8;
    private int currentFloor;

    [Header("Timer")]
    public float elapsedTime = 0f;
    public bool timerRunning = false;

    public enum GameState { Playing, Won }
    public GameState State { get; private set; }

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        currentFloor = startingFloor;
        State = GameState.Playing;
        timerRunning = true;
        anomalyManager.SetupFloor();
        UpdateFloorUI();
    }

    void Update()
    {
        if (timerRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    public void PlayerMadeChoice(bool playerSaysAnomaly)
    {
        if (State != GameState.Playing) return;

        bool correct = playerSaysAnomaly == anomalyManager.CurrentFloorHasAnomaly;

        if (correct)
        {
            currentFloor--;
            UpdateFloorUI();

            if (currentFloor <= 0)
            {
                WinGame();
                return;
            }
        }
        else
        {
            // Wrong answer — send player all the way back to floor 8
            currentFloor = startingFloor;
            UpdateFloorUI();
        }

        NextLoop();
    }

    private void NextLoop()
    {
        if (playerStartPoint != null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                player.transform.position = playerStartPoint.position;
        }

        anomalyManager.SetupFloor();
    }

    private void WinGame()
    {
        State = GameState.Won;
        timerRunning = false;
        Debug.Log($"WIN!!! Time: {elapsedTime:F2}s");
        // trigger win UI here
    }

    private void UpdateFloorUI()
    {
        // floorText.text = currentFloor.ToString();
        Debug.Log($"Now on floor {currentFloor}");
    }

    private void UpdateTimerUI()
    {
        // int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        // float seconds = elapsedTime % 60f;
        // timerText.text = $"{minutes:00}:{seconds:00.00}";
    }
}