using UnityEngine;

public class FlashLight : MonoBehaviour
{
    public GameObject ON;
    public GameObject OFF;
    private bool _isON;
    private bool _blackoutActive;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (ON != null) ON.SetActive(false);
        if (OFF != null) OFF.SetActive(true);
        _isON = false;
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
            }
            else
            {
                if (ON != null) ON.SetActive(true);
                if (OFF != null) OFF.SetActive(false);
            }
            _isON = !_isON;
        }
    }
}
