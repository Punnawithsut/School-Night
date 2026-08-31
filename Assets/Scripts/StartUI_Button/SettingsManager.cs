using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using Unity.Cinemachine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("UI")]
    public GameObject settingsPanel;
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Slider sensitivitySlider;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("References")]
    [SerializeField] private CinemachineInputAxisController cinemachineInputAxisController;

    [Header("Sensitivity")]
    public float sensitivity = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (cinemachineInputAxisController == null)
        {
            cinemachineInputAxisController =
                FindAnyObjectByType<CinemachineInputAxisController>();
        }

        // Load saved settings
        float bgmVolume =
            PlayerPrefs.GetFloat("BGMVolume", 1f);

        float sfxVolume =
            PlayerPrefs.GetFloat("SFXVolume", 1f);

        float savedSensitivity =
            PlayerPrefs.GetFloat("Sensitivity", 1f);

        bgmSlider.value = bgmVolume;
        sfxSlider.value = sfxVolume;
        sensitivitySlider.value = savedSensitivity;

        ApplySettings();

        // Connect sliders
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
        SetMixerVolume("BGMVolume", value);

        PlayerPrefs.SetFloat(
            "BGMVolume",
            value
        );
    }

    private void OnSFXChanged(float value)
    {
        SetMixerVolume("SFXVolume", value);

        PlayerPrefs.SetFloat(
            "SFXVolume",
            value
        );
    }

    private void SetMixerVolume(
        string parameter,
        float value
    )
    {
        if (audioMixer == null)
            return;

        // Convert 0-1 slider value to decibels
        float volume =
            Mathf.Log10(
                Mathf.Max(value, 0.0001f)
            ) * 20f;

        audioMixer.SetFloat(
            parameter,
            volume
        );
    }

    private void OnSensitivityChanged(float value)
    {
        if (value <= 0f)
            value = 0.1f;

        sensitivity = value;

        ApplyCinemachineSensitivity();

        PlayerPrefs.SetFloat(
            "Sensitivity",
            value
        );
    }

    private void ApplySettings()
    {
        SetMixerVolume(
            "BGMVolume",
            bgmSlider.value
        );

        SetMixerVolume(
            "SFXVolume",
            sfxSlider.value
        );

        sensitivity =
            sensitivitySlider.value;

        ApplyCinemachineSensitivity();
    }

    private void ApplyCinemachineSensitivity()
    {
        if (cinemachineInputAxisController == null)
            return;

        foreach (
            var controller
            in cinemachineInputAxisController.Controllers
        )
        {
            float direction =
                Mathf.Sign(
                    controller.Input.Gain
                );

            controller.Input.Gain =
                direction == 0f
                    ? sensitivity
                    : direction * sensitivity;
        }
    }
}