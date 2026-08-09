using UnityEngine;

public class FlashLight : MonoBehaviour
{
    public GameObject ON;
    public GameObject OFF;
    private bool _isON;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ON.SetActive(false);
        OFF.SetActive(true);
        _isON = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            if(_isON)
            {
                ON.SetActive(false);
                OFF.SetActive(true);
            } else
            {
                ON.SetActive(true);
                OFF.SetActive(false);
            }
            _isON = !_isON;
        }
    }
}
