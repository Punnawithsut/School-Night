using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // ชื่อ Scene เกมที่จะโหลดเมื่อกด Play
    public string gameSceneName = "5th_floor";

    public void OnPlayPressed()
    {
        SceneManager.LoadScene("Intro");
    }

    public void OnSettingsPressed()
    {
        Debug.Log("เปิด Settings");
        // เปิด Settings Panel (ทำในขั้นตอนต่อไปได้)
    }

    public void OnQuitPressed()
    {
        Application.Quit();
        Debug.Log("ออกจากเกม");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}