using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionAngle = 60f;

    [SerializeField] private float detectionRange = 10f;

    [SerializeField] private float angleOffset = 0f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;

    [SerializeField] private float rotationSpeed = 360f;

    [SerializeField] private float stopDistance = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;

    [Header("Patrol")]
    [SerializeField] private bool usePatrol = true;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolWaitTime = 1f;
    [SerializeField] private float patrolThreshold = 0.2f;
    [SerializeField] private float patrolRadius = 3f; // fallback roaming radius when no points

    private Rigidbody2D rb;

    private Transform target;

    private bool isPlayerDetected;

    private Vector2 directionToPlayer;

    private float distanceToPlayer;

    // Patrol state
    private int patrolIndex = 0;
    private float patrolWaitTimer = 0f;
    private Vector2 patrolOrigin;
    private Vector2 roamTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.angularDrag = 0f;

        FindPlayer();
        patrolOrigin = transform.position;
        roamTarget = patrolOrigin;
    }

    private void FixedUpdate()
    {
        // Cari ulang jika player hilang/destroy
        if (target == null)
        {
            FindPlayer();
            return;
        }

        DetectPlayer();

        if (isPlayerDetected)
        {
            MoveToPlayer();
            RotateToPlayer();
        }
        else
        {
            if (usePatrol)
            {
                Patrol();
            }
            else
            {
                StopMovement();
            }
        }
    }

    // =========================
    // PATROL
    // =========================
    private void Patrol()
    {
        if (patrolWaitTimer > 0f)
        {
            patrolWaitTimer -= Time.fixedDeltaTime;
            StopMovement();
            return;
        }

        Vector2 currentPos = rb.position;

        Vector2 targetPos;
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            targetPos = patrolPoints[patrolIndex].position;
        }
        else
        {
            // fallback roam target
            if (Vector2.Distance(currentPos, roamTarget) < patrolThreshold)
            {
                roamTarget = patrolOrigin + Random.insideUnitCircle * patrolRadius;
            }
            targetPos = roamTarget;
        }

        Vector2 dir = (targetPos - currentPos);
        float distance = dir.magnitude;
        if (distance <= patrolThreshold)
        {
            // reached point
            patrolWaitTimer = patrolWaitTime;
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            }
            return;
        }

        Vector2 direction = dir.normalized;

        Vector2 newPosition = currentPos + direction * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
        RotateToDirection(direction);
    }

    private void RotateToDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
            return;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + angleOffset;
        float currentAngle = transform.eulerAngles.z;
        float smoothAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.fixedDeltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, smoothAngle);
        rb.MoveRotation(smoothAngle);
    }

    // =========================
    // FIND PLAYER
    // =========================
    private void FindPlayer()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            target = player.transform;
        }
    }

    // =========================
    // PLAYER DETECTION
    // =========================
    private void DetectPlayer()
    {
        Vector2 currentPosition = rb.position;
        Vector2 playerPosition = target.position;

        directionToPlayer =
            (playerPosition - currentPosition).normalized;

        distanceToPlayer =
            Vector2.Distance(
                currentPosition,
                playerPosition
            );

        if (distanceToPlayer > detectionRange)
        {
            isPlayerDetected = false;
            return;
        }

        Vector2 forward = GetForwardDirection();

        float angle =
            Vector2.Angle(
                forward,
                directionToPlayer
            );

        isPlayerDetected =
            angle <= detectionAngle * 0.5f;
    }

    // =========================
    // MOVEMENT
    // =========================
    private void MoveToPlayer()
    {
        if (distanceToPlayer <= stopDistance)
        {
            StopMovement();
            return;
        }

        Vector2 newPosition =
            rb.position +
            directionToPlayer *
            moveSpeed *
            Time.fixedDeltaTime;

        rb.MovePosition(newPosition);
    }

    private void StopMovement()
    {
        rb.velocity = Vector2.zero;
    }

    // =========================
    // ROTATION
    // =========================
    private void RotateToPlayer()
    {
        Vector2 direction =
            (Vector2)target.position - rb.position;

        if (direction.sqrMagnitude < 0.001f)
            return;

        float targetAngle =
            Mathf.Atan2(direction.y, direction.x) *
            Mathf.Rad2Deg +
            angleOffset;

        float currentAngle =
            transform.eulerAngles.z;

        float smoothAngle =
            Mathf.MoveTowardsAngle(
                currentAngle,
                targetAngle,
                rotationSpeed * Time.fixedDeltaTime
            );

        transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                smoothAngle
            );

        rb.MoveRotation(smoothAngle);
    }

    // =========================
    // FORWARD DIRECTION
    // =========================
    private Vector2 GetForwardDirection()
    {
        float angle =
            (transform.eulerAngles.z + angleOffset) *
            Mathf.Deg2Rad;

        return new Vector2(
            Mathf.Cos(angle),
            Mathf.Sin(angle)
        );
    }

    // =========================
    // GIZMOS
    // =========================
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        Vector2 forward =
            GetForwardDirection();

        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        Vector3 leftBoundary =
            Quaternion.AngleAxis(
                -detectionAngle * 0.5f,
                Vector3.forward
            ) * forward;

        Vector3 rightBoundary =
            Quaternion.AngleAxis(
                detectionAngle * 0.5f,
                Vector3.forward
            ) * forward;

        Gizmos.color = Color.red;

        Gizmos.DrawRay(
            transform.position,
            leftBoundary * detectionRange
        );

        Gizmos.DrawRay(
            transform.position,
            rightBoundary * detectionRange
        );

        Gizmos.color = Color.blue;

        Gizmos.DrawRay(
            transform.position,
            forward * (detectionRange * 0.5f)
        );
    }

    // =========================
    // PUBLIC API
    // =========================
    public bool IsPlayerDetected()
    {
        return isPlayerDetected;
    }
}