using UnityEngine;

/// <summary>
/// EnemyAI — mengelola fisika, deteksi, dan movement primitif.
/// Logika state → EnemyFSM.cs
/// Pathfinding   → AStarPathfinder.cs
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────

    [Header("Detection")]
    [SerializeField] private float detectionAngle = 60f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float angleOffset    = 0f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed     = 3f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float stopDistance  = 0.2f;

    [Header("Patrol")]
    [SerializeField] public bool        usePatrol       = true;
    [SerializeField] public Transform[] patrolPoints;
    [SerializeField] public float       patrolWaitTime  = 1f;
    [SerializeField] public float       patrolThreshold = 0.25f;
    [SerializeField] public float       patrolRadius    = 4f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;

    // ── Public Properties ────────────────────────────────────

    public bool        UsePatrol       => usePatrol;
    public Transform[] PatrolPoints    => patrolPoints;
    public float       PatrolWaitTime  => patrolWaitTime;
    public float       PatrolThreshold => patrolThreshold;
    public float       PatrolRadius    => patrolRadius;

    /// <summary>Posisi Rigidbody enemy (digunakan oleh FSM/State).</summary>
    public Vector2 Position => rb.position;

    /// <summary>Posisi player terakhir diketahui.</summary>
    public Vector2 PlayerPosition => target != null ? (Vector2)target.position : rb.position;

    // ── Private ───────────────────────────────────────────────

    private Rigidbody2D rb;
    private Transform   target;
    private bool        isPlayerDetected;

    // ── Lifecycle ─────────────────────────────────────────────

    private void Awake()
    {
        rb              = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.angularDrag  = 0f;
        FindPlayer();
    }

    private void FixedUpdate()
    {
        if (target == null) { FindPlayer(); return; }
        UpdateDetection();
        // Gerak dan state sepenuhnya dikelola EnemyFSM
    }

    // ── Detection ────────────────────────────────────────────

    private void UpdateDetection()
    {
        float dist = Vector2.Distance(rb.position, target.position);

        if (dist > detectionRange)
        {
            isPlayerDetected = false;
            return;
        }

        Vector2 dirToPlayer = ((Vector2)target.position - rb.position).normalized;
        float   angle       = Vector2.Angle(ForwardDirection(), dirToPlayer);
        isPlayerDetected    = angle <= detectionAngle * 0.5f;
    }

    private void FindPlayer()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) target = go.transform;
    }

    // ══════════════════════════════════════════════════════════
    //  MOVEMENT API  — dipanggil oleh EnemyFSM / State
    //  Semua method ini hanya menerima SATU tujuan dan bergerak
    //  ke sana tanpa ambiguitas.
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Bergerak satu langkah menuju worldTarget.
    /// Digunakan oleh FSM saat mengikuti waypoint A* maupun gerak langsung.
    /// </summary>
    public void MoveToward(Vector2 worldTarget)
    {
        Vector2 delta = worldTarget - rb.position;
        float   dist  = delta.magnitude;

        if (dist <= stopDistance)
        {
            StopMovement();
            return;
        }

        Vector2 dir    = delta / dist; // normalized tanpa alokasi tambahan
        Vector2 newPos = rb.position + dir * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
        RotateToward(dir);
    }

    /// <summary>Gerak langsung ke player tanpa pathfinding (saat LoS bersih).</summary>
    public void MoveDirectly(Vector2 targetPos) => MoveToward(targetPos);

    /// <summary>Hentikan semua gerak.</summary>
    public void StopMovement()
    {
        rb.velocity = Vector2.zero;
    }

    // ── Rotation ─────────────────────────────────────────────

    private void RotateToward(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f) return;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + angleOffset;
        float current     = rb.rotation;
        float smooth      = Mathf.MoveTowardsAngle(current, targetAngle, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(smooth);
    }

    // ── Queries ───────────────────────────────────────────────

    public bool IsPlayerDetected() => isPlayerDetected;

    private Vector2 ForwardDirection()
    {
        float rad = (rb.rotation + angleOffset) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    // ── Gizmos ───────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Vector2 fwd = Application.isPlaying
            ? ForwardDirection()
            : new Vector2(
                Mathf.Cos((transform.eulerAngles.z + angleOffset) * Mathf.Deg2Rad),
                Mathf.Sin((transform.eulerAngles.z + angleOffset) * Mathf.Deg2Rad));

        // Lingkaran deteksi
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Sudut cone
        Vector3 left  = Quaternion.AngleAxis(-detectionAngle * 0.5f, Vector3.forward) * (Vector3)fwd;
        Vector3 right = Quaternion.AngleAxis( detectionAngle * 0.5f, Vector3.forward) * (Vector3)fwd;
        Gizmos.color  = Color.red;
        Gizmos.DrawRay(transform.position, left  * detectionRange);
        Gizmos.DrawRay(transform.position, right * detectionRange);

        // Forward
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, (Vector3)fwd * (detectionRange * 0.5f));

#if UNITY_EDITOR
        // Label state
        var fsm = GetComponent<EnemyFSM>();
        if (fsm != null)
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 1.3f,
                $"[{fsm.CurrentStateName}]");
#endif
    }
}