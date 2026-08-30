using UnityEngine;

public class FlashLight : MonoBehaviour
{
    [Header("Flashlight")]
    public GameObject ON;
    public GameObject OFF;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip turnOnSound;
    [SerializeField] private AudioClip turnOffSound;

    private bool _isON;

    private void Start()
    {
        ON.SetActive(false);
        OFF.SetActive(true);

        _isON = false;

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlashlight();
        }
    }

    private void ToggleFlashlight()
    {
        if (_isON)
        {
            // Turn OFF
            ON.SetActive(false);
            OFF.SetActive(true);

            PlaySound(turnOffSound);
        }
        else
        {
            // Turn ON
            ON.SetActive(true);
            OFF.SetActive(false);

            PlaySound(turnOnSound);
        }

        _isON = !_isON;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }
}