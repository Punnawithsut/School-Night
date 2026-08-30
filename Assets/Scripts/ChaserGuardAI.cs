using UnityEngine;
using UnityEngine.AI;

public class ChaserGuardAI : MonoBehaviour
{
    [Header("Waypoints & Targets")]
    public Transform spawnPoint;
    public float runSpeed = 5.5f;

    [Header("Jumpscare")]
    [SerializeField] private float jumpscareDelay = 2f;

    [Header("Jumpscare Camera")]
    [SerializeField] private JumpscareCamera jumpscareCamera;

    private Transform _player;
    private NavMeshAgent _agent;
    private Animator _animator;
    private AudioSource _audioSource;

    private bool _isCaught = false;
    private float _spawnTime;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        _isCaught = false;
        _spawnTime = Time.time;

        // Cancel any previous reset timer
        CancelInvoke(nameof(ResetPlayerToFloor8));

        if (spawnPoint == null)
        {
            GameObject sp = GameObject.Find("GuardSpawnPoint");

            if (sp != null)
            {
                spawnPoint = sp.transform;
            }
        }

        if (_agent != null && spawnPoint != null)
        {
            _agent.enabled = false;

            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;

            _agent.enabled = true;

            if (NavMesh.SamplePosition(
                spawnPoint.position,
                out NavMeshHit hit,
                3.0f,
                NavMesh.AllAreas))
            {
                _agent.Warp(hit.position);
            }
        }
    }

    private void Start()
    {
        GameObject playerObj =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            _player = playerObj.transform;
        }

        if (_agent != null)
        {
            _agent.speed = runSpeed;
            _agent.autoBraking = false;
        }

        // Automatically find JumpscareCamera if not assigned
        if (jumpscareCamera == null)
        {
            jumpscareCamera =
                FindAnyObjectByType<JumpscareCamera>();
        }
    }

    private void Update()
    {
        if (_agent == null ||
            !_agent.isOnNavMesh ||
            _isCaught ||
            _player == null)
        {
            return;
        }

        // Stop guard when game isn't playing
        if (GameManager.Instance != null &&
            GameManager.Instance.State !=
            GameManager.GameState.Playing)
        {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;

            if (_animator != null)
            {
                _animator.SetFloat(
                    "Speed",
                    0f,
                    0.1f,
                    Time.deltaTime
                );
            }

            return;
        }

        // Chase player
        _agent.isStopped = false;
        _agent.SetDestination(_player.position);

        // Update running animation
        if (_animator != null)
        {
            float currentSpeed =
                _agent.velocity.magnitude;

            _animator.SetFloat(
                "Speed",
                currentSpeed,
                0.1f,
                Time.deltaTime
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Don't catch immediately after spawning
        if (Time.time - _spawnTime < 1.5f)
        {
            return;
        }

        if (_isCaught)
        {
            return;
        }

        // Don't catch player if game isn't playing
        if (GameManager.Instance != null &&
            GameManager.Instance.State !=
            GameManager.GameState.Playing)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            _isCaught = true;

            // Stop guard
            if (_agent != null &&
                _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero;
                _agent.ResetPath();
            }

            // Stop running animation
            if (_animator != null)
            {
                _animator.SetFloat(
                    "Speed",
                    0f
                );
            }

            // Play jumpscare sound
            if (_audioSource != null &&
                _audioSource.clip != null)
            {
                _audioSource.PlayOneShot(
                    _audioSource.clip
                );
            }

            // Start camera jumpscare
            if (jumpscareCamera != null)
            {
                jumpscareCamera.StartJumpscare(transform);
            }

            // Wait before resetting the player
            if (GameManager.Instance != null)
            {
                Invoke(
                    nameof(ResetPlayerToFloor8),
                    jumpscareDelay
                );
            }
        }
    }

    private void ResetPlayerToFloor8()
    {
        // Stop camera jumpscare before resetting
        if (jumpscareCamera != null)
        {
            jumpscareCamera.StopJumpscare();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetToFloor8();
        }
    }
}