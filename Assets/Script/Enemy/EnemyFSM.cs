using System.Collections.Generic;
using UnityEngine;

// ════════════════════════════════════════════════════════
//  BASE STATE
// ════════════════════════════════════════════════════════

public abstract class EnemyState
{
    protected EnemyAI  ai;
    protected EnemyFSM fsm;

    protected EnemyState(EnemyAI ai, EnemyFSM fsm)
    {
        this.ai  = ai;
        this.fsm = fsm;
    }

    public virtual void OnEnter()  { }
    public virtual void OnExit()   { }
    public abstract void OnUpdate();
}

// ════════════════════════════════════════════════════════
//  STATE : IDLE
// ════════════════════════════════════════════════════════

public class IdleState : EnemyState
{
    private readonly float duration;
    private float timer;

    public IdleState(EnemyAI ai, EnemyFSM fsm, float duration = 1.5f) : base(ai, fsm)
        => this.duration = duration;

    public override void OnEnter()
    {
        timer = duration;
        ai.StopMovement();
    }

    public override void OnUpdate()
    {
        if (ai.IsPlayerDetected()) { fsm.ChangeState(fsm.ChaseState); return; }
        timer -= Time.fixedDeltaTime;
        if (timer <= 0f) fsm.ChangeState(fsm.PatrolState);
    }
}

// ════════════════════════════════════════════════════════
//  STATE : PATROL
//  FIX: RequestPath hanya dipanggil saat benar-benar perlu,
//       bukan setiap frame. pathIndex dijaga agar tidak reset.
// ════════════════════════════════════════════════════════

public class PatrolState : EnemyState
{
    private readonly Transform[] patrolPoints;
    private readonly float waitTime;
    private readonly float arriveThreshold;
    private readonly float roamRadius;
    private readonly Vector2 origin;

    private int     patrolIndex = 0;
    private float   waitTimer   = 0f;
    private Vector2 roamTarget;
    private Vector2 lastRequestedTarget;
    private bool    hasActiveTarget = false;

    public PatrolState(EnemyAI ai, EnemyFSM fsm,
        Transform[] points, float waitTime,
        float arriveThreshold, float roamRadius, Vector2 origin)
        : base(ai, fsm)
    {
        patrolPoints         = points;
        this.waitTime        = waitTime;
        this.arriveThreshold = arriveThreshold;
        this.roamRadius      = roamRadius;
        this.origin          = origin;
        roamTarget           = origin;
    }

    public override void OnEnter()
    {
        // Jangan langsung request path — biarkan OnUpdate yg handle
        hasActiveTarget = false;
    }

    public override void OnExit()
    {
        fsm.ClearPath();
        hasActiveTarget = false;
    }

    public override void OnUpdate()
    {
        if (ai.IsPlayerDetected()) { fsm.ChangeState(fsm.ChaseState); return; }

        // Tunggu di titik patroli
        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            ai.StopMovement();
            return;
        }

        Vector2 destination = GetDestination();

        // Sudah sampai di tujuan?
        if (Vector2.Distance(ai.Position, destination) <= arriveThreshold)
        {
            OnArrived();
            return;
        }

        // Request path baru hanya jika tujuan berubah atau belum punya path
        bool needNewPath = !hasActiveTarget
                        || Vector2.Distance(destination, lastRequestedTarget) > arriveThreshold
                        || fsm.IsPathFinished();

        if (needNewPath)
        {
            fsm.RequestPath(ai.Position, destination);
            lastRequestedTarget = destination;
            hasActiveTarget     = true;
        }

        // Ikuti path yang sudah dihitung
        fsm.AdvanceAndMoveAlongPath(arriveThreshold);
    }

    // ── Helpers ──────────────────────────────────────────────

    private Vector2 GetDestination()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
            return patrolPoints[patrolIndex].position;

        return roamTarget;
    }

    private void OnArrived()
    {
        waitTimer       = waitTime;
        hasActiveTarget = false;
        fsm.ClearPath();

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        }
        else
        {
            // Pilih titik roam baru
            roamTarget = origin + Random.insideUnitCircle * roamRadius;
        }
    }
}

// ════════════════════════════════════════════════════════
//  STATE : CHASE
//  FIX: pathIndex TIDAK pernah di-reset saat hanya refresh path.
//       Path baru menggantikan path lama tetapi tracker waypoint
//       dimulai dari awal path baru (yang memang benar — posisi
//       enemy sudah bergerak, path baru dihitung dari posisi baru).
//       Transisi LoS↔A* tidak menghapus tracker sembarangan.
// ════════════════════════════════════════════════════════

public class ChaseState : EnemyState
{
    private readonly LayerMask obstacleLayer;
    private readonly float     refreshInterval;

    private float refreshTimer  = 0f;
    private bool  usingAstarPath = false;

    public ChaseState(EnemyAI ai, EnemyFSM fsm,
        float refreshInterval, LayerMask obstacleLayer)
        : base(ai, fsm)
    {
        this.refreshInterval = refreshInterval;
        this.obstacleLayer   = obstacleLayer;
    }

    public override void OnEnter()
    {
        refreshTimer  = 0f;   // paksa hitung path segera saat masuk state
        usingAstarPath = false;
    }

    public override void OnExit()
    {
        fsm.ClearPath();
        usingAstarPath = false;
    }

