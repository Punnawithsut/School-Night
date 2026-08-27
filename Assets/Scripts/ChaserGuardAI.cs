using UnityEngine;
using UnityEngine.AI;

public class ChaserGuardAI : MonoBehaviour
{
    [Header("Waypoints & Targets")]
    public Transform spawnPoint;
    public float runSpeed = 5.5f;
    public float runAnimationValue = 5.5f;

    private Transform _player;
    private NavMeshAgent _agent;
    private Animator _animator;
    private bool _isCaught = false;
    private float _spawnTime;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        _isCaught = false;
        _spawnTime = Time.time;

        // 1. Auto-find GuardSpawnPoint in Scene if not assigned in Inspector
        if (spawnPoint == null)
        {
            GameObject sp = GameObject.Find("GuardSpawnPoint");
            if (sp != null) spawnPoint = sp.transform;
        }

        // 2. Force agent to spawn point before NavMesh activates
        if (_agent != null && spawnPoint != null)
        {
            _agent.enabled = false;
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
            _agent.enabled = true;

            if (NavMesh.SamplePosition(spawnPoint.position, out NavMeshHit hit, 3.0f, NavMesh.AllAreas))
            {
                _agent.Warp(hit.position);
            }
        }
    }

    private void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;

        if (_agent != null)
        {
            _agent.speed = runSpeed;
            _agent.autoBraking = false;
        }
    }

    private void Update()
    {
        if (_agent == null || !_agent.isOnNavMesh || _isCaught || _player == null) return;

        // Freeze during screen fades & floor intro animation
        if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing)
        {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;

            if (_animator != null)
            {
                _animator.SetFloat("Speed", 0f);
            }
            return;
        }

        // Active Chase
        _agent.isStopped = false;
        _agent.SetDestination(_player.position);

        if (_animator != null)
        {
            _animator.SetFloat("Speed", runAnimationValue);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1.5-second grace period after spawning to prevent instant catches during floor loading
        if (Time.time - _spawnTime < 1.5f) return;
        if (_isCaught) return;
        if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing) return;

        if (other.CompareTag("Player"))
        {
            _isCaught = true;
            if (_agent != null && _agent.isOnNavMesh) _agent.ResetPath();

            if (_animator != null)
            {
                _animator.SetFloat("Speed", 0f);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetToFloor8();
            }
        }
    }
}