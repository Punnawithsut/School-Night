using UnityEngine;
using UnityEngine.InputSystem;

public class FpsMovement : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float walkSpeed = 7f;
    [SerializeField] private float runSpeed = 12f;
    [SerializeField] private float crouchSpeed = 4f;

    [Header("Jump and Fall")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = -12f;
    [SerializeField] private float initialFallVelocity = -2f;

    [Header("Crouch")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchingHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    [SerializeField] private float cameraOffset = 0.4f;

    [Header("Look Settings")]
    [SerializeField] private Vector2 lookSensitivity = new Vector2(1f, 1f);
    [SerializeField] private float minPitch = -89f;
    [SerializeField] private float maxPitch = 89f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private CameraShake cameraNoiseController;
    [SerializeField] private StaminaSystem staminaSystem;

    [Header("Footsteps")]
    [SerializeField] private AudioSource footstepAudio;

    [SerializeField] private float crouchPitch = 0.8f;
    [SerializeField] private float walkPitch = 1.0f;
    [SerializeField] private float sprintPitch = 1.25f;

    [SerializeField] private float footstepStartTime = 0.05f;

    private CharacterController _characterController;

    private Vector2 _moveInput;
    private Vector2 _lookInput;

    private float _cameraPitch;

    private bool _isGrounded;
    private bool _isRunning;
    private bool _isCrouching;

    private float _verticalVelocity;
    private float _targetHeight;

    private bool _cameraLocked;

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();

        _targetHeight = standingHeight;
    }

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.action.performed += StoreMovementInput;
            moveAction.action.canceled += StoreMovementInput;
        }

        if (lookAction != null)
        {
            lookAction.action.performed += StoreLookInput;
            lookAction.action.canceled += StoreLookInput;
        }

        if (jumpAction != null)
            jumpAction.action.performed += Jump;

        if (crouchAction != null)
            crouchAction.action.performed += Crouch;

        if (sprintAction != null)
        {
            sprintAction.action.performed += Sprint;
            sprintAction.action.canceled += Sprint;
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.action.performed -= StoreMovementInput;
            moveAction.action.canceled -= StoreMovementInput;
        }

        if (lookAction != null)
        {
            lookAction.action.performed -= StoreLookInput;
            lookAction.action.canceled -= StoreLookInput;
        }

        if (jumpAction != null)
            jumpAction.action.performed -= Jump;

        if (crouchAction != null)
            crouchAction.action.performed -= Crouch;

        if (sprintAction != null)
        {
            sprintAction.action.performed -= Sprint;
            sprintAction.action.canceled -= Sprint;
        }
    }

    private void Update()
    {
        _isGrounded = _characterController.isGrounded;

        HandleMouseLook();
        HandleGravity();
        HandleMovement();
        HandleCrouchTransition();
    }

    private void StoreMovementInput(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void StoreLookInput(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
    }

    private void HandleMouseLook()
    {
        if (_cameraLocked)
            return;

        float sens = SettingsManager.Instance != null
            ? SettingsManager.Instance.sensitivity
            : 1f;

        Vector2 inputDelta = _lookInput * sens;

        transform.Rotate(
            Vector3.up * inputDelta.x * lookSensitivity.x
        );

        _cameraPitch -= inputDelta.y * lookSensitivity.y;

        _cameraPitch = Mathf.Clamp(
            _cameraPitch,
            minPitch,
            maxPitch
        );

        if (cameraTransform != null)
        {
            Vector3 currentLocalEuler =
                cameraTransform.localEulerAngles;

            cameraTransform.localRotation =
                Quaternion.Euler(
                    _cameraPitch,
                    currentLocalEuler.y,
                    currentLocalEuler.z
                );
        }
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (_isGrounded)
        {
            _verticalVelocity = jumpForce;

            // Stop footsteps immediately when jumping
            StopFootsteps();
        }
    }

    private void Crouch(InputAction.CallbackContext context)
    {
        if (_isCrouching)
        {
            if (CanStandUp())
            {
                _targetHeight = standingHeight;
                _isCrouching = false;
            }
        }
        else
        {
            _targetHeight = crouchingHeight;
            _isCrouching = true;
        }
    }

    private bool CanStandUp()
    {
        Vector3 crouchHeadPosition =
            transform.position +
            (Vector3.up * crouchingHeight);

        float distanceToCeiling =
            standingHeight - crouchingHeight;

        return !Physics.SphereCast(
            crouchHeadPosition,
            _characterController.radius * 0.9f,
            Vector3.up,
            out RaycastHit hit,
            distanceToCeiling
        );
    }

    private void Sprint(InputAction.CallbackContext context)
    {
        _isRunning = context.performed;
    }

    private void HandleGravity()
    {
        if (_isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = initialFallVelocity;
        }

        _verticalVelocity += gravity * Time.deltaTime;
    }

    private void HandleMovement()
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection =
            (forward * _moveInput.y) +
            (right * _moveInput.x);

        bool isMoving =
            _moveInput.sqrMagnitude > 0.001f;

        // If there is no stamina system, sprinting is allowed.
        bool hasStaminaToRun =
            staminaSystem == null ||
            staminaSystem.HasStamina();

        bool isSprintingNow =
            _isRunning &&
            isMoving &&
            hasStaminaToRun &&
            !_isCrouching;

        float currentSpeed =
            _isCrouching
                ? crouchSpeed
                : (isSprintingNow ? runSpeed : walkSpeed);

        Vector3 finalMove =
            moveDirection * currentSpeed;

        finalMove.y = _verticalVelocity;

        // Move player
        CollisionFlags collisions =
            _characterController.Move(
                finalMove * Time.deltaTime
            );

        if ((collisions & CollisionFlags.Above) != 0)
        {
            _verticalVelocity = initialFallVelocity;
        }

        // Footsteps
        HandleFootsteps(
            isMoving,
            isSprintingNow
        );

        // Camera movement / shake
        bool isGroundedAndMoving =
            isMoving && _isGrounded;

        if (cameraNoiseController != null)
        {
            cameraNoiseController.SetMovementState(
                isGroundedAndMoving,
                isSprintingNow
            );
        }

        // Stamina
        if (staminaSystem != null)
        {
            if (isSprintingNow && _isGrounded)
            {
                staminaSystem.DrainStamina();
            }
            else
            {
                staminaSystem.RegenStamina();
            }
        }
    }

    private void HandleCrouchTransition()
    {
        float currentHeight =
            _characterController.height;

        if (Mathf.Abs(
                currentHeight - _targetHeight
            ) > 0.01f)
        {
            float newHeight =
                Mathf.Lerp(
                    currentHeight,
                    _targetHeight,
                    crouchTransitionSpeed *
                    Time.deltaTime
                );

            _characterController.height =
                newHeight;

            _characterController.center =
                Vector3.up *
                (newHeight * 0.5f);
        }
        else
        {
            _characterController.height =
                _targetHeight;

            _characterController.center =
                Vector3.up *
                (_targetHeight * 0.5f);
        }

        if (cameraTransform != null)
        {
            Vector3 localCamPos =
                cameraTransform.localPosition;

            localCamPos.y =
                _characterController.height -
                cameraOffset;

            cameraTransform.localPosition =
                localCamPos;
        }
    }

    private void HandleFootsteps(
        bool isMoving,
        bool isSprintingNow
    )
    {
        if (footstepAudio == null)
            return;

        // Stop footsteps while airborne or standing still
        if (!_isGrounded || !isMoving)
        {
            StopFootsteps();
            return;
        }

        // Change playback speed based on movement
        if (_isCrouching)
        {
            footstepAudio.pitch = crouchPitch;
        }
        else if (isSprintingNow)
        {
            footstepAudio.pitch = sprintPitch;
        }
        else
        {
            footstepAudio.pitch = walkPitch;
        }

        // Start the audio from 0.05 seconds
        if (!footstepAudio.isPlaying)
        {
            footstepAudio.time = footstepStartTime;
            footstepAudio.Play();
        }
    }

    private void StopFootsteps()
    {
        if (footstepAudio != null &&
            footstepAudio.isPlaying)
        {
            footstepAudio.Stop();
        }
    }

    public void LockCamera()
    {
        _cameraLocked = true;
    }

    public void UnlockCamera()
    {
        _cameraLocked = false;
    }
}