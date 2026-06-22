using UnityEngine;
using UnityEngine.UI;

public class StaminaSystem : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100.0f;
    [SerializeField] private float drainRate = 10.0f;
    [SerializeField] private float regenRate = 5.0f;
    [SerializeField] private float regenDelay = 2.0f;

    [Header("UI Settings")]
    [SerializeField] private Image StaminaBar;

    private float _currentStamina;
    private float _delayTimer;

    public bool HasStamina()
    {
        return _currentStamina > 0;
    }

    void Start()
    {
        _currentStamina = maxStamina;
    }

    void Update()
    {
        if (StaminaBar != null)
        {
            StaminaBar.fillAmount = _currentStamina / maxStamina;
        }
    }

    public void DrainStamina()
    {
        _currentStamina -= drainRate * Time.deltaTime;
        _currentStamina = Mathf.Clamp(_currentStamina, 0.0f, maxStamina);
        _delayTimer = regenDelay;
    }

    public void RegenStamina()
    {
        if (_delayTimer > 0f)
        {
            _delayTimer -= Time.deltaTime;
            return;
        }

        _currentStamina += regenRate * Time.deltaTime;
        _currentStamina = Mathf.Clamp(_currentStamina, 0.0f, maxStamina);
    }
}
