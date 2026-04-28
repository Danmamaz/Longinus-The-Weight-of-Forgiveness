using System;
using UnityEngine;
using Longinus.PlotSystem;

namespace Longinus.EnemySystem
{
    /// <summary>
    /// Manages transitions and execution of enemy states.
    /// </summary>
    public class EnemyStateMachine
    {
        #region Properties
        
        public EnemyBaseState CurrentState { get; private set; }
        
        #endregion

        #region State/Core Logic

        /// <summary>
        /// Initializes the state machine with a starting state.
        /// </summary>
        public void Initialize(EnemyBaseState startingState)
        {
            if (startingState == null)
            {
                Debug.LogError("[EnemyStateMachine] Initialized with a null state.");
                return;
            }
            CurrentState = startingState;
            CurrentState.EnterState();
        }

        /// <summary>
        /// Exits the current state and enters the new provided state.
        /// </summary>
        public void ChangeState(EnemyBaseState newState)
        {
            if (newState == null || CurrentState == newState) return;

            CurrentState.ExitState();
            CurrentState = newState;
            CurrentState.EnterState();
        }

        /// <summary>
        /// Executes the logic of the current state every frame.
        /// </summary>
        public void Update()
        {
            if (CurrentState == null) return;
            
            CurrentState.UpdateState();
            CurrentState.CheckSwitchState();
        }

        /// <summary>
        /// Executes physics-related logic of the current state.
        /// </summary>
        public void FixedUpdate()
        {
            CurrentState?.FixedUpdateState();
        }
        
        #endregion
    }

    /// <summary>
    /// Abstract base class for all enemy states.
    /// </summary>
    public abstract class EnemyBaseState
    {
        #region Protected Variables
        
        protected readonly EnemyController _ctx;
        protected readonly EnemyStateMachine _stateMachine;
        
        #endregion

        #region Initialization

        protected EnemyBaseState(EnemyController ctx, EnemyStateMachine stateMachine)
        {
            _ctx = ctx;
            _stateMachine = stateMachine;
        }
        
        #endregion

        #region State/Core Logic

        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void FixedUpdateState();
        public abstract void ExitState();
        public abstract void CheckSwitchState();
        
        #endregion
    }

    /// <summary>
    /// Default state where the enemy is stationary and scanning for the player.
    /// </summary>
    public class EnemyIdleState : EnemyBaseState
    {
        private readonly int _animIdleHash = Animator.StringToHash("Idle");

        public EnemyIdleState(EnemyController ctx, EnemyStateMachine stateMachine) : base(ctx, stateMachine) { }

        #region State/Core Logic

        public override void EnterState()
        {
            _ctx.Animator.Play(_animIdleHash);
            _ctx.MovementManager.Stop();
        }

        public override void UpdateState() { }
        public override void FixedUpdateState() { }
        public override void ExitState() { }

        public override void CheckSwitchState()
        {
            if (_ctx.IsPlayerInAttackRange())
            {
                _stateMachine.ChangeState(_ctx.AttackState);
            }
            else if (_ctx.IsPlayerInDetectionRange())
            {
                _stateMachine.ChangeState(_ctx.ChaseState);
            }
            else if (_ctx.IsPatrollingEnemy) // Note: Requires exposing IsPatrollingEnemy in EnemyController
            {
                _stateMachine.ChangeState(_ctx.PatrolState);
            }
        }
        
        #endregion
    }

    /// <summary>
    /// Aggressive pursuit state when the player is detected but out of attack range.
    /// </summary>
    public class EnemyChaseState : EnemyBaseState
    {
        public EnemyChaseState(EnemyController ctx, EnemyStateMachine stateMachine) : base(ctx, stateMachine) { }

        #region State/Core Logic

        public override void EnterState()
        {
            _ctx.MovementManager.Chase();
            _ctx.MovementManager.SetAgentActive(true);
            _ctx.Animator.SetBool("IsMoving", true);
            _ctx.Animator.CrossFadeInFixedTime("Walking", .2f);
        }

        public override void UpdateState()
        {
            _ctx.MovementManager.Chase();
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            _ctx.MovementManager.Stop();
            _ctx.Animator.SetBool("IsMoving", false);

        }

        public override void CheckSwitchState()
        {
            if (_ctx.IsPlayerInAttackRange())
            {
                _stateMachine.ChangeState(_ctx.CombatStrafeState);
            }
            else if (!_ctx.IsPlayerInDetectionRange())
            {
                _stateMachine.ChangeState(_ctx.SearchState);
            }
        }
        
        #endregion
    }

    /// <summary>
    /// Executes combat animations and manages rotation locking during active attack frames.
    /// </summary>
    public class EnemyAttackState : EnemyBaseState
    {
        public enum AttackPhase { WindUp, Active, Recovery }
        
        #region Private Variables
        
        private bool _attackFinished;
        
        #endregion

        public AttackPhase CurrentPhase { get; private set; }

        public EnemyAttackState(EnemyController ctx, EnemyStateMachine stateMachine) : base(ctx, stateMachine) { }

        #region State/Core Logic

