using UnityEngine;
using UnityEngine.InputSystem;
using Longinus.UI;
using Longinus.PlotSystem;
using Longinus.Save;
using Longinus.Visuals;

namespace Longinus.Player
{
    /// <summary>
    /// Main controller for the player character. Manages input handling, state machine, and component bridging.
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(PlayerCombatManager))]
    [RequireComponent(typeof(PlayerStatsManager))]
    [RequireComponent(typeof(PlayerLocomotion))]
    public class PlayerController : MonoBehaviour
    {
        #region Constants & Inspector Variables
        
        [Header("Input References")]
        [SerializeField, Tooltip("Reference to the movement input action.")] 
        private InputActionReference _moveActionRef;
        
        [SerializeField, Tooltip("Reference to the dodge/roll input action.")] 
        private InputActionReference _rollActionRef;
        
        [SerializeField, Tooltip("Reference to the attack input action.")] 
        private InputActionReference _attackActionRef;
        
        [SerializeField, Tooltip("Reference to the interaction input action.")] 
        private InputActionReference _interactActionRef;
        
        [SerializeField, Tooltip("Reference to the pause menu input action.")]
        private InputActionReference _pauseActionRef;

        [SerializeField, Tooltip("Reference to the lock-on toggle input action.")]
        private InputActionReference _lockOnActionRef;

        [SerializeField, Tooltip("Reference to the switch lock-on target left input action.")]
        private InputActionReference _switchTargetLeftRef;

        [SerializeField, Tooltip("Reference to the switch lock-on target right input action.")]
        private InputActionReference _switchTargetRightRef;

        [Header("Movement & Roll Stats")]
        [SerializeField] private float _moveSpeed = 6f;
        [SerializeField] private float _rotationSpeed = 15f;
        [SerializeField] private float _rollDuration = 0.8f;
        [SerializeField] private float _rollDistanceMult = 15f;
        [SerializeField] private AnimationCurve _rollSpeedCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 3.5f));
        [SerializeField] private float _rollStaminaCost = 15f;

        [Header("System References")]
        [SerializeField] private UIManager _uiManager;
        [SerializeField] private InteractionSystem _interactionSystem;

        #endregion

        #region Private Variables

        private PlayerStateMachine _stateMachine;
        private System.Action<InputAction.CallbackContext> _onSwitchTargetLeft;
        private System.Action<InputAction.CallbackContext> _onSwitchTargetRight;

        #endregion

        #region Public Properties
        public static PlayerController Instance { get; private set; }
        
        public float MoveSpeed => _moveSpeed;
        public float RotationSpeed => _rotationSpeed;
        public float RollDuration => _rollDuration;
        public float RollDistanceMult => _rollDistanceMult;
        public AnimationCurve RollSpeedCurve => _rollSpeedCurve;

        public Animator Animator { get; private set; }
        public PlayerCombatManager CombatManager { get; private set; }
        public PlayerStatsManager Stats { get; private set; }
        public PlayerLocomotion Locomotion { get; private set; }
        public LockOnSystem LockOnSystem { get; private set; }
        public InteractionSystem InteractionSystem => _interactionSystem;

        // State Machine States
        public PlayerStateMachine StateMachine { get; private set; }
        public PlayerMoveState MoveState { get; private set; }
        public PlayerRollState RollState { get; private set; }
        public PlayerAttackState AttackState { get; private set; }
        public PlayerDeadState DeadState { get; private set; }
        public PlayerRestingState RestingState { get; private set; }

        // Input States
        public Vector3 MoveInput { get; private set; }
        public bool IsMoving => MoveInput.sqrMagnitude > 0.001f;
        public bool RollTriggered { get; private set; }
        public bool AttackTriggered { get; private set; }
        public float RollStaminaCost => _rollStaminaCost;
        
        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            Instance = this;
            CombatManager = GetComponent<PlayerCombatManager>();
            Stats = GetComponent<PlayerStatsManager>();
            Locomotion = GetComponent<PlayerLocomotion>();
            LockOnSystem = GetComponent<LockOnSystem>();
            Animator = GetComponentInChildren<Animator>();

            _onSwitchTargetLeft = _ => LockOnSystem?.SwitchTarget(-1);
            _onSwitchTargetRight = _ => LockOnSystem?.SwitchTarget(+1);

            InitStateMachine();
        }

        private void OnEnable()
        {
            if (_moveActionRef == null) return;

            SetInputActionsState(true);

            _rollActionRef.action.performed += OnRollPerformed;
            _attackActionRef.action.performed += OnAttackPerformed;
            _interactActionRef.action.performed += OnInteractPerformed;
            _pauseActionRef.action.performed += OnPausePerformed;
            if (_lockOnActionRef != null)
                _lockOnActionRef.action.performed += OnLockOnPerformed;
            if (_switchTargetLeftRef != null)
                _switchTargetLeftRef.action.performed += _onSwitchTargetLeft;
            if (_switchTargetRightRef != null)
                _switchTargetRightRef.action.performed += _onSwitchTargetRight;

            Stats.OnDeath += HandleDeath;
        }

        private void Start()
        {
            _stateMachine.Initialize(MoveState);

            if (SaveSystem.LoadState(PlotManager.Instance.PlotState, Stats, out Vector3 loadedPosition, out int loadedLevelIndex))
            {
                Locomotion.GetComponent<Rigidbody>().position = loadedPosition;
                transform.position = loadedPosition;
            }
        }

        private void Update()
        {
            ReadInput();
            _stateMachine.CurrentState?.UpdateState();
        }

        private void FixedUpdate()
        {
            _stateMachine.CurrentState?.FixedUpdateState();
        }

        private void OnDisable()
        {
            if (_moveActionRef == null) return;

            _rollActionRef.action.performed -= OnRollPerformed;
            _attackActionRef.action.performed -= OnAttackPerformed;
            _interactActionRef.action.performed -= OnInteractPerformed;
            _pauseActionRef.action.performed -= OnPausePerformed;
            if (_lockOnActionRef != null)
                _lockOnActionRef.action.performed -= OnLockOnPerformed;
            if (_switchTargetLeftRef != null)
                _switchTargetLeftRef.action.performed -= _onSwitchTargetLeft;
            if (_switchTargetRightRef != null)
                _switchTargetRightRef.action.performed -= _onSwitchTargetRight;

            SetInputActionsState(false);

            Stats.OnDeath -= HandleDeath;
        }

        #endregion

        #region State/Core Logic

        private void InitStateMachine()
        {
            _stateMachine = new PlayerStateMachine();
            StateMachine = _stateMachine;
            MoveState = new PlayerMoveState(this, _stateMachine);
            RollState = new PlayerRollState(this, _stateMachine);
            AttackState = new PlayerAttackState(this, _stateMachine);
            DeadState = new PlayerDeadState(this, _stateMachine);
            RestingState = new PlayerRestingState(this, _stateMachine);
        }

        private void ReadInput()
        {
            if (_moveActionRef != null)
            {
                Vector2 input = _moveActionRef.action.ReadValue<Vector2>();
                MoveInput = new Vector3(input.x, 0f, input.y).normalized;
            }
        }

        public void ResetRollTrigger() => RollTriggered = false;

        public void ResetAttackTrigger() => AttackTriggered = false;

        public void SetInputActionsState(bool state)
        {
            if(state)
            {
                _moveActionRef.action.Enable();
                _rollActionRef.action.Enable();
                _attackActionRef.action.Enable();
                _interactActionRef.action.Enable();
                if (_lockOnActionRef != null) _lockOnActionRef.action.Enable();
                if (_switchTargetLeftRef != null) _switchTargetLeftRef.action.Enable();
                if (_switchTargetRightRef != null) _switchTargetRightRef.action.Enable();
            }
            else
            {
                _moveActionRef.action.Disable();
                _rollActionRef.action.Disable();
                _attackActionRef.action.Disable();
                _interactActionRef.action.Disable();
                if (_lockOnActionRef != null) _lockOnActionRef.action.Disable();
                if (_switchTargetLeftRef != null) _switchTargetLeftRef.action.Disable();
                if (_switchTargetRightRef != null) _switchTargetRightRef.action.Disable();
            }
        }

        private void HandleDeath()
        {
            if (_stateMachine.CurrentState != DeadState)
            {
                LockOnSystem?.ClearLockOn();
                _stateMachine.ChangeState(DeadState);
                SetInputActionsState(false);
                PostProcessingDirector.Instance?.TransitionTo(PostProcessingDirector.PostProcessingMode.Death, 2f);
            }
        }

        public void EnableInteractionOnly()
        {
            _interactActionRef.action.Enable();
        }

        #endregion

        #region Event Listeners/Callbacks

        private void OnRollPerformed(InputAction.CallbackContext context) => RollTriggered = true;
        private void OnAttackPerformed(InputAction.CallbackContext context) => AttackTriggered = true;
        private void OnLockOnPerformed(InputAction.CallbackContext context) => LockOnSystem?.ToggleLockOn();

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (_stateMachine.CurrentState == MoveState)
            {
                InteractionSystem.InteractWithClosestObject();
            }
            else if (_stateMachine.CurrentState == RestingState)
            {
                _stateMachine.ChangeState(MoveState);
            }
        }


        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            if (_uiManager == null) 
            {
                Debug.LogWarning("[PlayerController] UIManager is missing. Cannot pause.");
                return;
            }
            
            bool isPaused = _uiManager.TogglePauseMenu();
            SetInputActionsState(!isPaused);
        }

        
        #endregion
    }
}