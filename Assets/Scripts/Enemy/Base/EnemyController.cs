using UnityEngine;
using Enemy.BaseEnemy;

namespace Enemy.BaseEnemy
{
    [RequireComponent(typeof(Animator), typeof(EnemyMovementManager), typeof(EnemyStatsManager))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Core References")]
        [SerializeField] private Transform playerTransform;
        [SerializeField, Tooltip("Шар, що блокує видимість (стіни)")] 
        private LayerMask obstacleLayer;

        [Header("AI Sensors & Settings")]
        [SerializeField, Tooltip("Радіус виявлення гравця")] 
        private float detectionRange = 10f;
        [SerializeField, Tooltip("Кут зору противника (в градусах)")] 
        private float fieldOfViewAngle = 120f; // Додай це. 120-140 градусів - стандарт.
        [SerializeField, Tooltip("Радіус для переходу в атаку")] 
        private float attackRange = 2f;
        
        [Header("Patrol Settings")]
        public bool patrolingEnemy;
        public Transform[] patrolWaypoints;

        public Animator Animator { get; private set; }
        public EnemyMovementManager MovementManager { get; private set; }
        public EnemyStatsManager statsManager;

        private EnemyStateMachine _stateMachine;
        public EnemyIdleState IdleState { get; private set; }
        public EnemyChaseState ChaseState { get; private set; }
        public EnemyAttackState AttackState { get; private set; }
        public EnemyPatrolState PatrolState { get; private set; }
        public EnemySearchState SearchState { get; private set; }
        public EnemyCombatStrafeState CombatStrafeState { get; private set; }
        public EnemyStaggeredState StaggeredState { get; private set; }
        public EnemySparedState SparedState { get; private set; }


        public bool HasLastKnownPosition { get; private set; }
        public Vector3 LastKnownPlayerPosition { get; private set; }

        public Transform PlayerTransform => playerTransform;

        private bool _isDead;
        
        private float _sqrDetectionRange;
        private float _sqrAttackRange;

#region Unity Lifecycle

        private void Awake()
        {
            Animator = GetComponent<Animator>();
            MovementManager = GetComponent<EnemyMovementManager>();
            statsManager = GetComponent<EnemyStatsManager>();

            _sqrDetectionRange = detectionRange * detectionRange;
            _sqrAttackRange = attackRange * attackRange;

            _stateMachine = new EnemyStateMachine();
            IdleState = new EnemyIdleState(this, _stateMachine);
            ChaseState = new EnemyChaseState(this, _stateMachine);
            AttackState = new EnemyAttackState(this, _stateMachine);
            PatrolState = new EnemyPatrolState(this, _stateMachine);
            SearchState = new EnemySearchState(this, _stateMachine);
            CombatStrafeState = new EnemyCombatStrafeState(this, _stateMachine);
            StaggeredState = new EnemyStaggeredState(this, _stateMachine);
        }

        private void OnEnable()
        {
            _isDead = false;
            ClearLastKnownPosition();
            
            statsManager.OnDeath += HandleDeath;
            statsManager.OnPoiseBreak += HandlePoiseBreak;

            if (playerTransform != null)
            {
                MovementManager.SetTarget(playerTransform);
            }

            // Запобігаємо NullReferenceException, якщо масив не призначено
            bool hasWaypoints = patrolWaypoints != null && patrolWaypoints.Length > 0;
            _stateMachine.Initialize(patrolingEnemy && hasWaypoints ? PatrolState : IdleState);
        }

        private void OnDisable()
        {
            if (statsManager != null)
                statsManager.OnDeath -= HandleDeath;
        }

        private void Update()
        {
            if (_isDead) return;

            UpdateSensors();
            _stateMachine.Update();
        }

        private void FixedUpdate()
        {
            if (_isDead) return;
            _stateMachine.FixedUpdate();
        }

#endregion

#region Last Known Position Methods
        private void UpdateSensors()
        {
            if (IsPlayerInDetectionRange())
            {
                HasLastKnownPosition = true;
                LastKnownPlayerPosition = playerTransform.position;
            }
        }

        public bool HasReachedLastKnownPosition()
        {
            if (!HasLastKnownPosition) return true;
            return MovementManager.ReachedDestination();
        }

        public void ClearLastKnownPosition()
        {
            HasLastKnownPosition = false;
        }

#endregion

#region Player Detection
        public bool IsPlayerInDetectionRange()
        {
            if (playerTransform == null) return false;
            
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            
            // 1. ФАЗА ПЕРША: Перевірка дистанції
            if (directionToPlayer.sqrMagnitude > _sqrDetectionRange) return false;

            // 2. ФАЗА ДРУГА: Перевірка кута зору (Field of View)
            // Ми беремо кут між тим, куди дивиться ворог (transform.forward) і вектором до гравця
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if (angleToPlayer > fieldOfViewAngle / 2f) return false; // Ділимо на 2, бо кут розходиться в обидві сторони від центру

            // 3. ФАЗА ТРЕТЯ: Line of Sight (Перешкоди)
            // Raycast кидаємо на дистанцію до гравця (а не на максимальний detectionRange)
            // і ТІЛЬКИ по шару obstacleLayer.
            float distanceToPlayer = directionToPlayer.magnitude;
            Vector3 rayStartOffset = transform.position + Vector3.up; // рівень грудей/очей
            
            if (Physics.Raycast(rayStartOffset, directionToPlayer.normalized, out RaycastHit hit, distanceToPlayer, obstacleLayer))
            {
                // Якщо ми влучили в перешкоду на шляху до гравця - стіна перекриває зір. Гравець у безпеці.
                return false;
            }

            // Якщо ми пройшли всі три фази — ми легально "бачимо" гравця.
            return true;
        }

        public bool IsPlayerInAttackRange()
        {
            if (playerTransform == null) return false;
            return (transform.position - playerTransform.position).sqrMagnitude <= _sqrAttackRange;
        }

#endregion

        private void HandleDeath()
        {
            if (_isDead) return;
            _isDead = true;
            
            MovementManager.Stop();
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Animator.Play(Animator.StringToHash("Death")); 
        }

        private void HandlePoiseBreak()
        {
            if (_isDead) return;
            _stateMachine.ChangeState(StaggeredState);
        }

        

#region Gizmos

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
#endregion
    }
}