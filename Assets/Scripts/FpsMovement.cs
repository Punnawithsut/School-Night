using UnityEngine;
using UnityEngine.InputSystem;

public class FpsMovement : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2f;

    [Header("Jump and Fall")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = -12f;
    [SerializeField] private float initialFallVelocity = -2f;

    [Header("Crouch")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchingHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    [SerializeField] private float cameraOffset = 0.4f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private CameraShake cameraNoiseController; 
    [SerializeField] private StaminaSystem staminaSystem;

    private CharacterController _characterController;
    private Vector2 _moveInput;
    private bool _isGrounded;
    private bool _isRunning;
    private bool _isCrouching;
    private float _verticalVelocity;
    private float _targetHeight;

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _targetHeight = standingHeight;
    }

    private void OnEnable()
    {
        moveAction.action.performed += StoreMovementInput;
        moveAction.action.canceled += StoreMovementInput;
        jumpAction.action.performed += Jump;
        crouchAction.action.performed += Crouch;
        sprintAction.action.performed += Sprint;
        sprintAction.action.canceled += Sprint;
    }

    private void OnDisable()
    {
        moveAction.action.performed -= StoreMovementInput;
        moveAction.action.canceled -= StoreMovementInput;
        jumpAction.action.performed -= Jump;
        crouchAction.action.performed -= Crouch;
        sprintAction.action.performed -= Sprint;
        sprintAction.action.canceled -= Sprint;
    }

    private void Update()
    {
        _isGrounded = _characterController.isGrounded;
        HandleGravity();
        HandleMovement();
        HandleCrouchTransition();
    }

    private void StoreMovementInput(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (_isGrounded)
        {
            _verticalVelocity = jumpForce;
        }
    }

    private void Crouch(InputAction.CallbackContext context)
    {
        //Debug.Log($"Crouch Triggered");
        if(_isCrouching)
        {
            if(CanStandUp())
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
        Vector3 crouchHeadPosition = transform.position + (Vector3.up * crouchingHeight);
        float distanceToCeiling = standingHeight - crouchingHeight;

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
        if(_isGrounded && _verticalVelocity < 0)
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

        Vector3 moveDirection = (forward * _moveInput.y) + (right * _moveInput.x);

        bool isMoving = _moveInput.sqrMagnitude > 0.001f;
        bool hasStaminaToRun = staminaSystem != null && staminaSystem.HasStamina();
        bool isSprintingNow = _isRunning && isMoving && hasStaminaToRun;

        var currentSpeed = _isCrouching ? crouchSpeed : (isSprintingNow ? runSpeed : walkSpeed);
        var finalMove = moveDirection * currentSpeed;

        finalMove.y = _verticalVelocity;

        var collisions = _characterController.Move(finalMove * Time.deltaTime);
        if ((collisions & CollisionFlags.Above) != 0)
        {
            _verticalVelocity = initialFallVelocity;
        }

        //camera shaking script
        bool isGroundedAndMoving =  isMoving && _isGrounded;
        if(cameraNoiseController != null)
        {
            cameraNoiseController.SetMovementState(isGroundedAndMoving, _isRunning);
        }

        //handle stamina system
        if(staminaSystem != null)
        {
            if(currentSpeed == runSpeed && isMoving && _isGrounded)
            {
                staminaSystem.DrainStamina();
            }
            else
            {
                staminaSystem.RegenStamina();
            }
        }
        //Debug.Log(isSprintingNow);
    }

    private void HandleCrouchTransition()
    {
        var currentHeight = _characterController.height;
        if (Mathf.Abs(currentHeight - _targetHeight) > 0.01f)
        {
            var newHeight = Mathf.Lerp(currentHeight, _targetHeight, crouchTransitionSpeed * Time.deltaTime);
            _characterController.height = newHeight;
            _characterController.center = Vector3.up * (newHeight * 0.5f);
        }
        else
        {
            //already close to smooth no lerping needed
            _characterController.height = _targetHeight;
            _characterController.center = Vector3.up * (_targetHeight * 0.5f);
        }

        Vector3 localCamPos = cameraTransform.localPosition;
        localCamPos.y = _characterController.height - cameraOffset;
        cameraTransform.localPosition = localCamPos;
    }
}
