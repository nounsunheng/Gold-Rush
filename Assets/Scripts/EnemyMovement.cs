using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Transform target;
    private PlayerController playerController;

    [SerializeField] private float detectionRange = 30f;
    [SerializeField] private float lostRange = 35f;
    [SerializeField] private float stopRange = 3f;

    [SerializeField] private string blendParam = "Blend";
    [SerializeField] private float blendDampTime = 0.1f;

    [SerializeField] private bool manualRotate = true;
    [SerializeField] private float rotateSpeedDegPerSec = 540f;

    private NavMeshAgent agent;
    private Animator animator;

    private bool isChasing;
    private bool hasHit = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        agent.stoppingDistance = stopRange;
        agent.updateRotation = !manualRotate;
    }

    private void Start()
    {
        if (target == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController != null) target = playerController.transform;
        }
    }

    private void Update()
    {
        if (agent == null || target == null) 
        {
            SetBlend(0f);
            return;
        }

        Vector3 toTarget = target.position - agent.transform.position;
        float sqrDist = toTarget.sqrMagnitude;
        float detectSqr = detectionRange * detectionRange;
        float lostSqr   = lostRange * lostRange;
        float stopSqr   = stopRange * stopRange;

        if (!isChasing)
        {
            if (sqrDist <= detectSqr) isChasing = true;
        }
        else
        {
            if (sqrDist >= lostSqr) isChasing = false;
        }

        if (isChasing && !hasHit)
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
        else
        {
            if (!agent.pathPending && agent.hasPath) agent.ResetPath();
        }

        float speed01 = agent.speed > 0.001f ? Mathf.Clamp01(agent.velocity.magnitude / agent.speed) : 0f;
        SetBlend(speed01);

        if (manualRotate)
        {
            Vector3 vel = agent.velocity;
            vel.y = 0f;
            if (vel.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(vel.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeedDegPerSec * Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasHit && other.CompareTag("player"))
        {
            hasHit = true;
            Debug.Log($"{gameObject.name} Hit Player");
        }
    }

    private void OnValidate()
    {
        if (agent != null) agent.stoppingDistance = stopRange;
        if (lostRange < detectionRange) lostRange = detectionRange + 1f;
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
        Gizmos.DrawWireSphere(transform.position, lostRange);
    }
}