        public override void EnterState()
        {
            _attackFinished = false;
            CurrentPhase = AttackPhase.WindUp;

            _ctx.MovementManager.Stop();
            
            _ctx.Animator.SetTrigger("LightAttack");
        }

        public override void UpdateState()
        {
            if (CurrentPhase == AttackPhase.WindUp)
            {
                _ctx.MovementManager.FaceTarget();
            }
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            _ctx.MovementManager.UnlockRotation();
            _ctx.MovementManager.SetAgentActive(true);
        }

        public override void CheckSwitchState()
        {
            if (!_attackFinished) return;

            if (_ctx.IsPlayerInAttackRange())
            {
                _stateMachine.ChangeState(_ctx.CombatStrafeState);
            }
            else if (_ctx.IsPlayerInDetectionRange())
            {
                _stateMachine.ChangeState(_ctx.ChaseState);
            }
            else
            {
                _stateMachine.ChangeState(_ctx.SearchState);
            }
        }
        
        #endregion

        #region Event Listeners/Callbacks

        public void OnWindUpEnd()
        {
            CurrentPhase = AttackPhase.Active;
            _ctx.MovementManager.LockRotation();
            _ctx.MovementManager.SetAgentActive(false);
        }

        public void OnActiveEnd()
        {
            CurrentPhase = AttackPhase.Recovery;
        }

        public void OnAttackFinished()
        {
            _attackFinished = true;
            _ctx.MovementManager.UnlockRotation();
        }
        
        #endregion
    }

    /// <summary>
    /// Cycles through predefined waypoints when idle.
    /// </summary>
    public class EnemyPatrolState : EnemyBaseState
    {
        private int _currentWaypointIndex;

        public EnemyPatrolState(EnemyController ctx, EnemyStateMachine stateMachine) : base(ctx, stateMachine)
        {
            _currentWaypointIndex = 0;
        }

        #region State/Core Logic

        public override void EnterState()
        {
            _ctx.Animator.SetBool("IsMoving", true);
            MoveToNextWaypoint();
        }

        public override void UpdateState()
        {
            if (_ctx.PatrolWaypoints == null || _ctx.PatrolWaypoints.Length == 0) return;

            if (_ctx.MovementManager.ReachedDestination())
            {
                _currentWaypointIndex = (_currentWaypointIndex + 1) % _ctx.PatrolWaypoints.Length;
                MoveToNextWaypoint();
            }
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            _ctx.MovementManager.Stop();
            _ctx.Animator.SetBool("IsMoving", true);
        }

        public override void CheckSwitchState()
        {
            if (_ctx.IsPlayerInAttackRange())
            {
                _stateMachine.ChangeState(_ctx.AttackState);
            }
            else if (_ctx.IsPlayerInDetectionRange())
            {
                _stateMachine.ChangeState(_ctx.ChaseState);
            }
        }

        private void MoveToNextWaypoint()
        {
            if (_ctx.PatrolWaypoints != null && _ctx.PatrolWaypoints.Length > 0)
            {
                _ctx.MovementManager.MoveTo(_ctx.PatrolWaypoints[_currentWaypointIndex]);
            }
        }
        
        #endregion
    }

    /// <summary>
    /// Investigates the last known position of the player before returning to idle/patrol.
    /// </summary>
    public class EnemySearchState : EnemyBaseState
    {
        public EnemySearchState(EnemyController ctx, EnemyStateMachine stateMachine) : base(ctx, stateMachine) { }

        #region State/Core Logic

        public override void EnterState()
        {
            _ctx.Animator.SetBool("IsWalking", true);
            
            if (_ctx.HasLastKnownPosition)
            {
                _ctx.MovementManager.MoveToPosition(_ctx.LastKnownPlayerPosition);
            }
        }

        public override void UpdateState() { }
        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            _ctx.MovementManager.Stop();
            _ctx.Animator.SetBool("IsWalking", false);
        }

        public override void CheckSwitchState()
        {
            if (_ctx.IsPlayerInDetectionRange())
            {
                _stateMachine.ChangeState(_ctx.ChaseState);
                return;
            }

            if (_ctx.HasReachedLastKnownPosition() || !_ctx.HasLastKnownPosition)
            {
                _ctx.ClearLastKnownPosition();
                TransitionToIdle();
            }
        }

        private void TransitionToIdle()
        {
            if (_ctx.IsPatrollingEnemy && _ctx.PatrolWaypoints != null && _ctx.PatrolWaypoints.Length > 0)
            {
                _stateMachine.ChangeState(_ctx.PatrolState);
            }
            else
            {
                _stateMachine.ChangeState(_ctx.IdleState);
            }
        }
        
