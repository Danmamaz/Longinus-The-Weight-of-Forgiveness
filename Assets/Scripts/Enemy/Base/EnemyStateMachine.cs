using UnityEngine;

namespace Enemy.BaseEnemy
{
    public class EnemyStateMachine
    {
        public EnemyBaseState CurrentState { get; private set; }

        public void Initialize(EnemyBaseState startingState)
        {
            if (startingState == null)
            {
                Debug.LogError("[CRITICAL] State Machine initialized with null state.");
                return;
            }
            CurrentState = startingState;
            CurrentState.EnterState();
        }

        public void ChangeState(EnemyBaseState newState)
        {
            if (newState == null || CurrentState == newState) return;

            CurrentState.ExitState();
            CurrentState = newState;
            CurrentState.EnterState();
        }

        public void Update()
        {
            if (CurrentState == null) return;
            
            CurrentState.UpdateState();
            CurrentState.CheckSwitchState();
        }

        public void FixedUpdate()
        {
            CurrentState?.FixedUpdateState();
        }
    }

    public abstract class EnemyBaseState
    {
        protected readonly EnemyController _ctx;
        protected readonly EnemyStateMachine _stateMachine;

        protected EnemyBaseState(EnemyController ctx, EnemyStateMachine stateMachine)
        {
            _ctx = ctx;
            _stateMachine = stateMachine;
        }

        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void FixedUpdateState();
        public abstract void ExitState();
        public abstract void CheckSwitchState();
    }

    public class EnemyIdleState : EnemyBaseState
    {
        private readonly int _animIdleHash = Animator.StringToHash("Idle");

        public EnemyIdleState(EnemyController ctx, EnemyStateMachine stateMachine) : base(ctx, stateMachine) { }

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
            else if (_ctx.patrolingEnemy)
            {
                _stateMachine.ChangeState(_ctx.PatrolState);
            }
        }
    }

    public class EnemyChaseState : EnemyBaseState
    {
        private readonly int _animChaseHash = Animator.StringToHash("Chase");

        public EnemyChaseState(EnemyController ctx, EnemyStateMachine stateMachine) : base(ctx, stateMachine) { }

        public override void EnterState()
        {
            _ctx.MovementManager.Chase();
            _ctx.Animator.Play(_animChaseHash);
        }

        public override void UpdateState()
        {
            _ctx.MovementManager.Chase();
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            _ctx.MovementManager.Stop();
        }

        public override void CheckSwitchState()
        {
            if (_ctx.IsPlayerInAttackRange())
            {
                _stateMachine.ChangeState(_ctx.AttackState);
            }
            else if (!_ctx.IsPlayerInDetectionRange())
            {
                _stateMachine.ChangeState(_ctx.SearchState);
            }
        }
    }

    public class EnemyAttackState : EnemyBaseState
    {
        private readonly int _animAttackHash = Animator.StringToHash("Attack");

        public EnemyAttackState(EnemyController ctx, EnemyStateMachine stateMachine) : base(ctx, stateMachine) { }

        public override void EnterState()
        {
            _ctx.MovementManager.Stop();
            _ctx.Animator.Play(_animAttackHash);
        }

        public override void UpdateState() { }
        public override void FixedUpdateState() { }
        public override void ExitState() { }

        public override void CheckSwitchState()
        {
            if (_ctx.IsPlayerInDetectionRange() && !_ctx.IsPlayerInAttackRange())
            {
                _stateMachine.ChangeState(_ctx.ChaseState);
            }
            else if (!_ctx.IsPlayerInDetectionRange())
            {
                _stateMachine.ChangeState(_ctx.IdleState);
            }
        }
    }

    public class EnemyPatrolState : EnemyBaseState
    {
        private readonly int _animPatrolHash = Animator.StringToHash("Patrol");
        private int _currentWaypointIndex;

        public EnemyPatrolState(EnemyController ctx, EnemyStateMachine stateMachine) : base(ctx, stateMachine)
        {
            _currentWaypointIndex = 0;
        }

        public override void EnterState()
        {
            _ctx.Animator.Play(_animPatrolHash);
            MovetoNextWaypoint();
        }

        public override void UpdateState()
        {
            if (_ctx.patrolWaypoints == null || _ctx.patrolWaypoints.Length == 0) return;

            if (_ctx.MovementManager.ReachedDestination())
            {
                _currentWaypointIndex = (_currentWaypointIndex + 1) % _ctx.patrolWaypoints.Length;
                MovetoNextWaypoint();
            }
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            _ctx.MovementManager.Stop();
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

        private void MovetoNextWaypoint()
        {
            if (_ctx.patrolWaypoints != null && _ctx.patrolWaypoints.Length > 0)
            {
                _ctx.MovementManager.MoveTo(_ctx.patrolWaypoints[_currentWaypointIndex]);
            }
        }
    }

    public class EnemySearchState : EnemyBaseState
    {
        public EnemySearchState(EnemyController ctx, EnemyStateMachine stateMachine) : base(ctx, stateMachine) { }

        public override void EnterState()
        {
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
            if (_ctx.patrolingEnemy && _ctx.patrolWaypoints != null && _ctx.patrolWaypoints.Length > 0)
            {
                _stateMachine.ChangeState(_ctx.PatrolState);
            }
            else
            {
                _stateMachine.ChangeState(_ctx.IdleState);
            }
        }
    }
}