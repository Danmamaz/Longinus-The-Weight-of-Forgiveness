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
                _stateMachine.ChangeState(_ctx.CombatStrafeState);
            }
            else if (!_ctx.IsPlayerInDetectionRange())
            {
                _stateMachine.ChangeState(_ctx.SearchState);
            }
        }
    }

    public class EnemyAttackState : EnemyBaseState
{
    public enum AttackPhase { WindUp, Active, Recovery }
    public AttackPhase CurrentPhase { get; private set; }

    private readonly int _animLightAttack = Animator.StringToHash("LightAttack");
    private readonly int _animHeavyAttack = Animator.StringToHash("HeavyAttack");

    private bool _attackFinished;

    public EnemyAttackState(EnemyController ctx, EnemyStateMachine stateMachine)
        : base(ctx, stateMachine) { }

    public override void EnterState()
    {
        _attackFinished = false;
        CurrentPhase = AttackPhase.WindUp;

        _ctx.MovementManager.Stop();

        // Обираємо атаку (поки що рандом, потім замінити на логіку)
        bool heavy = UnityEngine.Random.value > 0.7f;
        _ctx.Animator.Play(heavy ? _animHeavyAttack : _animLightAttack);
    }

    public override void UpdateState()
    {
        // Wind-up: ворог повільно довертається до гравця
        if (CurrentPhase == AttackPhase.WindUp)
        {
            _ctx.MovementManager.FaceTarget();
        }
        // Active & Recovery: rotation locked, нічого не робимо
    }

    public override void FixedUpdateState() { }

    public override void ExitState()
    {
        _ctx.MovementManager.UnlockRotation();
    }

    public override void CheckSwitchState()
    {
        if (!_attackFinished) return;

        if (_ctx.IsPlayerInAttackRange())
        {
            // Можна переходити в CombatStrafing замість повторної атаки
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

    // --- Викликаються через Animation Events ---
    public void OnWindUpEnd()
    {
        CurrentPhase = AttackPhase.Active;
        _ctx.MovementManager.LockRotation();
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
    public class EnemyCombatStrafeState : EnemyBaseState
{
    private readonly int _animStrafeHash = Animator.StringToHash("CombatStrafe");

    private float _strafeTimer;
    private float _strafeDuration;
    private float _strafeDirection; // -1 або +1

    public EnemyCombatStrafeState(EnemyController ctx, EnemyStateMachine stateMachine)
        : base(ctx, stateMachine) { }

    public override void EnterState()
    {
        _ctx.Animator.Play(_animStrafeHash);
        _ctx.MovementManager.Stop();

        _strafeDuration = UnityEngine.Random.Range(0.8f, 2.0f);
        _strafeTimer = 0f;
        _strafeDirection = UnityEngine.Random.value > 0.5f ? 1f : -1f;
    }

    public override void UpdateState()
    {
        _strafeTimer += Time.deltaTime;

        // Повертаємось обличчям до гравця
        _ctx.MovementManager.FaceTarget();

        // Рух вбік відносно напрямку на гравця
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

        if (_strafeTimer >= _strafeDuration && _ctx.IsPlayerInAttackRange())
        {
            _stateMachine.ChangeState(_ctx.AttackState);
        }
        else if (!_ctx.IsPlayerInAttackRange())
        {
            _stateMachine.ChangeState(_ctx.ChaseState);
        }
    }
}

public class EnemyStaggeredState : EnemyBaseState
{
    private readonly int _animStagger = Animator.StringToHash("Stagger");
    private bool _finished;

    public EnemyStaggeredState(EnemyController ctx, EnemyStateMachine stateMachine)
        : base(ctx, stateMachine) { }

    public override void EnterState()
    {
        _finished = false;
        _ctx.MovementManager.Stop();
        _ctx.MovementManager.UnlockRotation();
        _ctx.Animator.Play(_animStagger);
    }

    public override void UpdateState() { }
    public override void FixedUpdateState() { }
    public override void ExitState() { }

    public override void CheckSwitchState()
    {
        if (_finished)
            _stateMachine.ChangeState(_ctx.CombatStrafeState);
    }

    public void OnStaggerFinished() => _finished = true;
}


}

