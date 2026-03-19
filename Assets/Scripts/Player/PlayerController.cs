using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerCombatManager))]
[RequireComponent(typeof(PlayerStatsManager))]
[RequireComponent(typeof(PlayerLocomotion))]
public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference moveActionRef;
    [SerializeField] private InputActionReference rollActionRef;
    [SerializeField] private InputActionReference lightAttackRef;
    [SerializeField] private InputActionReference heavyAttackRef;
    [SerializeField] private InputActionReference interactActionRef;
    [SerializeField] private InputActionReference pauseActionRef;

    [Header("Stats")]
    public float MoveSpeed = 6f;
    public float RotationSpeed = 15f;
    public float RollDuration = 0.8f;
    public float RollDistanceMult = 15f;
    public AnimationCurve RollSpeedCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 3.5f));

    [Header("UI")]
    [SerializeField] private UIManager uiManager;

    public Animator Animator { get; private set; }
    public PlayerCombatManager CombatManager { get; private set; }
    public PlayerStatsManager Stats { get; private set; }
    public PlayerLocomotion Locomotion { get; private set; }
    public InteractionSystem InteractionSystem;

    private PlayerStateMachine _stateMachine;
    public PlayerMoveState MoveState { get; private set; }
    public PlayerRollState RollState { get; private set; }
    public PlayerAttackState AttackState { get; private set; }
    public PlayerInteractState InteractState { get; private set; }

    public Vector3 MoveInput { get; private set; }
    public bool IsMoving => MoveInput.sqrMagnitude > 0.001f;
    
    public bool RollTriggered { get; private set; }
    public bool AttackTriggered => LightAttackTriggered || HeavyAttackTriggered;
    public bool LightAttackTriggered { get; private set; }
    public bool HeavyAttackTriggered { get; private set; }

    private void Awake()
    {
        CombatManager = GetComponent<PlayerCombatManager>();
        Stats = GetComponent<PlayerStatsManager>();
        Locomotion = GetComponent<PlayerLocomotion>();
        Animator = GetComponentInChildren<Animator>();

        InitStateMachine();
    }

    private void Start() => _stateMachine.Initialize(MoveState);
    
    private void Update()
    {
        ReadInput();

        _stateMachine.CurrentState.UpdateState();
    }

    private void FixedUpdate() => _stateMachine.CurrentState.FixedUpdateState();

    private void InitStateMachine()
    {
        _stateMachine = new PlayerStateMachine();
        MoveState = new PlayerMoveState(this, _stateMachine);
        RollState = new PlayerRollState(this, _stateMachine);
        AttackState = new PlayerAttackState(this, _stateMachine);
        InteractState = new PlayerInteractState(this, _stateMachine);
    }

    private void ReadInput()
    {
        if (moveActionRef != null)
        {
            Vector2 input = moveActionRef.action.ReadValue<Vector2>();
            MoveInput = new Vector3(input.x, 0, input.y).normalized;
        }
    }

    public void ResetRollTrigger() => RollTriggered = false;
    public void ResetAttackTriggers() { LightAttackTriggered = false; HeavyAttackTriggered = false; }

    private void OnEnable()
    {
        if (moveActionRef == null) return;

        SetInputActionsState(true);

        rollActionRef.action.performed += OnRollPerformed;
        lightAttackRef.action.performed += OnLightAttackPerformed;
        heavyAttackRef.action.performed += OnHeavyAttackPerformed;
        interactActionRef.action.performed += OnInteractPerformed;
        pauseActionRef.action.performed += OnPausePerformed;
    }

    private void OnDisable()
    {
        if (moveActionRef == null) return;

        rollActionRef.action.performed -= OnRollPerformed;
        lightAttackRef.action.performed -= OnLightAttackPerformed;
        heavyAttackRef.action.performed -= OnHeavyAttackPerformed;
        interactActionRef.action.performed -= OnInteractPerformed;
        pauseActionRef.action.performed -= OnPausePerformed;

        SetInputActionsState(false);
    }

    public void SetInputActionsState(bool state)
    {
        if(state)
        {
            moveActionRef.action.Enable();
            rollActionRef.action.Enable();
            lightAttackRef.action.Enable();
            heavyAttackRef.action.Enable();
            interactActionRef.action.Enable();
        }
        else
        {
            moveActionRef.action.Disable();
            rollActionRef.action.Disable();
            lightAttackRef.action.Disable();
            heavyAttackRef.action.Disable();
            interactActionRef.action.Disable();
        }
    }


    private void OnRollPerformed(InputAction.CallbackContext context)
    {
        RollTriggered = true;
    }

    private void OnLightAttackPerformed(InputAction.CallbackContext context)
    {
        LightAttackTriggered = true;
    }

    private void OnHeavyAttackPerformed(InputAction.CallbackContext context)
    {
        HeavyAttackTriggered = true;
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
    
        if (_stateMachine.CurrentState == MoveState)
        {
            _stateMachine.ChangeState(InteractState);
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        bool isPaused = uiManager.TogglePauseMenu();

        SetInputActionsState(!isPaused);
    }

}