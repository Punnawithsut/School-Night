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
    private bool _blackoutActive;

    private void Start()
    {
        if (ON != null) ON.SetActive(false);
        if (OFF != null) OFF.SetActive(true);
        _isON = false;

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }

    public void SetBlackoutActive(bool active)
    {
        _blackoutActive = active;

        if (_blackoutActive && _isON)
        {
            if (ON != null) ON.SetActive(false);
            if (OFF != null) OFF.SetActive(true);
            _isON = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (_blackoutActive)
                return;

            if (_isON)
            {
                if (ON != null) ON.SetActive(false);
                if (OFF != null) OFF.SetActive(true);
                PlaySound(turnOffSound);
            }
            else
            {
                if (ON != null) ON.SetActive(true);
                if (OFF != null) OFF.SetActive(false);
                PlaySound(turnOnSound);
            }
            _isON = !_isON;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }
}