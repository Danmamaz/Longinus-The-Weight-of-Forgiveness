using UnityEngine;

public class PlayerStateMachine
{
    public PlayerBaseState CurrentState { get; private set; }

    public void Initialize(PlayerBaseState startingState)
    {
        CurrentState = startingState;
        CurrentState.EnterState();
    }

    public void ChangeState(PlayerBaseState newState)
    {
        CurrentState.ExitState();
        CurrentState = newState;
        CurrentState.EnterState();
    }
}

public abstract class PlayerBaseState
{
    protected PlayerController _ctx;
    protected PlayerStateMachine _stateMachine;

    public PlayerBaseState(PlayerController ctx, PlayerStateMachine factory)
    {
        _ctx = ctx;
        _stateMachine = factory;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void FixedUpdateState();
    public abstract void ExitState();
    public abstract void CheckSwitchStates();
}

public class PlayerMoveState : PlayerBaseState
{
    public PlayerMoveState(PlayerController ctx, PlayerStateMachine factory) : base(ctx, factory) { }

    public override void EnterState() 
    {
        _ctx.Animator.SetBool("IsMoving", _ctx.IsMoving);
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
        _ctx.Animator.SetBool("IsMoving", _ctx.IsMoving);
        
        if (_ctx.IsMoving)
            _ctx.Locomotion.HandleRotation(_ctx.MoveInput, _ctx.RotationSpeed);
    }

    public override void FixedUpdateState()
    {
        _ctx.Locomotion.HandleMovement(_ctx.MoveInput, _ctx.MoveSpeed);
    }

    public override void ExitState() 
    {
        _ctx.Animator.SetBool("IsMoving", false);
    }

    public override void CheckSwitchStates()
    {
        if (_ctx.AttackTriggered)
        {
            _stateMachine.ChangeState(_ctx.AttackState);
            return;
        }

        if (_ctx.RollTriggered) 
        {
            if (_ctx.Stats.TryConsumeStamina(15f))
            {
                _stateMachine.ChangeState(_ctx.RollState);
            }
            else
            {
                _ctx.ResetRollTrigger();
            }
        }
    }
}

public class PlayerRollState : PlayerBaseState
{
    private float _timer;
    private Vector3 _rollDirection;

    public PlayerRollState(PlayerController ctx, PlayerStateMachine factory) : base(ctx, factory) { }

    public override void EnterState()
    {
        _timer = 0f;
        _ctx.ResetRollTrigger();
    
        if (_ctx.MoveInput.sqrMagnitude > 0.01f)
        {
            _rollDirection = _ctx.MoveInput.normalized;
        
            _ctx.Locomotion.HandleRotation(_rollDirection, 2000f);
        }
        else
        {
            _rollDirection = _ctx.transform.forward;
        }

        _ctx.Animator.SetTrigger("Roll");
    }

    public override void UpdateState()
    {
        _timer += Time.deltaTime;
        CheckSwitchStates();
    }

    public override void FixedUpdateState()
    {
        float normalizedTime = _timer / _ctx.RollDuration;
        float speedMultiplier = _ctx.RollSpeedCurve.Evaluate(normalizedTime);
        
        Vector3 rollVelocity = _rollDirection * (speedMultiplier * _ctx.RollDistanceMult);
        _ctx.Locomotion.SetVelocity(rollVelocity);
    }

    public override void ExitState()
    {
        _ctx.Locomotion.StopMovement();
    }

    public override void CheckSwitchStates()
    {
        if (_timer >= _ctx.RollDuration) _stateMachine.ChangeState(_ctx.MoveState);
    }
}

public class PlayerAttackState : PlayerBaseState
{
    public PlayerAttackState(PlayerController ctx, PlayerStateMachine factory) : base(ctx, factory) { }

    public override void EnterState()
    {
        _ctx.Locomotion.StopMovement();
        
        bool isHeavy = _ctx.HeavyAttackTriggered; 
        bool started = _ctx.CombatManager.AttemptAttack(isHeavy);
        
        _ctx.ResetAttackTriggers();

        if (!started) _stateMachine.ChangeState(_ctx.MoveState);
    }

    public override void UpdateState()
    {
        if (!_ctx.CombatManager.IsAttacking) CheckSwitchStates();
    }

    public override void FixedUpdateState() { }
    public override void ExitState() { }

    public override void CheckSwitchStates()
    {
        if (_ctx.RollTriggered) _stateMachine.ChangeState(_ctx.RollState);
        else _stateMachine.ChangeState(_ctx.MoveState);
    }
}

public class PlayerInteractState : PlayerBaseState
{
    private bool _isInteracting;

    public PlayerInteractState(PlayerController ctx, PlayerStateMachine factory) : base(ctx, factory) { }

    public override void EnterState()
    {
        _ctx.Locomotion.StopMovement();
        
        _ctx.ResetAttackTriggers();
        _ctx.ResetRollTrigger();
        
        _isInteracting = true;

        _ctx.InteractionSystem.InteractWithSelectedObject();
        
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void FixedUpdateState()
    {
        
    }

    public override void ExitState()
    {
        _isInteracting = false; 
    }

    public override void CheckSwitchStates()
    {
        
        if (_ctx.RollTriggered)
        {
             _stateMachine.ChangeState(_ctx.RollState);
             return;
        }

        //AnimatorStateInfo stateInfo = _ctx.Animator.GetCurrentAnimatorStateInfo(0);
        
        //if (!_ctx.Animator.GetCurrentAnimatorStateInfo(0).IsName("Interact") && !_isInteracting)
        //{
        //     _stateMachine.ChangeState(_ctx.MoveState);
        //}
        
        if (_ctx.MoveInput.sqrMagnitude > 0.1f)
        {
            _stateMachine.ChangeState(_ctx.MoveState);
        }
    }
}