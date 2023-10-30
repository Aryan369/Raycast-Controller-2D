using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private Coroutine attackCoroutine; // Coroutine for delaying attacks
    [SerializeField] float meleeAttackDelay = 1f;
    [SerializeField] float rangedAttackDelay = 2f;

    public float detectionRange = 10f;
    public float attackRange = 1f;
    public float chaseSpeed = 6f;
    public float patrolSpeed = 2f;

    public float idleToPatrolTimeThreshold = 5f;
    public float patrolToIdleTimeThreshold = 10f;
    private float extraPatrolTimeAfterChase = 10f;

    public float patrolWaitTimeBeforeNextPatrolPoint = 2f;
    private float patrolWaitTimeBeforeNextPatrolPointRemaining;

    public Vector3[] patrolPoints;

    private int currentPatrolPointIndex;

    private Transform _transform;
    private NavMeshAgent _agent;
    private Transform player;
    private Vector3 lastKnownPlayerPosition;
    private float lastTimePlayerDetected;
    private bool playerDetected;

    private enum AIState
    {
        Idle,
        Patrol,
        Chase,
        Attack
    }
    [SerializeField] private AIState currentState;

    public LayerMask collisionMask;

    private enum EnemyTypes
    {
        Melee,
        Shooter
    }
    [SerializeField] private EnemyTypes enemyType;
    private Gun gun;

    void Awake()
    {
        _transform = transform;
        _agent = GetComponent<NavMeshAgent>();

        if(enemyType == EnemyTypes.Shooter)
        {
            gun = GetComponent<Gun>();
        }

        _agent.updateRotation = false;
        _agent.updateUpAxis = false;

        if(enemyType == EnemyTypes.Shooter)
        {
            _agent.stoppingDistance = attackRange - 9f;
        }
        else
        {
            _agent.stoppingDistance = attackRange;
        }
        currentState = AIState.Idle;
    }

    void Start()
    {
        player = Player.Instance.transform;
    }

    void Update()
    {
        switch (currentState)
        {
            case AIState.Idle:
                Idle();
                break;
            case AIState.Patrol:
                Patrol();
                break;
            case AIState.Chase:
                Chase();
                break;
            case AIState.Attack:
                Attack();
                break;
        }
    }

    private void Idle()
    {
        if (PlayerInSight())
        {
            currentState = AIState.Chase;
            _agent.isStopped = false;
            return;
        }
        else if (Time.time - lastTimePlayerDetected > idleToPatrolTimeThreshold)
        {
            currentState = AIState.Patrol;
            lastTimePlayerDetected = Time.time;
            _agent.isStopped = false;
            return;
        }

        _agent.isStopped = true;
    }

    private void Patrol()
    {
        if (PlayerInSight())
        {
            currentState = AIState.Chase;
            return;
        }

        _agent.speed = patrolSpeed;

        if (_agent.remainingDistance <= attackRange)
        {
            // wait for some time before moving to next patrol point
            if (patrolWaitTimeBeforeNextPatrolPointRemaining <= 0f)
            {
                currentPatrolPointIndex = (currentPatrolPointIndex + 1) % patrolPoints.Length;
                _agent.SetDestination(patrolPoints[currentPatrolPointIndex]);
                patrolWaitTimeBeforeNextPatrolPointRemaining = patrolWaitTimeBeforeNextPatrolPoint;
            }
            else
            {
                patrolWaitTimeBeforeNextPatrolPointRemaining -= Time.deltaTime;
            }
        }

        if (Time.time - lastTimePlayerDetected > patrolToIdleTimeThreshold)
        {
            currentState = AIState.Idle;
            lastTimePlayerDetected = Time.time;
        }
    }

    private void Chase()
    {
        if (PlayerInSight())
        {
            _agent.speed = chaseSpeed;
            _agent.SetDestination(lastKnownPlayerPosition);
        }
        else if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            currentState = AIState.Patrol;
            lastTimePlayerDetected = Time.time + extraPatrolTimeAfterChase;
            return;
        }

        if (Vector2.Distance(_transform.position, lastKnownPlayerPosition) <= attackRange)
        {
            currentState = AIState.Attack;
        }
    }

    private void Attack()
    {
        if (!PlayerInSight() || Vector2.Distance(_transform.position, player.position) > attackRange)
        {
            currentState = AIState.Chase;
            _agent.isStopped = false;
        }
        else
        {
            _agent.isStopped = true;

            if (enemyType == EnemyTypes.Melee)
            {
                // Attack the player Melee
                if (attackCoroutine == null)
                {
                    attackCoroutine = StartCoroutine(AttackMeleeCoroutine());
                }
            }
            else
            {
                // Attack the player Shooter
                if (attackCoroutine == null)
                {
                    attackCoroutine = StartCoroutine(AttackRangedCoroutine());
                }
            }
        }
    }

    private IEnumerator AttackMeleeCoroutine()
    {
        // Perform melee attack
        //TODO Attack Anim
        if(Vector2.Distance(_transform.position, player.position) <= attackRange)
        {
            Player.Instance.KillPlayer();
        }

        // Delay between attacks
        yield return new WaitForSeconds(meleeAttackDelay); // Change this to the desired delay between attacks

        // Reset attack coroutine
        attackCoroutine = null;
    }

    private IEnumerator AttackRangedCoroutine()
    {
        // Perform ranged attack
        Vector3 dir = (lastKnownPlayerPosition - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        gun.SetFirePointRot(angle);
        gun.Shoot();

        // Delay between attacks
        yield return new WaitForSeconds(rangedAttackDelay); // Change this to the desired delay between attacks

        // Reset attack coroutine
        attackCoroutine = null;
    }


    private bool PlayerInSight()
    {
        if(Player.Instance == null)
        {
            return false;
        }

        if (playerDetected && currentState != AIState.Attack)
        {
            // If player has already been detected, only check again after a certain time has passed
            if (Time.time - lastTimePlayerDetected > .75f)
            {
                playerDetected = false;
            }
            else
            {
                return true;
            }
        }

        if(Vector2.Distance(_transform.position, player.position) <= detectionRange)
        {
            RaycastHit2D hit = Physics2D.Raycast(_transform.position, player.position - _transform.position, detectionRange, collisionMask);
            Debug.DrawLine(_transform.position, _transform.position + (player.position - _transform.position).normalized * detectionRange, Color.magenta);
            //if(hit.collider != null)
            //{
            //    print(hit.collider.gameObject);
            //}

            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                playerDetected = true;
                lastTimePlayerDetected = Time.time;
                lastKnownPlayerPosition = player.position;
                return true;
            }
        }

        return false;
    }

    public void KillEnemy()
    {
        Instantiate(Player.Instance.deathParticle, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    #region Gizmos
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;

        // Draw the gizmo for each patrol point
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Gizmos.DrawSphere(patrolPoints[i], 0.2f);
        }
    }

    #endregion
}