    public override void OnUpdate()
    {
        if (!ai.IsPlayerDetected())
        {
            fsm.ChangeState(fsm.PatrolState);
            return;
        }

        refreshTimer -= Time.fixedDeltaTime;

        if (refreshTimer <= 0f)
        {
            refreshTimer = refreshInterval;
            EvaluatePath();
        }

        // Bergerak mengikuti path jika ada, atau langsung jika LoS bersih
        if (usingAstarPath && !fsm.IsPathFinished())
        {
            fsm.AdvanceAndMoveAlongPath(0.3f);
        }
        else
        {
            // LoS bersih → gerak langsung
            ai.MoveDirectly(ai.PlayerPosition);
        }
    }

    // ── Path Evaluation ──────────────────────────────────────

    private void EvaluatePath()
    {
        if (HasLineOfSight())
        {
            // LoS bersih — tidak perlu A*, bersihkan path lama
            fsm.ClearPath();
            usingAstarPath = false;
        }
        else
        {
            // Terhalang — hitung A* dari posisi saat ini ke player
            bool ok = fsm.RequestPath(ai.Position, ai.PlayerPosition);
            usingAstarPath = ok;
        }
    }

    private bool HasLineOfSight()
    {
        Vector2 from = ai.Position;
        Vector2 to   = ai.PlayerPosition;
        Vector2 dir  = (to - from).normalized;
        float   dist = Vector2.Distance(from, to);
        RaycastHit2D hit = Physics2D.Raycast(from, dir, dist, obstacleLayer);
        return hit.collider == null;
    }
}

// ════════════════════════════════════════════════════════
//  FSM CONTROLLER
//  FIX: pathIndex disimpan di sini dan HANYA di-reset
//       saat path baru di-set (RequestPath), bukan saat
//       FollowPath dipanggil. Ini mencegah enemy "balik ke
//       awal path" setiap frame.
// ════════════════════════════════════════════════════════

[RequireComponent(typeof(EnemyAI))]
public class EnemyFSM : MonoBehaviour
{
    [Header("Chase Settings")]
    [SerializeField] private float     chaseRefreshRate = 0.3f;
    [SerializeField] private LayerMask obstacleLayer;

    // ── State References ─────────────────────────────────────
    public IdleState   IdleState   { get; private set; }
    public PatrolState PatrolState { get; private set; }
    public ChaseState  ChaseState  { get; private set; }

    // ── Path State (PERSISTENT - tidak di-reset tiap frame) ──
    private List<Vector2> currentPath  = null;
    private int           pathIndex    = 0;   // ← satu-satunya sumber kebenaran index

    // ── Internal ─────────────────────────────────────────────
    private EnemyState activeState;
    private EnemyAI    ai;

    // ── Lifecycle ─────────────────────────────────────────────

    private void Awake()
    {
        ai = GetComponent<EnemyAI>();
    }

    private void Start()
    {
        IdleState   = new IdleState(ai, this);
        PatrolState = new PatrolState(ai, this,
            ai.PatrolPoints, ai.PatrolWaitTime,
            ai.PatrolThreshold, ai.PatrolRadius,
            ai.Position);
        ChaseState  = new ChaseState(ai, this, chaseRefreshRate, obstacleLayer);

        ChangeState(ai.UsePatrol ? (EnemyState)PatrolState : IdleState);
    }

    private void FixedUpdate()
    {
        activeState?.OnUpdate();
    }

    // ── State Machine API ─────────────────────────────────────

    public void ChangeState(EnemyState next)
    {
        activeState?.OnExit();
        activeState = next;
        activeState.OnEnter();
    }

    public string CurrentStateName => activeState?.GetType().Name ?? "None";

    // ── Path API ─────────────────────────────────────────────

    /// <summary>
    /// Minta A* path baru dari 'from' ke 'to'.
    /// pathIndex SELALU reset ke 0 saat path baru diterima.
    /// Return true jika path ditemukan.
    /// </summary>
    public bool RequestPath(Vector2 from, Vector2 to)
    {
        if (EnemyAstar.Instance == null) return false;

        var path = EnemyAstar.Instance.FindPath(from, to);
        if (path == null || path.Count == 0) return false;

        currentPath = path;
        pathIndex   = 0;   // reset index HANYA di sini
        return true;
    }

    /// <summary>Hapus path aktif.</summary>
    public void ClearPath()
    {
        currentPath = null;
        pathIndex   = 0;
    }

    /// <summary>True jika tidak ada path atau semua waypoint sudah dicapai.</summary>
    public bool IsPathFinished()
        => currentPath == null || pathIndex >= currentPath.Count;

    /// <summary>
    /// Advance pathIndex jika waypoint saat ini sudah cukup dekat,
    /// lalu perintahkan EnemyAI bergerak ke waypoint tersebut.
    /// Dipanggil oleh State setiap frame — TIDAK mengubah pathIndex
    /// secara tidak terduga.
    /// </summary>
    public void AdvanceAndMoveAlongPath(float waypointRadius)
    {
        if (IsPathFinished()) return;

        Vector2 currentWaypoint = currentPath[pathIndex];
        float   dist            = Vector2.Distance(ai.Position, currentWaypoint);

        // Sudah cukup dekat → maju ke waypoint berikutnya
        if (dist <= waypointRadius)
        {
            pathIndex++;
            if (IsPathFinished()) return;
            currentWaypoint = currentPath[pathIndex];
        }

        // Gerak ke waypoint yang sudah pasti benar
        ai.MoveToward(currentWaypoint);
    }

#if UNITY_EDITOR
    // ── Debug Gizmos ─────────────────────────────────────────

    private void OnDrawGizmos()
    {
        if (currentPath == null || currentPath.Count == 0) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < currentPath.Count - 1; i++)
            Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);

        // Highlight waypoint aktif
        if (pathIndex < currentPath.Count)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentPath[pathIndex], 0.2f);
        }
    }
#endif
}