        #endregion
    }

    /// <summary>
    /// Tactical repositioning state to maintain distance and orbit the player during combat.
    /// </summary>
    public class EnemyCombatStrafeState : EnemyBaseState
    {
        #region Private Variables
        
        private float _strafeTimer;
        private float _strafeDuration;
        private float _strafeDirection;
        
        #endregion

        public EnemyCombatStrafeState(EnemyController ctx, EnemyStateMachine stateMachine) : base(ctx, stateMachine) { }

        #region State/Core Logic

        public override void EnterState()
        {
            _ctx.Animator.SetTrigger("WindUp");
            _ctx.MovementManager.Stop();

            _strafeDuration = UnityEngine.Random.Range(0.8f, 2.0f);
            _strafeTimer = 0f;
            _strafeDirection = UnityEngine.Random.value > 0.5f ? 1f : -1f;
        }

        public override void UpdateState()
        {
            _strafeTimer += Time.deltaTime;

            _ctx.MovementManager.FaceTarget();

            Vector3 toPlayer = (_ctx.PlayerTransform.position - _ctx.transform.position).normalized;
            Vector3 strafeDir = Vector3.Cross(Vector3.up, toPlayer) * _strafeDirection;
            Vector3 targetPos = _ctx.transform.position + strafeDir * 2f;

            _ctx.MovementManager.MoveToPosition(targetPos);
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            _ctx.MovementManager.Stop();
        }

        public override void CheckSwitchState()
        {
            if (!_ctx.IsPlayerInDetectionRange())
            {
                _stateMachine.ChangeState(_ctx.SearchState);
                return;
            }

            if (_strafeTimer >= _strafeDuration)
            {
                if (!_ctx.IsPlayerOutOfAttackExitRange()) 
                {
                    _stateMachine.ChangeState(_ctx.AttackState);
                }
                else
                {
                    _stateMachine.ChangeState(_ctx.ChaseState);
                }
            }
        }
        
        
        #endregion
    }

    /// <summary>
    /// Interrupts current actions when poise is broken.
    /// </summary>
    public class EnemyStaggeredState : EnemyBaseState
    {
        private bool _isFinished;

        public EnemyStaggeredState(EnemyController ctx, EnemyStateMachine stateMachine) : base(ctx, stateMachine) { }

        #region State/Core Logic

        public override void EnterState()
        {
            _isFinished = false;
            _ctx.MovementManager.Stop();
            _ctx.MovementManager.UnlockRotation();
            _ctx.Animator.SetTrigger("Stagger");
        }

        public override void UpdateState() { }
        public override void FixedUpdateState() { }
        public override void ExitState() { }

        public override void CheckSwitchState()
        {
            if (_isFinished && !_ctx.StatsManager.IsSpareable)
            {
                _stateMachine.ChangeState(_ctx.CombatStrafeState);                
            }
            // Add spared transition here when implemented in StatsManager
        }   

        #endregion
        
        #region Event Listeners/Callbacks
        
        public void OnStaggerFinished() => _isFinished = true;
        
        #endregion
    }

    /// <summary>
    /// Handles the interactive sequence where the player can choose to kill or spare the boss.
    /// </summary>
    public class EnemyBossDeathChoiceState : EnemyBaseState
    {
        #region Constants & Private Variables
        
        private readonly int _animStaggerHash = Animator.StringToHash("Stagger");
        private const float MERCY_DURATION = 5f;
        
        private float _timer;
        private bool _choiceMade;
        
        #endregion

        public EnemyBossDeathChoiceState(EnemyController ctx, EnemyStateMachine stateMachine) : base(ctx, stateMachine) { }

        #region State/Core Logic

        public override void EnterState()
        {
            _choiceMade = false;
            _timer = 0f;

            _ctx.MovementManager.Stop();
            _ctx.Animator.Play(_animStaggerHash);

            _ctx.StatsManager.OnChoicePhaseDamaged += OnPlayerHit;
        }

        public override void UpdateState()
        {
            if (_choiceMade) return;

            _timer += Time.deltaTime;

            if (_timer >= MERCY_DURATION)
            {
                TriggerMercy();
            }
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            _ctx.StatsManager.OnChoicePhaseDamaged -= OnPlayerHit;
        }

        public override void CheckSwitchState() { }
        
        #endregion
        
        #region Event Listeners/Callbacks

        /// <summary>
        /// Executes the kill sequence if the player attacks the boss during the choice phase.
        /// </summary>
        private void OnPlayerHit()
        {
            if (_choiceMade) return;
            _choiceMade = true;

            _ctx.StatsManager.ExecuteFinalDeath(); 
        }

        /// <summary>
        /// Executes the spare sequence if the player waits out the timer.
        /// </summary>
        private void TriggerMercy()
        {
            if (_choiceMade) return;
            _choiceMade = true;

            // Hard-stop the state machine to prevent any lingering logic
            _stateMachine.ChangeState(_ctx.DeadState);
        }
        
        #endregion
    }

    /// <summary>
    /// Inert state that halts all logic permanently. Used when the enemy is killed or spared.
    /// </summary>
    public class EnemyDeadState : EnemyBaseState
    {
        public EnemyDeadState(EnemyController ctx, EnemyStateMachine stateMachine) : base(ctx, stateMachine) { }


        #region State/Core Logic

        public override void EnterState()
        {
            _ctx.MovementManager.Stop();
            _ctx.Animator.SetTrigger("Die");

        }

        public override void UpdateState() { }
        public override void FixedUpdateState() { }
        public override void ExitState() { }
        public override void CheckSwitchState() { }

        #endregion
    }
}