using UnityEngine;
using UnityEngine.AI;

public class NormalGuardAI : MonoBehaviour
{
    public Transform spawnPoint;
    public Transform elevatorFrontPoint;

    [Header("Movement Settings")]
    public float walkSpeed = 3.0f;

    private NavMeshAgent _agent;
    private Animator _animator;
    private bool _hasReached = false;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    public void ResetGuard()
    {
        _hasReached = false;
        if (_agent == null) _agent = GetComponent<NavMeshAgent>();
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
            _agent.Warp(spawnPoint.position);
            _agent.isStopped = false;
            _agent.speed = walkSpeed;
            if (elevatorFrontPoint != null)
            {
                _agent.SetDestination(elevatorFrontPoint.position);
            }
        }
    }

    private void OnEnable()
    {
        ResetGuard();
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
            float currentSpeed = _agent.velocity.magnitude;
            _animator.SetFloat("Speed", currentSpeed, 0.1f, Time.deltaTime);
        }
    }
}