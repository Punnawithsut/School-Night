using UnityEngine;

public class LightBlackoutTrigger : MonoBehaviour
{
    private HorrorLightFlicker _flickerScript;
    private Light[] _hallwayLights;
    private FlashLight _flashLight;
    private bool _hasTriggered = false;

    private void Start()
    {
        _flickerScript = FindFirstObjectByType<HorrorLightFlicker>();
        _hallwayLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        _flashLight = FindFirstObjectByType<FlashLight>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            _hasTriggered = true;

            if (_flickerScript != null)
                _flickerScript.enabled = false;

            if (_flashLight != null)
                _flashLight.SetBlackoutActive(true);

            if (_hallwayLights != null)
            {
                foreach (Light lightSource in _hallwayLights)
                {
                    if (lightSource != null)
                        lightSource.enabled = false;
                }
            }
        }
    }

    // Automatically runs when this anomaly is destroyed or disabled on floor change
    private void OnDisable()
    {
        if (_flickerScript != null)
            _flickerScript.enabled = true;

        if (_flashLight != null)
            _flashLight.SetBlackoutActive(false);

        if (_hallwayLights != null)
        {
            foreach (Light lightSource in _hallwayLights)
            {
                if (lightSource != null)
                    lightSource.enabled = true;
            }
        }
    }
}