using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAIController : MonoBehaviour
{
    [Header("General")]
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;

    [Header("Patrolling")]
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;
    private bool isWaiting;
    private Coroutine waitCoroutine;

    [Header("Attacking")]
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    private EnemyAttack enemyAttack;
    public float attackRange;

    [Header("Vision")]
    public bool playerInSightRange;
    public bool playerInAttackRange;
    public float viewAngle;
    public float viewDistance;

    [Header("Chase Memory")]
    public float loseSightDelay;
    private float loseSightTimer;
    private bool isChasing;

    [Header("Movement Speed")]
    public float patrolSpeed;
    public float chaseSpeed;

    // Start is called before the first frame update
    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        enemyAttack = GetComponent<EnemyAttack>();

        agent.speed = patrolSpeed;
    }

    private void Update()
    {
        bool canCurrentlySeePlayer = CanSeePlayer();
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // If enemy can see player normally, enter chase mode
        if (canCurrentlySeePlayer)
        {
            isChasing = true;
            loseSightTimer = loseSightDelay;
        }

        // If chasing but can't currently see player
        if (isChasing && !canCurrentlySeePlayer)
        {
            loseSightTimer -= Time.deltaTime;

            if (loseSightTimer <= 0f)
            {
                isChasing = false;
            }
        }

        // Interrupt waiting if player gets within sight
        if (isChasing && isWaiting)
        {
            StopWaiting();
        }

        // STATE LOGIC
        if (!isChasing)
        {
            agent.speed = patrolSpeed;
            Patrolling();
        }
        else if (isChasing && !playerInAttackRange)
        {
            agent.speed = chaseSpeed;
            ChasePlayer();
        }
        else if (isChasing && playerInAttackRange)
        {
            agent.speed = 0f;
            AttackPlayer();
        }
    }

    private void Patrolling()
    {
        if (isWaiting) return;

        // If walk point is not set, search for one
        if (!walkPointSet)
            SearchWalkPoint();
        // If walk point is set, move enemy to walk point
        if (walkPointSet)
            agent.SetDestination(walkPoint);

        // If enemy has reached walk point, start waiting 
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && walkPointSet)
        {
            walkPointSet = false;
            // Start waiting at walk point
            if (waitCoroutine == null)
                waitCoroutine = StartCoroutine(WaitAtWalkPoint());
        }
    }

    private void SearchWalkPoint()
    {
        // Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        // Makes sure enemy doesn't move
        agent.SetDestination(transform.position);
        // Makes enemy look at player while ignoring y axis to prevent that weird tilting
        Vector3 direction = (player.position - transform.position);
        direction.y = 0f;
        // Smoothly rotate towards the player
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

        if (!alreadyAttacked)
        {
            enemyAttack.Attack(); 
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private bool CanSeePlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check distance
        if (distanceToPlayer > viewDistance)
            return false;

        // Check angle
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > viewAngle / 2)
            return false;

        //Check line of sight
        if (Physics.Raycast(transform.position + Vector3.up * 1.5f, directionToPlayer, out RaycastHit hit, viewDistance))
        {
            if (hit.transform == player)
                return true;
        }

        return false;
    }

    private void StopWaiting()
    {
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

        isWaiting = false;
        agent.isStopped = false;
    }

    private IEnumerator WaitAtWalkPoint()
    {
        isWaiting = true;
        agent.isStopped = true;

        float waitTime = Random.Range(2f, 5f);
        yield return new WaitForSeconds(waitTime);

        agent.isStopped = false;
        isWaiting = false;
        waitCoroutine = null;
    }

}
