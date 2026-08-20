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

    // Exposed so AnomalyManager (and anything else) can read the authoritative floor value.
    public int CurrentFloor => currentFloor;

    [Header("Timer")]
    public float elapsedTime = 0f;
    public bool timerRunning = false;

    [Header("Floor Intro Animation")]
    public RectTransform floorIntroText;
    public TextMeshProUGUI floorIntroLabel;
    public Vector2 introCenterPos = Vector2.zero;
    public Vector2 introCornerPos = new Vector2(400f, -250f);
    public float introBigScale = 4f;
    public float introSmallScale = 1f;
    public float introDuration = 1.2f;
    public float introHoldTime = 0.3f;

    [Header("Timer UI")]
    public TextMeshProUGUI timerText;

    [Header("Transition")]
    public CanvasGroup fadeCanvas;

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
        if (fadeCanvas != null) fadeCanvas.alpha = 0f;
        TeleportPlayer();
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

    private void TeleportPlayer()
    {
        if (playerStartPoint == null) return;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.position = playerStartPoint.position;
        player.transform.rotation = Quaternion.Euler(0, playerStartPoint.rotation.eulerAngles.y, 0);

        var cam = player.GetComponentInChildren<Camera>();
        if (cam != null)
            cam.transform.localRotation = Quaternion.Euler(playerStartPoint.rotation.eulerAngles.x, 0, 0);

        if (cc != null) cc.enabled = true;
    }

    public void PlayerMadeChoice(bool playerSaysAnomaly)
    {
        if (State != GameState.Playing) return;
        StartCoroutine(ChoiceRoutine(playerSaysAnomaly));
    }

    private IEnumerator ChoiceRoutine(bool playerSaysAnomaly)
    {
        State = GameState.Transitioning;
        timerRunning = false;

        yield return StartCoroutine(Fade(0f, 1f, 0.5f));
        yield return new WaitForSeconds(0.3f);

        bool correct = playerSaysAnomaly == anomalyManager.CurrentFloorHasAnomaly;
        if (correct)
        {
            currentFloor--;
            if (currentFloor <= 0)
            {
                WinGame();
                yield break;
            }
        }
        else
        {
            currentFloor = startingFloor;
        }

        NextLoop();

        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(Fade(1f, 0f, 0.5f));
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeCanvas == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        fadeCanvas.alpha = to;
    }

    private void NextLoop()
    {
        TeleportPlayer();
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
        if (fadeCanvas != null) fadeCanvas.alpha = 0f;
        Debug.Log($"WIN!!! Time: {elapsedTime:F2}s");
        // trigger win UI here
    }

    private void UpdateFloorUI()
    {
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