using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // ชื่อ Scene เกมที่จะโหลดเมื่อกด Play
    public string gameSceneName = "SampleScene";

    public void OnPlayPressed()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnSettingsPressed()
    {
        Debug.Log("เปิด Settings");
        // เปิด Settings Panel (ทำในขั้นตอนต่อไปได้)
    }

    public void OnQuitPressed()
    {
        Application.Quit();
        Debug.Log("ออกจากเกม"); // จะเห็นแค่ใน Editor
    }
}