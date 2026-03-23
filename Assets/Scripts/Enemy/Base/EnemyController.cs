using UnityEngine;

namespace Longinus.EnemySystem
{
    /// <summary>
    /// Core controller for enemy AI. Manages the state machine, sensors, and bridges data between movement and stats.
    /// </summary>
    [RequireComponent(typeof(Animator), typeof(EnemyMovementManager), typeof(EnemyStatsManager))]
    public class EnemyController : MonoBehaviour
    {
        #region Constants & Inspector Variables
        
        [Header("Core References")]
        [SerializeField, Tooltip("Reference to the player's transform.")] 
        private Transform _playerTransform;
        
        [SerializeField, Tooltip("Layer masking environment obstacles that block line of sight.")] 
        private LayerMask _obstacleLayer;

        [SerializeField, Tooltip("Specific colliders (Hurtboxes/Hitboxes) to disable upon death to prevent camera or trigger interference.")]
        private Collider[] _bodyColliders;

        [Header("AI Sensors & Settings")]
        [SerializeField, Tooltip("Maximum distance the enemy can detect the player.")] 
        private float _detectionRange = 10f;
        
        [SerializeField, Tooltip("Vision cone angle in degrees.")] 
        private float _fieldOfViewAngle = 120f;
        
        [SerializeField, Tooltip("Distance threshold to transition into attack state.")] 
        private float _attackRange = 2f;
        
        [Header("Patrol Settings")]
        public Transform[] PatrolWaypoints;
        public bool IsPatrollingEnemy;

        [Header("Boss Choice")]
        [SerializeField, Tooltip("Unique identifier for PlotManager to track decisions related to this entity.")] 
        private string _decisionId = "boss_01";
        
        #endregion

        #region Private Variables
        
        private EnemyStateMachine _stateMachine;
        private bool _isDead;
        private float _sqrDetectionRange;
        private float _sqrAttackRange;
        
        #endregion

        #region Public Properties

        public Animator Animator { get; private set; }
        public EnemyMovementManager MovementManager { get; private set; }
        public EnemyStatsManager StatsManager { get; private set; }

        public EnemyIdleState IdleState { get; private set; }
        public EnemyChaseState ChaseState { get; private set; }
        public EnemyAttackState AttackState { get; private set; }
        public EnemyPatrolState PatrolState { get; private set; }
        public EnemySearchState SearchState { get; private set; }
        public EnemyCombatStrafeState CombatStrafeState { get; private set; }
        public EnemyStaggeredState StaggeredState { get; private set; }
        public EnemyBossDeathChoiceState BossDeathChoiceState { get; private set; }
        public EnemyDeadState DeadState { get; private set; } // Added to enforce hard stop

        public bool HasLastKnownPosition { get; private set; }
        public Vector3 LastKnownPlayerPosition { get; private set; }
        public Transform PlayerTransform => _playerTransform;
        public string DecisionId => _decisionId;
        
        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            Animator = GetComponent<Animator>();
            MovementManager = GetComponent<EnemyMovementManager>();
            StatsManager = GetComponent<EnemyStatsManager>();

            _sqrDetectionRange = _detectionRange * _detectionRange;
            _sqrAttackRange = _attackRange * _attackRange;

            _stateMachine = new EnemyStateMachine();
            IdleState = new EnemyIdleState(this, _stateMachine);
            ChaseState = new EnemyChaseState(this, _stateMachine);
            AttackState = new EnemyAttackState(this, _stateMachine);
            PatrolState = new EnemyPatrolState(this, _stateMachine);
            SearchState = new EnemySearchState(this, _stateMachine);
            CombatStrafeState = new EnemyCombatStrafeState(this, _stateMachine);
            StaggeredState = new EnemyStaggeredState(this, _stateMachine);
            BossDeathChoiceState = new EnemyBossDeathChoiceState(this, _stateMachine);
            DeadState = new EnemyDeadState(this, _stateMachine);
        }

