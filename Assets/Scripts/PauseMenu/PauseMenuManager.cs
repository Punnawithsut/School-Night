using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    private bool isPaused = false;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        CursorController.IsPaused = true; 
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        CursorController.IsPaused = false; 
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnQuitPressed()
    {
        Time.timeScale = 1f;
        CursorController.IsPaused = false;
        SceneManager.LoadScene("startUI");
    }
}