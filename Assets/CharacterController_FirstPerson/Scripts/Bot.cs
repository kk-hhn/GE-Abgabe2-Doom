using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class Bot : MonoBehaviour
{
    public Transform playerTarget;
    public float detectionRadius = 25f;
    public float updateRate = 0.2f;       
    [Header("Combat & Distance Parameters")]
    public float attackRange = 15f;
    public float stopDistance = 3f;

    [Header("Shooting Settings")]
    public GameObject botBallPrefab;
    public Transform shootPoint;
    public float shootForce = 15f;
    public float fireRate = 1.5f;
    public float rotationSpeed = 5f;

    [Header("Inverse Kinematics Settings")]
    public bool useIK = true;
    public float ikWeightSpeed = 5f;      // gegen ruckeln
    public Vector3 ikTargetOffset = new Vector3(0f, 1.2f, 0f); // dass er auf brust/kopfhöhe zielt

    private NavMeshAgent _agent;
    private Animator _animator;
    private float _nextUpdateTimer;
    private float _nextShootTimer;
    private float _currentIKWeight = 0f;  // Blending für IK


    public bool IsMoving { get; private set; }
    public bool CanSeePlayer { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool IsPlayerDetected { get; private set; } // nur true wenn spieler in "hör"reichweite

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (playerTarget == null)
        {
            FP_CharacterController player = FindFirstObjectByType<FP_CharacterController>();
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        if (_agent != null)
        {
            _agent.stoppingDistance = stopDistance;
        }
    }

    void Update()
    {
        if (playerTarget == null) return;

        if (Time.time >= _nextUpdateTimer)
        {
            _nextUpdateTimer = Time.time + updateRate;
            UpdateBotStateAndDestination();
        }

        if (CanSeePlayer)
        {
            // zum spieler drehen
            Vector3 direction = (playerTarget.position - transform.position).normalized;
            direction.y = 0f;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            _agent.isStopped = true;
            IsAttacking = true;

            if (Time.time >= _nextShootTimer)
            {
                _nextShootTimer = Time.time + fireRate;
                ShootAtPlayer();
            }
        }
        else
        {
            IsAttacking = false;

            if (IsPlayerDetected)
            {
                _agent.isStopped = false;
            }
            else
            {
                _agent.isStopped = true;
            }
        }

        IsMoving = _agent.velocity.sqrMagnitude > 0.1f && !_agent.isStopped;

        if (_animator != null)
        {
            float currentNormalizedSpeed = (_agent.isStopped) ? 0f : _agent.velocity.magnitude / _agent.speed;
            _animator.SetFloat("Speed", currentNormalizedSpeed, 0.1f, Time.deltaTime);
            _animator.SetBool("IsAttacking", IsAttacking);
        }
    }

    // IK callback
    void OnAnimatorIK(int layerIndex)
    {
        if (_animator == null) return;

        float targetWeight = (CanSeePlayer && useIK && playerTarget != null) ? 1f : 0f;
        _currentIKWeight = Mathf.Lerp(_currentIKWeight, targetWeight, Time.deltaTime * ikWeightSpeed);

        if (_currentIKWeight > 0.001f && playerTarget != null)
        {
            Vector3 targetPosition = playerTarget.position + ikTargetOffset;

            //Blickrichtung
            _animator.SetLookAtWeight(_currentIKWeight, 0.3f, 0.8f, 1.0f);
            _animator.SetLookAtPosition(targetPosition);

            // arm
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, _currentIKWeight * 0.7f);
            _animator.SetIKPosition(AvatarIKGoal.RightHand, targetPosition);
        }
        else
        {
            _animator.SetLookAtWeight(0f);
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
        }
    }

    private void UpdateBotStateAndDestination()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer <= detectionRadius)
        {
            IsPlayerDetected = true;
            _agent.SetDestination(playerTarget.position);

            if (distanceToPlayer <= attackRange)
            {
                CanSeePlayer = CheckLineOfSight();
            }
            else
            {
                CanSeePlayer = false;
            }
        }
        else
        {
            // spieler ist zu weit entfernt, idle state
            IsPlayerDetected = false;
            CanSeePlayer = false;
            
            if (_agent.hasPath)
            {
                _agent.ResetPath();
            }
        }
    }

    private void ShootAtPlayer()
    {
        if (botBallPrefab == null) return;

        Vector3 spawnPos = shootPoint.position;
        GameObject projectile = Instantiate(botBallPrefab, spawnPos, Quaternion.identity);

        if (projectile.TryGetComponent<Ball_Behaviour>(out Ball_Behaviour ballScript))
        {
            ballScript.isPlayerBullet = false;
        }

        Vector3 targetPos = playerTarget.position + Vector3.up * 1f;
        Vector3 shootDirection = (targetPos - spawnPos).normalized;

        if (projectile.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.AddForce(shootDirection * shootForce, ForceMode.Impulse);
        }
    }

    private bool CheckLineOfSight()
    {
        Vector3 eyePosition = transform.position + Vector3.up * 1.5f;
        Vector3 directionToPlayer = (playerTarget.position - eyePosition).normalized;

        if (Physics.Raycast(eyePosition, directionToPlayer, out RaycastHit hit, attackRange))
        {
            if (hit.transform == playerTarget)
            {
                return true;
            }
        }
        return false;
    }

    //alle gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}