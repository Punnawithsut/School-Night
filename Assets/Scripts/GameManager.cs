using UnityEngine;
using TMPro;
using System.Collections;

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

    [Header("Floor Intro Animation")]
    public RectTransform floorIntroText;   // big centered text, e.g. "Floor 5"
    public TextMeshProUGUI floorIntroLabel;
    public Vector2 introCenterPos = Vector2.zero;
    public Vector2 introCornerPos = new Vector2(400f, -250f);
    public float introBigScale = 4f;
    public float introSmallScale = 1f;
    public float introDuration = 1.2f;
    public float introHoldTime = 0.3f;

    [Header("Timer UI")]
    public TextMeshProUGUI timerText;

    public enum GameState { Playing, Won, Transitioning }
    public GameState State { get; private set; }

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        currentFloor = startingFloor;
        anomalyManager.SetupFloor();
        PlayFloorIntro();
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
        PlayFloorIntro();
    }

    private void PlayFloorIntro()
    {
        StartCoroutine(FloorIntroRoutine());
    }

    private IEnumerator FloorIntroRoutine()
    {
        State = GameState.Transitioning;
        timerRunning = false;

        // hide timer during the reveal
        if (timerText != null)
            timerText.gameObject.SetActive(false);

        floorIntroLabel.text = $"Floor {currentFloor}";
        floorIntroText.anchoredPosition = introCenterPos;
        floorIntroText.localScale = Vector3.one * introBigScale;
        floorIntroText.gameObject.SetActive(true);

        yield return new WaitForSeconds(introHoldTime);

        float elapsed = 0f;
        Vector3 startScale = Vector3.one * introBigScale;
        Vector3 endScale = Vector3.one * introSmallScale;

        while (elapsed < introDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / introDuration);
            float eased = EaseInOutCubic(t);

            floorIntroText.anchoredPosition = Vector2.Lerp(introCenterPos, introCornerPos, eased);
            floorIntroText.localScale = Vector3.Lerp(startScale, endScale, eased);

            yield return null;
        }

        floorIntroText.anchoredPosition = introCornerPos;
        floorIntroText.localScale = endScale;

        // show timer once the tween settles
        if (timerText != null)
            timerText.gameObject.SetActive(true);

        State = GameState.Playing;
        timerRunning = true;
        UpdateFloorUI();
    }

    private float EaseInOutCubic(float t)
    {
        return t < 0.5f
            ? 4f * t * t * t
            : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
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
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        float seconds = elapsedTime % 60f;
        timerText.text = $"{minutes:00}:{seconds:00.00}";
    }
}