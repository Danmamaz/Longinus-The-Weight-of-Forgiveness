using UnityEngine;
using UnityEngine.InputSystem;
using Longinus.UI;

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
        
        [SerializeField, Tooltip("Reference to the light attack input action.")] 
        private InputActionReference _lightAttackRef;
        
        [SerializeField, Tooltip("Reference to the heavy attack input action.")] 
        private InputActionReference _heavyAttackRef;
        
        [SerializeField, Tooltip("Reference to the interaction input action.")] 
        private InputActionReference _interactActionRef;
        
        [SerializeField, Tooltip("Reference to the pause menu input action.")] 
        private InputActionReference _pauseActionRef;

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
        public InteractionSystem InteractionSystem => _interactionSystem;

        // State Machine States
        public PlayerMoveState MoveState { get; private set; }
        public PlayerRollState RollState { get; private set; }
        public PlayerAttackState AttackState { get; private set; }
        public PlayerInteractState InteractState { get; private set; }

        // Input States
        public Vector3 MoveInput { get; private set; }
        public bool IsMoving => MoveInput.sqrMagnitude > 0.001f;
        public bool RollTriggered { get; private set; }
        public bool AttackTriggered => LightAttackTriggered || HeavyAttackTriggered;
        public bool LightAttackTriggered { get; private set; }
        public bool HeavyAttackTriggered { get; private set; }
        public float RollStaminaCost => _rollStaminaCost;
        
        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            Instance = this;
            CombatManager = GetComponent<PlayerCombatManager>();
            Stats = GetComponent<PlayerStatsManager>();
            Locomotion = GetComponent<PlayerLocomotion>();
            Animator = GetComponentInChildren<Animator>();

            InitStateMachine();
        }

        private void OnEnable()
        {
            if (_moveActionRef == null) return;

            SetInputActionsState(true);

            _rollActionRef.action.performed += OnRollPerformed;
            _lightAttackRef.action.performed += OnLightAttackPerformed;
            _heavyAttackRef.action.performed += OnHeavyAttackPerformed;
            _interactActionRef.action.performed += OnInteractPerformed;
            _pauseActionRef.action.performed += OnPausePerformed;
        }

        private void Start()
        {
            _stateMachine.Initialize(MoveState);
        }

        private void Update()
        {
            ReadInput();
            
            // Defensively calling through the current state to avoid exceptions if the machine isn't fully initialized
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
            _lightAttackRef.action.performed -= OnLightAttackPerformed;
            _heavyAttackRef.action.performed -= OnHeavyAttackPerformed;
            _interactActionRef.action.performed -= OnInteractPerformed;
            _pauseActionRef.action.performed -= OnPausePerformed;

            SetInputActionsState(false);
        }

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Instantiates the state machine and all player states.
        /// </summary>
        private void InitStateMachine()
        {
            _stateMachine = new PlayerStateMachine();
            MoveState = new PlayerMoveState(this, _stateMachine);
            RollState = new PlayerRollState(this, _stateMachine);
            AttackState = new PlayerAttackState(this, _stateMachine);
            InteractState = new PlayerInteractState(this, _stateMachine);
        }

        /// <summary>
        /// Reads and normalizes the movement input vector.
        /// </summary>
        private void ReadInput()
        {
            if (_moveActionRef != null)
            {
                Vector2 input = _moveActionRef.action.ReadValue<Vector2>();
                MoveInput = new Vector3(input.x, 0f, input.y).normalized;
            }
        }

        /// <summary>
        /// Consumes the roll trigger to prevent continuous rolling.
        /// </summary>
        public void ResetRollTrigger() => RollTriggered = false;

        /// <summary>
        /// Consumes attack triggers to prevent buffered attacks from firing indefinitely.
        /// </summary>
        public void ResetAttackTriggers() 
        { 
            LightAttackTriggered = false; 
            HeavyAttackTriggered = false; 
        }

        /// <summary>
        /// Enables or disables combat and movement input actions.
        /// Pause action remains active intentionally.
        /// </summary>
        /// <param name="state">True to enable, false to disable.</param>
        public void SetInputActionsState(bool state)
        {
            if(state)
            {
                _moveActionRef.action.Enable();
                _rollActionRef.action.Enable();
                _lightAttackRef.action.Enable();
                _heavyAttackRef.action.Enable();
                _interactActionRef.action.Enable();
            }
            else
            {
                _moveActionRef.action.Disable();
                _rollActionRef.action.Disable();
                _lightAttackRef.action.Disable();
                _heavyAttackRef.action.Disable();
                _interactActionRef.action.Disable();
            }
        }

        #endregion

        #region Event Listeners/Callbacks

        private void OnRollPerformed(InputAction.CallbackContext context) => RollTriggered = true;
        private void OnLightAttackPerformed(InputAction.CallbackContext context) => LightAttackTriggered = true;
        private void OnHeavyAttackPerformed(InputAction.CallbackContext context) => HeavyAttackTriggered = true;

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (_stateMachine.CurrentState == MoveState)
            {
                _stateMachine.ChangeState(InteractState);
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