using UnityEngine;
using Unity.Cinemachine;
using System;
using UnityEditorInternal;

public class CameraShake : MonoBehaviour
{
    private CinemachineBasicMultiChannelPerlin _cameraNoise;

    [Header("Shake Profiles")]
    [SerializeField] private Vector2 idleSettings = new Vector2(0.2f, 1.0f);
    [SerializeField] private Vector2 walkSettings = new Vector2(0.5f, 2.0f);
    [SerializeField] private Vector2 runSettings = new Vector2(1.0f, 3.0f);
    [SerializeField] private float transitionSpeed = 8f;

    private float _targetAmplitude;
    private float _targetFrequency;

    void Start()
    {
        _cameraNoise = GetComponent<CinemachineBasicMultiChannelPerlin>();
        if (_cameraNoise == null)
        {
            Debug.Log("Please add CinemachineBasicMultiChannelPerlin");
        }
    }

    void Update()
    {
        if (_cameraNoise == null) return;

        _cameraNoise.AmplitudeGain = Mathf.Lerp(_cameraNoise.AmplitudeGain, _targetAmplitude, transitionSpeed * Time.deltaTime);
        _cameraNoise.FrequencyGain = Mathf.Lerp(_cameraNoise.FrequencyGain, _targetFrequency, transitionSpeed + Time.deltaTime);
    }

    public void SetMovementState(bool isMoving, bool isRunning)
    {
        if (!isMoving)
        {
            _targetAmplitude = idleSettings.x;
            _targetFrequency = idleSettings.y;
        }
        else if (isRunning)
        {
            _targetAmplitude = runSettings.x;
            _targetFrequency = runSettings.y;
        }
        else
        {
            _targetAmplitude = walkSettings.x;
            _targetFrequency = walkSettings.y;
        }
    }
}
