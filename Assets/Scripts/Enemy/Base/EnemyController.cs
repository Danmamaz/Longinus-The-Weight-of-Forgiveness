using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    public float MoveSpeed = 6f;
    public float RotationSpeed = 15f;
    public float DetectionRange = 10f;
    public float AttackRange = 2f;

    [Header("Line of Sight")]
    [SerializeField] private float viewAngle = 60f;
    [SerializeField] private Transform eyePoint;
    [SerializeField] private float eyeHeightOffset = 1.5f;

    [Header("Hearing")]
    [SerializeField] private float hearingRadius = 6f;

    [Header("Search")]
    [SerializeField] private float searchArrivalThreshold = 1f; // how close to last-known pos before giving up

    [Header("Layers")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [SerializeField] public Transform[] patrolWaypoints;
    [SerializeField] private Collider[] hits;

    private EnemyStateMachine _stateMachine;
    public EnemyStateMachine StateMachine => _stateMachine;
    private float _viewDotThreshold;

    // --- Last-known position ---
    public Vector3 LastKnownPlayerPosition { get; private set; }
    public bool HasLastKnownPosition { get; private set; }

    public Transform PlayerTarget { get; private set; }
    public Animator Animator { get; private set; }
    public EnemyMovementManager MovementManager { get; private set; }
    public EnemyStatsManager Stats { get; private set; }
    public EnemyIdleState IdleState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }
    public EnemyPatrolState PatrolState { get; private set; }
    public EnemySearchState SearchState { get; private set; }

    private Coroutine _checkRoutine;
    public bool patrolingEnemy;

    // --- Debug ---
    private bool _debugCanSeePlayer;
    private Vector3 _debugRayOrigin;
    private Vector3 _debugRayTarget;

    // ----------------------------------------------------------------
    // Lifecycle
    // ----------------------------------------------------------------

    private void Awake()
    {
        Animator = GetComponentInChildren<Animator>();
        Stats = GetComponent<EnemyStatsManager>();
        MovementManager = GetComponent<EnemyMovementManager>();

        PlayerTarget = GameObject.FindGameObjectWithTag("Player")?.transform;

        _viewDotThreshold = Mathf.Cos(viewAngle * Mathf.Deg2Rad);

        InitStateMachine();
    }

    private void Start()
    {
        _stateMachine.Initialize(IdleState);
        _checkRoutine = StartCoroutine(CheckSwitchRoutine());
    }

    private void Update()
    {
        _stateMachine.CurrentState.UpdateState();
    }

    private void FixedUpdate()
    {
        _stateMachine.CurrentState.FixedUpdateState();
    }

    private void OnDestroy()
    {
        if (_checkRoutine != null)
            StopCoroutine(_checkRoutine);
    }

    private void InitStateMachine()
    {
        _stateMachine = new EnemyStateMachine();
        IdleState = new EnemyIdleState(this, _stateMachine);
        ChaseState = new EnemyChaseState(this, _stateMachine);
        AttackState = new EnemyAttackState(this, _stateMachine);
        PatrolState = new EnemyPatrolState(this, _stateMachine);
        SearchState = new EnemySearchState(this, _stateMachine);
    }

    // ----------------------------------------------------------------
    // Last-Known Position
    // ----------------------------------------------------------------

    /// <summary>
    /// Call every frame/check while the player IS detected to keep the
    /// breadcrumb up to date. When detection is lost, the last value
    /// written here becomes the search destination.
    /// </summary>
    public void UpdateLastKnownPosition(Vector3 position)
    {
        LastKnownPlayerPosition = position;
        HasLastKnownPosition = true;
    }

    public void ClearLastKnownPosition()
    {
        HasLastKnownPosition = false;
    }

    /// <summary>
    /// True when the enemy is close enough to the last-known position
    /// to consider the search finished.
    /// </summary>
    public bool HasReachedLastKnownPosition()
    {
        if (!HasLastKnownPosition) return true;

        float sqrDist = (transform.position - LastKnownPlayerPosition).sqrMagnitude;
        return sqrDist <= searchArrivalThreshold * searchArrivalThreshold;
    }

    // ----------------------------------------------------------------
    // Detection
    // ----------------------------------------------------------------

    private Vector3 EyePosition =>
        eyePoint != null
            ? eyePoint.position
            : transform.position + Vector3.up * eyeHeightOffset;

    public bool IsPlayerInDetectionRange()
    {
        _debugCanSeePlayer = false;

        int count = Physics.OverlapSphereNonAlloc(
            transform.position, DetectionRange, hits, playerLayer);

        for (int i = 0; i < count; i++)
        {
            if (!hits[i].CompareTag("Player")) continue;

            Transform player = hits[i].transform;
            PlayerTarget = player;

            Vector3 origin = EyePosition;
            Vector3 targetPoint = player.position + Vector3.up;
            Vector3 direction = (targetPoint - origin).normalized;
            float distance = Vector3.Distance(origin, targetPoint);

            _debugRayOrigin = origin;
            _debugRayTarget = targetPoint;

            // 1. FoV
            float dot = Vector3.Dot(transform.forward, direction);
            bool insideFov = dot >= _viewDotThreshold;

            if (insideFov)
            {
                // 2. LoS
                bool blocked = Physics.Raycast(
                    origin, direction, distance, obstacleLayer);

                if (!blocked)
                {
                    _debugCanSeePlayer = true;
                    UpdateLastKnownPosition(player.position); // <-- breadcrumb
                    return true;
                }
            }

            // 3. Hearing
            float sqrDist = (player.position - transform.position).sqrMagnitude;
            if (sqrDist <= hearingRadius * hearingRadius)
            {
                INoiseSource noise = player.GetComponent<INoiseSource>();
                if (noise != null && noise.IsMakingNoise())
                {
                    UpdateLastKnownPosition(player.position); // <-- breadcrumb
                    return true;
                }
            }

            return false;
        }

        return false;
    }

    public bool IsPlayerInAttackRange()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, AttackRange, hits, playerLayer);

        for (int i = 0; i < count; i++)
        {
            if (hits[i].CompareTag("Player"))
            {
                PlayerTarget = hits[i].transform;
                return true;
            }
        }
        return false;
    }

    // ----------------------------------------------------------------
    // Coroutine
    // ----------------------------------------------------------------

    private IEnumerator CheckSwitchRoutine()
    {
        while (true)
        {
            _stateMachine.CurrentState?.CheckSwitchState();
            yield return new WaitForSeconds(Random.Range(0.1f, 0.2f));
        }
    }

    // ----------------------------------------------------------------
    // Gizmos
    // ----------------------------------------------------------------

    private void OnDrawGizmos()
    {
        DrawDetectionSphere();
        DrawAttackRange();
        DrawHearingRadius();
        DrawFovCone();
        DrawLoSRay();
        DrawLastKnownPosition();
    }

    private void DrawDetectionSphere()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, DetectionRange);
    }

    private void DrawAttackRange()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRange);
    }

    private void DrawHearingRadius()
    {
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, hearingRadius);
    }

    private void DrawFovCone()
    {
        float halfAngle = viewAngle;
        Vector3 origin = Application.isPlaying
            ? EyePosition
            : transform.position + Vector3.up * eyeHeightOffset;

        Vector3 leftDir  = Quaternion.Euler(0, -halfAngle, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0,  halfAngle, 0) * transform.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(origin, leftDir  * DetectionRange);
        Gizmos.DrawRay(origin, rightDir * DetectionRange);

        int segments = 20;
        float step = (halfAngle * 2f) / segments;
        Vector3 prev = Quaternion.Euler(0, -halfAngle, 0) * transform.forward * DetectionRange + origin;

        for (int i = 1; i <= segments; i++)
        {
            float angle = -halfAngle + step * i;
            Vector3 next = Quaternion.Euler(0, angle, 0) * transform.forward * DetectionRange + origin;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }

    private void DrawLoSRay()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = _debugCanSeePlayer ? Color.green : Color.red;
        Gizmos.DrawLine(_debugRayOrigin, _debugRayTarget);
    }

    private void DrawLastKnownPosition()
    {
        if (!Application.isPlaying || !HasLastKnownPosition) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(LastKnownPlayerPosition, 0.35f);
        Gizmos.DrawLine(transform.position, LastKnownPlayerPosition);
    }
}