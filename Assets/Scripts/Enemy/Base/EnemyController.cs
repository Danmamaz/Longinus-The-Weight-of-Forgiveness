using UnityEngine;
using System.Collections;

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

        [SerializeField, Tooltip("Multiplier for attack range to exit combat states. Prevents state flickering.")] 
        private float _exitAttackRangeMultiplier = 1.2f;
        private float _sqrExitAttackRange;
        
        [SerializeField, Tooltip("Vision cone angle in degrees.")] 
        private float _fieldOfViewAngle = 120f;
        
        [SerializeField, Tooltip("Distance threshold to transition into attack state.")] 
        private float _attackRange = 2f;

        [SerializeField, Tooltip("Radius where enemy hears the player regardless of FOV/Obstacles")]
        private float _hearingRange = 4f; 
        private float _sqrHearingRange;
        
        [Header("Patrol Settings")]
        public Transform[] PatrolWaypoints;
        public bool IsPatrollingEnemy;

        [Header("Combat Feel")]
        [SerializeField, Tooltip("Force of the hardcoded knockback")]
        private float _knockbackForce = 10f;
        
        #endregion

        #region Private Variables
        
        private EnemyStateMachine _stateMachine;
        private bool _isDead;
        private float _sqrDetectionRange;
        private float _sqrAttackRange;
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private Coroutine _knockbackCoroutine;
        public static System.Collections.Generic.List<EnemyController> AllEnemies = new();
        
        #endregion

        #region Public Properties

        public Animator Animator { get; private set; }
        public EnemyMovementManager MovementManager { get; private set; }
        public EnemyStatsManager StatsManager { get; private set; }

        public EnemyStateMachine StateMachine => _stateMachine;

        public EnemyIdleState IdleState { get; private set; }
        public EnemyChaseState ChaseState { get; private set; }
        public EnemyAttackState AttackState { get; private set; }
        public EnemyPatrolState PatrolState { get; private set; }
        public EnemySearchState SearchState { get; private set; }
        public EnemyCombatStrafeState CombatStrafeState { get; private set; }
        public EnemyStaggeredState StaggeredState { get; private set; }
        public EnemyBossDeathChoiceState BossDeathChoiceState { get; private set; }
        public EnemyDeadState DeadState { get; private set; }
        public EnemyKiteState KiteState { get; private set; }
        public EnemyShootState ShootState { get; private set; }
        public BossPhaseTransitionState PhaseTransitionState { get; private set; }

        public bool HasLastKnownPosition { get; private set; }
        public Vector3 LastKnownPlayerPosition { get; private set; }
        public Transform PlayerTransform => _playerTransform;
        
        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            Animator = GetComponent<Animator>();
            MovementManager = GetComponent<EnemyMovementManager>();
            StatsManager = GetComponent<EnemyStatsManager>();

            _sqrDetectionRange = _detectionRange * _detectionRange;
            _sqrAttackRange = _attackRange * _attackRange;
            _sqrHearingRange = _hearingRange * _hearingRange;

            float exitRange = _attackRange * _exitAttackRangeMultiplier;
            _sqrExitAttackRange = exitRange * exitRange;

            _startPosition = transform.position;
            _startRotation = transform.rotation;

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

        private void Start()
        {
            if (_playerTransform != null)
            {
                MovementManager.SetTarget(_playerTransform);
            }

            bool hasWaypoints = PatrolWaypoints != null && PatrolWaypoints.Length > 0;
            _stateMachine.Initialize(IsPatrollingEnemy && hasWaypoints ? PatrolState : IdleState);
        }

        private void OnEnable()
        {
            _isDead = false;
            ClearLastKnownPosition();
            AllEnemies.Add(this);
            
            StatsManager.OnDeath += HandleDeath;
            StatsManager.OnPoiseBreak += HandlePoiseBreak;
            StatsManager.OnSpareableDeath += HandleSpareableDeath;
        }

        private void OnDisable()
        {
            AllEnemies.Remove(this);

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
        /// Checks if the player is far enough to exit the combat strafe/attack sequence (Hysteresis).
        /// </summary>
        public bool IsPlayerOutOfAttackExitRange()
        {
            if (_playerTransform == null) return true;

            return (transform.position - _playerTransform.position).sqrMagnitude > _sqrExitAttackRange;
        }

        /// <summary>
        /// Casts a ray to determine whether the enemy has an unobstructed line of sight to the target.
        /// </summary>
        public bool HasLineOfSight(Transform target)
        {
            if (target == null) return false;

            Vector3 directionToTarget = target.position - transform.position;
            float distanceToTarget = directionToTarget.magnitude;
            Vector3 rayStart = transform.position + Vector3.up;

            return !Physics.Raycast(rayStart, directionToTarget.normalized, distanceToTarget, _obstacleLayer);
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

            return HasLineOfSight(_playerTransform);
        }

        /// <summary>
        /// Checks if the player is close enough to trigger an attack sequence.
        /// </summary>
        public bool IsPlayerInAttackRange()
        {
            if (_playerTransform == null) return false;

            return (transform.position - _playerTransform.position).sqrMagnitude <= _sqrAttackRange;
        }

        public void Respawn()
        {
            _isDead = false;
            transform.position = _startPosition;
            transform.rotation = _startRotation;
            
            StatsManager.RestoreAll();
            _stateMachine.Initialize(IsPatrollingEnemy ? PatrolState : IdleState);
            
            foreach (var col in _bodyColliders) 
            {
                if (col != null) col.enabled = true;
            }
            
            Animator.SetBool("IsMoving", false);
            Animator.Play("Idle");
        }

        /// <summary>
        /// Hardcoded physical knockback execution.
        /// </summary>
        public void ApplyKnockback(Vector3 hitPoint)
        {
            if (_isDead) return;

            Vector3 direction = (transform.position - hitPoint).normalized;
            direction.y = 0;
            if (_knockbackCoroutine != null) StopCoroutine(_knockbackCoroutine);
            _knockbackCoroutine = StartCoroutine(KnockbackRoutine(direction, _knockbackForce));
        }

        private IEnumerator KnockbackRoutine(Vector3 direction, float force)
        {
            MovementManager.SetAgentActive(false);

            float duration = 0.15f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                transform.position += direction * force * Time.deltaTime;
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!_isDead)
            {
                MovementManager.SetAgentActive(true);
            }
        }

        /// <summary>
        /// Registers ranged combat states supplied by RangedEnemyController during its initialization.
        /// </summary>
        public void RegisterRangedStates(EnemyKiteState kiteState, EnemyShootState shootState)
        {
            KiteState = kiteState;
            ShootState = shootState;
        }

        /// <summary>
        /// Replaces the default <see cref="EnemyAttackState"/> with a boss-specific subclass.
        /// Called by <see cref="BossController"/> after all states are constructed.
        /// </summary>
        public void OverrideAttackState(EnemyAttackState newAttackState)
        {
            AttackState = newAttackState;
        }

        /// <summary>
        /// Registers the boss phase-transition state created by <see cref="BossController"/>.
        /// </summary>
        public void RegisterPhaseTransitionState(BossPhaseTransitionState state)
        {
            PhaseTransitionState = state;
        }

        /// <summary>
        /// If the player is within this radius, they are instantly detected.
        /// </summary>
        public bool IsPlayerHeard()
        {
            if (_playerTransform == null) return false;
            return (transform.position - _playerTransform.position).sqrMagnitude <= _sqrHearingRange;
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
            _stateMachine.ChangeState(DeadState);
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