using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    // FSM states
    public enum State { Search, Chase, Attack }
    public State currentState = State.Search;

    // sight + range settings
    public float sightRange = 15f;
    public float attackRange = 2.5f;
    public float fieldOfView = 110f;

    // patrol settings
    public float patrolRadius = 12f;
    public float patrolWaitTime = 2f;

    // aura damage settings
    public int auraDamagePerTick = 5;
    public float auraTickRate = 1f;

    // refs
    private NavMeshAgent agent;
    private Transform player;
    private PlayerHealth playerHealth;

    // timers
    private float patrolTimer;
    private float auraTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // find the player by tag
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerHealth = p.GetComponent<PlayerHealth>();
        }
    }

    void Update()
    {
        if (player == null) return;

        // run the current state
        switch (currentState)
        {
            case State.Search: DoSearch(); break;
            case State.Chase: DoChase(); break;
            case State.Attack: DoAttack(); break;
        }

        // check if we should switch states
        CheckTransitions();
    }

    // wander to random points on the NavMesh
    void DoSearch()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            patrolTimer -= Time.deltaTime;
            if (patrolTimer <= 0f)
            {
                Vector3 next = RandomNavMeshPoint(transform.position, patrolRadius);
                agent.SetDestination(next);
                patrolTimer = patrolWaitTime;
            }
        }
    }

    // run toward the player
    void DoChase()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    // stop and tick aura damage
    void DoAttack()
    {
        agent.isStopped = true;

        // face the player on the horizontal plane
        Vector3 lookAt = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookAt);

        auraTimer -= Time.deltaTime;
        if (auraTimer <= 0f)
        {
            if (playerHealth != null)
                playerHealth.TakeDamage(auraDamagePerTick);
            auraTimer = auraTickRate;
        }
    }

    // decide which state to be in
    void CheckTransitions()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        bool canSee = CanSeePlayer(dist);

        if (canSee && dist <= attackRange)
            currentState = State.Attack;
        else if (canSee)
            currentState = State.Chase;
        else
        {
            if (currentState != State.Search)
            {
                currentState = State.Search;
                patrolTimer = 0f;
                agent.isStopped = false;
            }
        }
    }

    // distance + FOV cone + raycast line-of-sight
    bool CanSeePlayer(float dist)
    {
        if (dist > sightRange) return false;

        Vector3 toPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > fieldOfView * 0.5f) return false;

        Vector3 eye = transform.position + Vector3.up * 1.5f;
        if (Physics.Raycast(eye, toPlayer, out RaycastHit hit, sightRange))
        {
            return hit.transform.CompareTag("Player");
        }
        return false;
    }

    // pick a random reachable point near the agent
    Vector3 RandomNavMeshPoint(Vector3 origin, float radius)
    {
        Vector3 random = origin + Random.insideUnitSphere * radius;
        if (NavMesh.SamplePosition(random, out NavMeshHit hit, radius, NavMesh.AllAreas))
            return hit.position;
        return origin;
    }

    // draw sight + attack ranges in the editor for tuning
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}