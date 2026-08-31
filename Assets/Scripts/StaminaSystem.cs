using UnityEngine;
using UnityEngine.UI;

public class StaminaSystem : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float drainRate = 20f;
    [SerializeField] private float regenRate = 5f;
    [SerializeField] private float regenDelay = 2f;

    [Header("UI Settings")]
    [SerializeField] private Image StaminaBar;

    [Header("Exhaustion Sound")]
    [SerializeField] private AudioSource exhaustAudio;

    // Start exhaustion when stamina reaches this percentage.
    // 0.25 = 25%
    [SerializeField, Range(0f, 1f)]
    private float lowStaminaThreshold = 0.25f;

    // Small pause between breathing clips.
    [SerializeField] private float exhaustSoundDelay = 0.5f;

    private float _currentStamina;
    private float _delayTimer;

    private float _exhaustTimer;
    private bool _wasLowStamina;

    private void Start()
    {
        _currentStamina = maxStamina;

        _delayTimer = 0f;
        _exhaustTimer = 0f;
        _wasLowStamina = false;

        if (exhaustAudio != null)
        {
            // We control the replay ourselves.
            exhaustAudio.loop = false;

            // Don't automatically play when the game starts.
            exhaustAudio.playOnAwake = false;

            exhaustAudio.Stop();
        }
    }

    private void Update()
    {
        UpdateStaminaBar();
        HandleExhaustSound();
    }

    // Used by FpsMovement
    public bool HasStamina()
    {
        return _currentStamina > 0f;
    }

    public void DrainStamina()
    {
        _currentStamina -=
            drainRate * Time.deltaTime;

        _currentStamina = Mathf.Clamp(
            _currentStamina,
            0f,
            maxStamina
        );

        // Reset regeneration delay
        _delayTimer = regenDelay;
    }

    public void RegenStamina()
    {
        if (_delayTimer > 0f)
        {
            _delayTimer -= Time.deltaTime;
            return;
        }

        _currentStamina +=
            regenRate * Time.deltaTime;

        _currentStamina = Mathf.Clamp(
            _currentStamina,
            0f,
            maxStamina
        );
    }

    private void UpdateStaminaBar()
    {
        if (StaminaBar == null)
            return;

        StaminaBar.fillAmount =
            _currentStamina / maxStamina;
    }

    private void HandleExhaustSound()
    {
        if (exhaustAudio == null ||
            exhaustAudio.clip == null)
        {
            return;
        }

        float staminaPercent =
            _currentStamina / maxStamina;

        bool isLowStamina =
            staminaPercent <= lowStaminaThreshold;

        // =====================================
        // STAMINA IS LOW
        // =====================================
        if (isLowStamina)
        {
            // Player has just entered low stamina
            if (!_wasLowStamina)
            {
                _wasLowStamina = true;

                // Allow the first breath immediately
                _exhaustTimer = 0f;
            }

            // Count down the delay
            if (_exhaustTimer > 0f)
            {
                _exhaustTimer -= Time.deltaTime;
            }

            // Only start a new breath when:
            // - previous sound has completely finished
            // - delay has finished
            if (!exhaustAudio.isPlaying &&
                _exhaustTimer <= 0f)
            {
                exhaustAudio.Play();

                // Wait for the entire clip + extra delay
                // before allowing another breath.
                _exhaustTimer =
                    exhaustAudio.clip.length +
                    exhaustSoundDelay;
            }
        }

        // =====================================
        // STAMINA HAS RECOVERED
        // =====================================
        else
        {
            _wasLowStamina = false;

            // IMPORTANT:
            // Don't call Stop() here.
            //
            // If the breathing sound is currently
            // fading out, allow it to finish naturally.

            _exhaustTimer = 0f;
        }
    }
}