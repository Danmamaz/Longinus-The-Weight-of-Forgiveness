using UnityEngine;

namespace Longinus.Player
{
    /// <summary>
    /// Manages transitions and execution of player states.
    /// </summary>
    public class PlayerStateMachine
    {
        #region Properties
        
        public PlayerBaseState CurrentState { get; private set; }
        
        #endregion

        #region State/Core Logic

        /// <summary>
        /// Initializes the state machine with a starting state.
        /// </summary>
        public void Initialize(PlayerBaseState startingState)
        {
            if (startingState == null)
            {
                Debug.LogError("[PlayerStateMachine] Initialized with a null state.");
                return;
            }
            
            CurrentState = startingState;
            CurrentState.EnterState();
        }

        /// <summary>
        /// Exits the current state and enters the new provided state.
        /// </summary>
        public void ChangeState(PlayerBaseState newState)
        {
            if (newState == null || CurrentState == newState) return;

            CurrentState.ExitState();
            CurrentState = newState;
            CurrentState.EnterState();
        }
        
        #endregion
    }

    /// <summary>
    /// Abstract base class for all player states.
    /// </summary>
    public abstract class PlayerBaseState
    {
        #region Protected Variables
        
        protected readonly PlayerController _ctx;
        protected readonly PlayerStateMachine _stateMachine;
        
        #endregion

        #region Initialization
        
        protected PlayerBaseState(PlayerController ctx, PlayerStateMachine stateMachine)
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
        public abstract void CheckSwitchStates();
        
        #endregion
    }

    /// <summary>
    /// Handles standard player locomotion and idle behaviors.
    /// </summary>
    public class PlayerMoveState : PlayerBaseState
    {
        #region Private Variables
        
        private readonly int _animIsMovingHash = Animator.StringToHash("IsMoving");
        
        #endregion

        public PlayerMoveState(PlayerController ctx, PlayerStateMachine stateMachine) : base(ctx, stateMachine) { }

        #region State/Core Logic

        public override void EnterState() 
        {
            _ctx.Animator.SetBool(_animIsMovingHash, _ctx.IsMoving);
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
            _ctx.Animator.SetBool(_animIsMovingHash, _ctx.IsMoving);
            
            if (_ctx.IsMoving)
            {
                _ctx.Locomotion.HandleRotation(_ctx.MoveInput, _ctx.RotationSpeed);
            }
        }

        public override void FixedUpdateState()
        {
            _ctx.Locomotion.HandleMovement(_ctx.MoveInput, _ctx.MoveSpeed);
        }

        public override void ExitState() 
        {
            _ctx.Animator.SetBool(_animIsMovingHash, false);
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
        
        #endregion
    }

    /// <summary>
    /// Handles the dodge/roll mechanic, including root motion approximation and directional snapping.
    /// </summary>
    public class PlayerRollState : PlayerBaseState
    {
        #region Private Variables
        
        private readonly int _animRollHash = Animator.StringToHash("Roll");
        private float _timer;
        private Vector3 _rollDirection;
        
        #endregion

        public PlayerRollState(PlayerController ctx, PlayerStateMachine stateMachine) : base(ctx, stateMachine) { }

        #region State/Core Logic

        public override void EnterState()
        {
            _timer = 0f;
            _ctx.ResetRollTrigger();
        
            if (_ctx.MoveInput.sqrMagnitude > 0.01f)
            {
                _rollDirection = _ctx.MoveInput.normalized;
                _ctx.Locomotion.HandleRotation(_rollDirection, 2000f); // Instant snap rotation for responsive dodging
            }
            else
            {
                _rollDirection = _ctx.transform.forward;
            }

            _ctx.Animator.SetTrigger(_animRollHash);
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
            if (_timer >= _ctx.RollDuration) 
            {
                _stateMachine.ChangeState(_ctx.MoveState);
            }
        }
        
        #endregion
    }

    /// <summary>
    /// Halts movement and transitions into combat animation logic.
    /// </summary>
    public class PlayerAttackState : PlayerBaseState
    {
        public PlayerAttackState(PlayerController ctx, PlayerStateMachine stateMachine) : base(ctx, stateMachine) { }

        #region State/Core Logic

        public override void EnterState()
        {
            _ctx.Locomotion.StopMovement();
            
            bool started = _ctx.CombatManager.AttemptAttack(); 
            
            _ctx.ResetAttackTriggers();

            if (!started) 
            {
                _stateMachine.ChangeState(_ctx.MoveState);
            }
        }

        public override void UpdateState()
        {
            if (_ctx.AttackTriggered)
            {
                _ctx.CombatManager.AttemptAttack();
                
                _ctx.ResetAttackTriggers(); 
            }

            if (!_ctx.CombatManager.IsAttacking) 
            {
                CheckSwitchStates();
            }
        }

        public override void FixedUpdateState() { }
        public override void ExitState() { }

        public override void CheckSwitchStates()
        {
            if (_ctx.RollTriggered) 
            {
                _stateMachine.ChangeState(_ctx.RollState);
            }
            else 
            {
                _stateMachine.ChangeState(_ctx.MoveState);
            }
        }
        
        #endregion
    }
    public class PlayerInteractState : PlayerBaseState
    {
        #region Private Variables
        
        private bool _isInteracting;
        
        #endregion

        public PlayerInteractState(PlayerController ctx, PlayerStateMachine stateMachine) : base(ctx, stateMachine) { }

        #region State/Core Logic

        public override void EnterState()
        {
            _ctx.Locomotion.StopMovement();
            
            _ctx.ResetAttackTriggers();
            _ctx.ResetRollTrigger();
            
            _isInteracting = true;

            // Updated to use the correct API from our previously refactored InteractionSystem
            _ctx.InteractionSystem.InteractWithClosestObject();
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
        }

        public override void FixedUpdateState() { }

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

            // Allows movement input to cancel the interaction state fallback
            if (_ctx.MoveInput.sqrMagnitude > 0.1f)
            {
                _stateMachine.ChangeState(_ctx.MoveState);
            }
        }
        
        #endregion
    }
}