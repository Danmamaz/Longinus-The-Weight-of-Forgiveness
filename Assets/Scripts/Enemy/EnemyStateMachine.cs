using UnityEngine;


public class EnemyStateMachine
{
    public EnemyBaseState CurrentState { get; private set; }

    public void Initialize(EnemyBaseState startingState)
    {
        CurrentState = startingState;
        CurrentState.EnterState();

    }

    public void ChangeState(EnemyBaseState newState)
    {
        CurrentState.ExitState();
        CurrentState = newState;
        CurrentState.EnterState();
    }
}

public abstract class EnemyBaseState
{
    protected EnemyController _ctx;

    protected EnemyStateMachine _stateMachine;

    public EnemyBaseState(EnemyController ctx, EnemyStateMachine stateMachine)
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
    public EnemyIdleState(EnemyController ctx, EnemyStateMachine stateMachine) : base(ctx, stateMachine)
    {

    }

    public override void EnterState()
    {

        _ctx.Animator.Play("Idle");

    }

    public override void UpdateState()
    {

    }

    public override void FixedUpdateState()
    {

    }

    public override void ExitState()
    {

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
        else if (_ctx.patrolingEnemy)
        {
            _stateMachine.ChangeState(_ctx.PatrolState);
        }
    }
}

public class EnemyChaseState : EnemyBaseState
{
    public EnemyChaseState(EnemyController ctx, EnemyStateMachine stateMachine) : base(ctx, stateMachine) { }
    public override void EnterState()
    {
        // ? Check 

        _ctx.MovementManager.Chase();
        _ctx.Animator.Play("Chase");
    }

    public override void UpdateState()
    {
        _ctx.MovementManager.Chase();
    }

    public override void FixedUpdateState()
    {
        
    }

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
            _stateMachine.ChangeState(_ctx.IdleState);
        }
    }
}

public class EnemyAttackState : EnemyBaseState
{
    public EnemyAttackState(EnemyController ctx, EnemyStateMachine stateMachine) : base(ctx, stateMachine) { }
    public override void EnterState()
    {
        // ? Check
        _ctx.Animator.Play("Attack");

    }

    public override void UpdateState()
    {

    }

    public override void FixedUpdateState()
    {

    }

    public override void ExitState()
    {

    }

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
    private int _currentWaypointIndex;
    public EnemyPatrolState(EnemyController ctx, EnemyStateMachine stateMachine) : base(ctx, stateMachine)
    {
        _currentWaypointIndex = 0;
    }
    public override void EnterState()
    {
        _ctx.Animator.Play("Patrol");
        MovetoNextWaypoint();

    }

    public override void UpdateState()
    {

        if (_ctx.patrolWaypoints == null || _ctx.patrolWaypoints.Length == 0) return;

        // If reached current waypoint, move to the next one
        if (_ctx.MovementManager.ReachedDestination())
        {
            _currentWaypointIndex = (_currentWaypointIndex + 1) % _ctx.patrolWaypoints.Length;
            MovetoNextWaypoint();
        }

    }

    public override void FixedUpdateState()
    {

    }

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
