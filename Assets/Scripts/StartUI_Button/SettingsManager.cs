using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using Unity.Cinemachine;
using System;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("UI")]
    public GameObject settingsPanel;
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Slider sensitivitySlider;

    [Header("References")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public CinemachineInputAxisController cinemachineInputAxisController;

    [Header("Sensitivity")]
    public float sensitivity = 1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        if (cinemachineInputAxisController == null)
        {
            cinemachineInputAxisController = FindAnyObjectByType<CinemachineInputAxisController>();
        }

        // โหลดค่าที่เคยตั้งไว้
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 1f);

        ApplySettings();

        // เชื่อม Slider กับ function
        bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        PlayerPrefs.Save();
    }

    private void OnBGMChanged(float value)
    {
        if (bgmSource != null) bgmSource.volume = value;
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    private void OnSFXChanged(float value)
    {
        if (sfxSource != null) sfxSource.volume = value;
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    private void OnSensitivityChanged(float value)
    {
        if(value <= 0f) value = 0.1f;
        sensitivity = value;
        ApplyCinemachineSensitivity();
        PlayerPrefs.SetFloat("Sensitivity", value);
    }

    private void ApplySettings()
    {
        if (bgmSource != null) bgmSource.volume = bgmSlider.value;
        if (sfxSource != null) sfxSource.volume = sfxSlider.value;
        sensitivity = sensitivitySlider.value;
        ApplyCinemachineSensitivity();
    }

    private void ApplyCinemachineSensitivity()
    {
        if (cinemachineInputAxisController == null) return;

        foreach (var controller in cinemachineInputAxisController.Controllers)
        {
            float direction = Mathf.Sign(controller.Input.Gain);
            Console.WriteLine(direction);
            controller.Input.Gain = direction == 0f ? sensitivity : direction * sensitivity;
        }
    }
}