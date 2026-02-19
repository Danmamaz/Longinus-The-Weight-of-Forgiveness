using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class EnemyController : MonoBehaviour


{
    [Header("Stats")]

    public float MoveSpeed = 6f;
    public float RotationSpeed = 15f;
    public float DetectionRange = 10f;
    public float AttackRange = 2f;

    [SerializeField] private LayerMask playerLayer;
    [SerializeField] public Transform[] patrolWaypoints;

    [SerializeField] private Collider[] hits;
    private EnemyStateMachine _stateMachine;

    public Transform PlayerTarget { get; private set; }

    public Animator Animator { get; private set; }

    public EnemyMovementManager MovementManager { get; private set; }
    public EnemyStatsManager Stats { get; private set; }
    public EnemyIdleState IdleState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }
    public EnemyPatrolState PatrolState { get; private set; }

    private Coroutine _checkRoutine;

    public bool patrolingEnemy;


    private void Awake()
    {
        Animator = GetComponentInChildren<Animator>();
        Stats = GetComponent<EnemyStatsManager>();
        MovementManager = GetComponent<EnemyMovementManager>();

        // ! temp
        PlayerTarget = GameObject.FindGameObjectWithTag("Player")?.transform;
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

    private void InitStateMachine()
    {
        _stateMachine = new EnemyStateMachine();
        IdleState = new EnemyIdleState(this, _stateMachine);
        ChaseState = new EnemyChaseState(this, _stateMachine);
        AttackState = new EnemyAttackState(this, _stateMachine);
        PatrolState = new EnemyPatrolState(this, _stateMachine);
    }


    private void OnDestroy()
    {
        if (_checkRoutine != null)
        {
            StopCoroutine(_checkRoutine);
        }
    }
    private void OnDrawGizmos()
    {
        DetectionGizmo();
        AttackRangeGizmo();
    }

    private void DetectionGizmo()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, DetectionRange);
    }

    private void AttackRangeGizmo()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRange);
    }

    public bool IsPlayerInDetectionRange()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, DetectionRange, hits, playerLayer);
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

    public bool IsPlayerInAttackRange()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, AttackRange, hits, playerLayer);
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

    private IEnumerator CheckSwitchRoutine()
    {
        while (true)
        {
            _stateMachine.CurrentState?.CheckSwitchState();

            float waitTime = Random.Range(0.1f, 0.2f);
            yield return new WaitForSeconds(waitTime);
        }
    }


}