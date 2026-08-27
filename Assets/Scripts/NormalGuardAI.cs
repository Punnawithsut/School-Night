using UnityEngine;
using UnityEngine.AI;

public class NormalGuardAI : MonoBehaviour
{
    public Transform spawnPoint;
    public Transform elevatorFrontPoint;

    [Header("Movement Settings")]
    public float walkSpeed = 2.0f;

    [Header("Animation Settings")]
    [Tooltip("Fixed Animator parameter value to guarantee Walk animation plays")]
    public float walkAnimationValue = 2.0f;

    private NavMeshAgent _agent;
    private Animator _animator;
    private bool _hasReached = false;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        _hasReached = false;
        if (_agent == null) return;

        _agent.enabled = false;
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }
        _agent.enabled = true;

        if (_agent.isOnNavMesh)
        {
            _agent.isStopped = false;
            _agent.speed = walkSpeed;
            if (elevatorFrontPoint != null)
            {
                _agent.SetDestination(elevatorFrontPoint.position);
            }
        }
    }

    private void Update()
    {
        if (_agent == null || !_agent.isOnNavMesh) return;

        if (!_hasReached && elevatorFrontPoint != null)
        {
            Vector3 currentPos = transform.position;
            Vector3 targetPos = elevatorFrontPoint.position;
            currentPos.y = 0;
            targetPos.y = 0;

            if (Vector3.Distance(currentPos, targetPos) <= _agent.stoppingDistance)
            {
                _hasReached = true;
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero;
                _agent.ResetPath();
            }
        }

        if (_animator != null)
        {
            // Forces exact walk animation value while walking, then 0 when arrived
            float speedValue = _hasReached ? 0f : walkAnimationValue;
            _animator.SetFloat("Speed", speedValue);
        }
    }
}