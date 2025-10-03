using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Transform target;
    private PlayerController playerController;

    [Header("Chase")]
    [SerializeField] private float detectionRange = 30f;
    [SerializeField] private float lostRange = 35f;
    [SerializeField] private float stopRange = 3f;

    [Header("Wander")]
    [SerializeField] private float patrolRadius = 15f;
    [SerializeField] private float patrolWaitMin = 1.5f;
    [SerializeField] private float patrolWaitMax = 3.5f;
    [SerializeField] private Transform wanderCenter; // optional; if null, uses spawn position

    [Header("Animation")]
    [SerializeField] private string blendParam = "Blend";
    [SerializeField] private float blendDampTime = 0.1f;

    [Header("Rotation")]
    [SerializeField] private bool manualRotate = true;
    [SerializeField] private float rotateSpeedDegPerSec = 540f;

    private NavMeshAgent agent;
    private Animator animator;

    private bool isChasing;
    private bool hasHit;

    private Vector3 spawnPosition;
    private float nextPatrolTime;
    private Vector3 currentPatrolPoint;
    private bool hasPatrolPoint;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        agent.stoppingDistance = stopRange;
        agent.updateRotation = !manualRotate;
        spawnPosition = transform.position;
    }

    private void Start()
    {
        if (target == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController != null) target = playerController.transform;
        }
        ScheduleNextPatrol();
    }

    private void Update()
    {
        if (agent == null)
        {
            SetBlend(0f);
            return;
        }

        if (target == null)
        {
            DoWander();
            return;
        }

        Vector3 toTarget = target.position - agent.transform.position;
        float sqrDist = toTarget.sqrMagnitude;
        float detectSqr = detectionRange * detectionRange;
        float lostSqr = lostRange * lostRange;
        float stopSqr = stopRange * stopRange;

        if (!isChasing)
        {
            if (sqrDist <= detectSqr) isChasing = true;
        }
        else
        {
            if (sqrDist >= lostSqr) isChasing = false;
        }

        if (!hasHit && isChasing)
        {
            if (sqrDist > stopSqr)
            {
                agent.SetDestination(target.position);
            }
            else
            {
                agent.ResetPath();
            }
        }
        else if (!isChasing)
        {
            DoWander();
        }
        else
        {
            if (!agent.pathPending && agent.hasPath) agent.ResetPath();
        }

        float speed01 = agent.speed > 0.001f ? Mathf.Clamp01(agent.velocity.magnitude / agent.speed) : 0f;
        SetBlend(speed01);

        if (manualRotate)
        {
            Vector3 vel = agent.velocity; vel.y = 0f;
            if (vel.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(vel.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeedDegPerSec * Time.deltaTime);
            }
        }
    }

    private void DoWander()
    {
        if (Time.time >= nextPatrolTime || !hasPatrolPoint || ReachedDestination())
        {
            Vector3 center = wanderCenter ? wanderCenter.position : spawnPosition;
            hasPatrolPoint = TryGetRandomPoint(center, patrolRadius, out currentPatrolPoint);
            if (hasPatrolPoint) agent.SetDestination(currentPatrolPoint);
            ScheduleNextPatrol();
        }
    }

    private bool ReachedDestination()
    {
        if (agent.pathPending) return false;
        if (agent.remainingDistance > Mathf.Max(0.1f, agent.stoppingDistance)) return false;
        if (agent.hasPath && agent.velocity.sqrMagnitude > 0.01f) return false;
        return true;
    }

    private void ScheduleNextPatrol()
    {
        nextPatrolTime = Time.time + Random.Range(patrolWaitMin, patrolWaitMax);
    }

    private bool TryGetRandomPoint(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 random = center + Random.insideUnitSphere * radius;
            if (NavMesh.SamplePosition(random, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = center;
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasHit && other.CompareTag("Player"))
        {
            hasHit = true;
            Debug.Log($"{gameObject.name} Hit Player");
        }
        else if (!hasHit && other.CompareTag("Gold"))
        {
            // hasHit = true;
            Debug.Log($"{gameObject.name} Hit Player");
        }
    }

    private void OnValidate()
    {
        if (agent != null) agent.stoppingDistance = stopRange;
        if (lostRange < detectionRange) lostRange = detectionRange + 1f;
        if (patrolWaitMax < patrolWaitMin) patrolWaitMax = patrolWaitMin;
    }

    private void SetBlend(float value)
    {
        if (animator == null) return;
        animator.SetFloat(blendParam, value, blendDampTime, Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopRange);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(wanderCenter ? wanderCenter.position : (Application.isPlaying ? spawnPosition : transform.position), patrolRadius);
    }
}