        private void OnEnable()
        {
            _isDead = false;
            ClearLastKnownPosition();
            
            StatsManager.OnDeath += HandleDeath;
            StatsManager.OnPoiseBreak += HandlePoiseBreak;
            StatsManager.OnSpareableDeath += HandleSpareableDeath;

            if (_playerTransform != null)
            {
                MovementManager.SetTarget(_playerTransform);
            }

            bool hasWaypoints = PatrolWaypoints != null && PatrolWaypoints.Length > 0;
            _stateMachine.Initialize(IsPatrollingEnemy && hasWaypoints ? PatrolState : IdleState);
        }

        private void OnDisable()
        {
            if (StatsManager != null)
            {
                StatsManager.OnDeath -= HandleDeath;
                StatsManager.OnPoiseBreak -= HandlePoiseBreak;
                StatsManager.OnSpareableDeath -= HandleSpareableDeath;
            }
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

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _detectionRange);
            
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
#endif

        #endregion

        #region State/Core Logic
        
        /// <summary>
        /// Updates environmental sensors to track the player's last known position.
        /// </summary>
        private void UpdateSensors()
        {
            if (IsPlayerInDetectionRange())
            {
                HasLastKnownPosition = true;
                LastKnownPlayerPosition = _playerTransform.position;
            }
        }

        /// <summary>
        /// Checks if the enemy has reached the last coordinate where the player was seen.
        /// </summary>
        public bool HasReachedLastKnownPosition()
        {
            if (!HasLastKnownPosition) return true;
            return MovementManager.ReachedDestination();
        }

        /// <summary>
        /// Clears the cached player position data.
        /// </summary>
        public void ClearLastKnownPosition()
        {
            HasLastKnownPosition = false;
        }

        /// <summary>
        /// Evaluates distance, field of view, and line of sight to determine if the player is visible.
        /// </summary>
        public bool IsPlayerInDetectionRange()
        {
            if (_playerTransform == null) return false;
            
            Vector3 directionToPlayer = _playerTransform.position - transform.position;
            
            if (directionToPlayer.sqrMagnitude > _sqrDetectionRange) return false;

            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if (angleToPlayer > _fieldOfViewAngle / 2f) return false;

            float distanceToPlayer = directionToPlayer.magnitude;
            Vector3 rayStartOffset = transform.position + Vector3.up; 
            
            if (Physics.Raycast(rayStartOffset, directionToPlayer.normalized, out RaycastHit hit, distanceToPlayer, _obstacleLayer))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the player is close enough to trigger an attack sequence.
        /// </summary>
        public bool IsPlayerInAttackRange()
        {
            if (_playerTransform == null) return false;
            return (transform.position - _playerTransform.position).sqrMagnitude <= _sqrAttackRange;
        }

        /// <summary>
        /// Safely disables predefined body colliders to prevent interference post-mortem.
        /// </summary>
        public void DisableColliders()
        {
            if (_bodyColliders == null || _bodyColliders.Length == 0)
            {
                Debug.LogWarning($"[EnemyController] No body colliders assigned to {_decisionId}. Falling back to main collider.");
                Collider col = GetComponent<Collider>();
                if (col != null) col.enabled = false;
                return;
            }

            foreach (var col in _bodyColliders)
            {
                if (col != null) col.enabled = false;
            }
        }

        #endregion

        #region Event Listeners/Callbacks

        /// <summary>
        /// Handles irreversible death sequence and state transition.
        /// </summary>
        private void HandleDeath()
        {
            if (_isDead) return;
            _isDead = true;
            
            MovementManager.Stop();
            DisableColliders();

            _stateMachine.ChangeState(DeadState);
            Animator.Play(Animator.StringToHash("Death")); 
        }

        /// <summary>
        /// Interrupts current action and forces stagger state.
        /// </summary>
        private void HandlePoiseBreak()
        {
            if (_isDead) return;
            _stateMachine.ChangeState(StaggeredState);
        }

        /// <summary>
        /// Triggers the interactive boss death choice phase.
        /// </summary>
        private void HandleSpareableDeath()
        {
            if (_isDead) return;
            _stateMachine.ChangeState(BossDeathChoiceState);
        }

        #endregion
    }
}