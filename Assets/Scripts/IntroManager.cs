using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [Header("UI")]
    public RectTransform storyText;
    public Button skipButton;

    [Header("Settings")]
    public float scrollSpeed = 60f;
    public float startDelay = 1f;
    public string nextScene = "5th_floor";

    private bool _isScrolling = false;
    private float _startY;
    private float _endY;

    private void Start()
    {
        if (skipButton != null)
            skipButton.onClick.AddListener(SkipIntro);

        StartCoroutine(BeginScroll());
    }

    private IEnumerator BeginScroll()
    {
        // ซ่อนก่อน
        storyText.gameObject.SetActive(false);

        yield return new WaitForEndOfFrame();

        _startY = -Screen.height / 8f - storyText.rect.height / 8f;
        _endY = Screen.height + storyText.rect.height ;

        storyText.anchoredPosition = new Vector2(0, _startY);

        // แสดงหลัง position ถูกเซ็ตแล้ว
        storyText.gameObject.SetActive(true);

        yield return new WaitForSeconds(startDelay);

        _isScrolling = true;
    }

    private void Update()
    {
        if (!_isScrolling) return;

        Vector2 pos = storyText.anchoredPosition;
        pos.y += scrollSpeed * Time.deltaTime;
        storyText.anchoredPosition = pos;

        if (pos.y >= _endY)
        {
            _isScrolling = false;
            LoadNextScene();
        }
    }

    private void SkipIntro()
    {
        _isScrolling = false;
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextScene);
    }
}