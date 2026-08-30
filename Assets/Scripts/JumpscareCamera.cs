using UnityEngine;

public class JumpscareCamera : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("Player")]
    [SerializeField] private FpsMovement fpsMovement;

    [Header("Jumpscare")]
    [SerializeField] private float rotateSpeed = 5f;
    [SerializeField] private float lookHeight = 1.2f;

    private bool _isJumpscare;
    private Transform _guard;

    private void Start()
    {
        if (cameraTransform == null)
        {
            Camera cam = Camera.main;

            if (cam != null)
            {
                cameraTransform = cam.transform;
            }
        }

        if (fpsMovement == null)
        {
            fpsMovement = GetComponentInParent<FpsMovement>();

            if (fpsMovement == null)
            {
                fpsMovement = FindAnyObjectByType<FpsMovement>();
            }
        }
    }

    private void LateUpdate()
    {
        if (!_isJumpscare)
            return;

        if (_guard == null || cameraTransform == null)
            return;

        Vector3 targetPosition =
            _guard.position +
            Vector3.up * lookHeight;

        Vector3 direction =
            targetPosition -
            cameraTransform.position;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction.normalized);

        cameraTransform.rotation =
            Quaternion.Slerp(
                cameraTransform.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            );
    }

    public void StartJumpscare(Transform guard)
    {
        if (guard == null)
            return;

        _guard = guard;
        _isJumpscare = true;

        if (fpsMovement != null)
        {
            fpsMovement.LockCamera();
        }
    }

    public void StopJumpscare()
    {
        _isJumpscare = false;
        _guard = null;

        if (fpsMovement != null)
        {
            fpsMovement.UnlockCamera();
        }
    }
}