using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Components
    [Header("References")]
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private PlayerMoveData _moveData;
    [SerializeField] public CachedMoveData moveData;
    [HideInInspector] private CharacterController controller;
    #endregion

    [Header("Movement States")]
    private IMovementState currentMoveState;
    public GroundState groundState;
    public AirState airState;
    private IEnumerable<IMovementState> AllStates
    {
        get
        {
            yield return groundState;
            yield return airState;
        }
    }

    [Header("Inputs")]
    [SerializeField] public bool allowMove = true;
    [SerializeField] private bool attemptingSprint;
    [SerializeField] private bool attemptingCrouch;
    [SerializeField] private bool attemptingJump = false;
    [SerializeField] public bool toggleCrouch;
    
    [Header("Speed")]
    [SerializeField] private bool changeSpeed;
    [SerializeField] private float speed = 0f;
    [SerializeField] private float groundSmoothen;
    [SerializeField] public Vector2 moveInput;
    [SerializeField] public Vector3 moveDir = Vector3.zero;
    [SerializeField] public Vector3 velocity = Vector3.zero;
    [HideInInspector] public Vector3 moveSmoothen = Vector3.zero;
    [HideInInspector] public Vector3 slopeSmoothen = Vector3.zero;
    [HideInInspector] public Vector3 moveProject = Vector3.zero;

    [Header("Walking and Sprinting")]
    [SerializeField] public bool isMoving = false;
    [SerializeField] public bool isSprinting = false;

    [Header("Crouching")]
    [SerializeField] private bool canUncrouch = false;
    [SerializeField] public bool isCrouching = false;

    [Header("Jumping")]
    [SerializeField] private bool useGravity = true;
    [SerializeField] public float fallVelocity = 0f;
    [SerializeField] public bool jumped = false;
    
    [Header("Ground Detection")]
    [SerializeField] public float coyoteTime;
    [SerializeField] public bool CoyoteGrounded() => coyoteTime < moveData.coyoteTimeMax;
    [SerializeField] public float fallSpeed = 0f;
    [SerializeField] public float groundDistance;
    public ControllerColliderHit colliderHit;

    [Header("Events")]
    public UnityEvent OnStartMove, OnStopMove;
    public UnityEvent OnStartJump, OnStopGrounded, OnStartLand;
    public UnityEvent OnCrouchAction;
    private Coroutine tagCoroutine;
    private Coroutine crouchCoroutine;

    #region Initialization and Deactivation Logic
    private bool playerInputInit = false;
    private void Awake()
    {
        if (controller == null) controller = playerManager.GetCharacterController();
        InitVariables(ref moveData);
        InitEvents();
        InitStates();
    }

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        ToggleInputs(true);
        playerInputInit = true;
    }
    
    private void OnEnable()
    {
        if (!playerInputInit) return;
        ToggleInputs(true);
    }

    private void OnDisable() => ToggleInputs(false);

    private void ToggleInputs(bool enable)
    {
        var playerInput = PlayerInput.Instance;
        var actions = new[]
        {
            playerInput.MoveAction,
            playerInput.JumpAction,
            playerInput.SprintAction,
            playerInput.CrouchAction
        };
        Action<InputAction.CallbackContext> actionHandler = enable ? OnActionInput : null;
        Action<InputAction.CallbackContext> cancelHandler = enable ? OnActionInput : null;
        foreach (var action in actions)
        {
            action.performed -= actionHandler;
            action.canceled -= cancelHandler;
            if (enable)
            {
                action.performed += actionHandler;
                action.canceled += cancelHandler;
            }
        }
    }

    private void InitEvents()
    {
        OnCrouchAction.AddListener(CalculateHeight);
        OnStartLand.AddListener(Crouch);
        OnStartLand.AddListener(Jump);
        OnStartLand.AddListener(Sprint);
        OnStartLand.AddListener(StartTag);
        OnStopGrounded.AddListener(Crouch);
        OnStartMove.AddListener(Sprint);
        OnStopMove.AddListener(Sprint);
    }

    public void InitStates()
    {
        groundState = new GroundState(playerManager);
        airState = new AirState(playerManager);
    }

    private void InitVariables(ref CachedMoveData data) 
    {
        data.tagMovement = _moveData.tagMovement;
        data.acceleration = _moveData.acceleration;
        data.tagSpeed = _moveData.tagSpeed;
        data.tagDelay = _moveData.tagDelay;
        data.walkSpeed = _moveData.walkSpeed;
        data.sprintSpeed = _moveData.sprintSpeed;
        data.crouchSpeed = _moveData.crouchSpeed;
        data.walkDamp = _moveData.walkDamp;
        data.sprintDamp = _moveData.sprintDamp;
        data.crouchDamp = _moveData.crouchDamp;
        data.airSmoothen = _moveData.airSmoothen;
        data.heightLerp = _moveData.heightLerp;
        data.standHeight = _moveData.standHeight;
        data.crouchHeight = _moveData.crouchHeight;
        data.jumpForce = _moveData.jumpForce;
        data.groundMask = _moveData.groundMask;
        data.antiBump = _moveData.antiBump;
        data.fallTimeMax = _moveData.fallTimeMax;
        data.coyoteTimeMax = _moveData.coyoteTimeMax;
        data.stepOffset = _moveData.stepOffset;
        groundDistance = controller.radius;
    }
    #endregion
    
    private void OnGUI()
    {
        GUILayout.Label("Speed: " + velocity.magnitude);
        GUILayout.Label("Gravity: " + fallVelocity);
        GUILayout.Label("Frames Per Second: " + (int)(1 / Time.deltaTime));
        GUILayout.Label("CurrentState: " + currentMoveState);
        if (GUILayout.Button("[Debug] Refresh Movement Data")) currentMoveState?.RefreshVar(moveData);
    }

    #region Update Methods
    #region State Machine Logic
    private void MoveStateHandler()
    {
        var nextState = GetMoveState();
        if (nextState != null && nextState != currentMoveState)
        {
            currentMoveState?.OnExitedState();
            currentMoveState = nextState;
            currentMoveState?.OnEnteredState();
        }
    }

    private IMovementState GetMoveState()
    {
        IMovementState nextState = null;
        foreach (IMovementState state in AllStates)
        {
            if (state.IsRelevant(this))
            {
                nextState = state;
                break;
            }
        }
        return nextState;
    }
    #endregion

    private void Update()
    {
        QueueStopCrouch = CanUncrouch();
        Move();
        ApplyGravity();
    }

    private void FixedUpdate()
    {
        MoveStateHandler();
        GroundChecker();
    }
    #endregion

    #region Inputs
    private void OnActionInput(InputAction.CallbackContext ctx)
    {
        var action = ctx.action;
        var playerInput = PlayerInput.Instance;
        switch (action)
        {
            case var moveAction when moveAction == playerInput.MoveAction:
                moveInput = ctx.performed ? ctx.ReadValue<Vector2>().normalized : Vector2.zero;
            break;
            case var jumpAction when jumpAction == playerInput.JumpAction:
                attemptingJump = ctx.performed;
                Jump();
            break;
            case var sprintAction when sprintAction == playerInput.SprintAction:
                attemptingSprint = ctx.performed;
            break;
            case var crouchAction when crouchAction == playerInput.CrouchAction:
                attemptingCrouch = ctx.performed ? (!toggleCrouch || !attemptingCrouch) : toggleCrouch && attemptingCrouch;
                Crouch();
            break;
        }
        Sprint();
    }
    #endregion

    #region Gravity and Detection Logic
    private bool fell;
    public bool Fell
    {
        get { return fell; }
        set
        {
            if (fell == value) return;
            fell = value;
            if (!fell) return;
            jumped = false;
            OnStartLand?.Invoke();
            coyoteTime = 0;
        }
    }

    private void ApplyGravity()
    {
        if (useGravity) fallVelocity -= Physics.gravity.y * -2f * Time.deltaTime;
        if (controller.enabled) controller.Move(new Vector3(0f, fallVelocity, 0f) * Time.deltaTime);
		if (!controller.isGrounded)
        {
            fallSpeed = fallVelocity;
            coyoteTime += Time.deltaTime;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) => colliderHit = hit;
    private void GroundChecker() => Fell = controller.isGrounded;

    #endregion
    #region Movement Logic
    #endregion

    #region Movement Logic
    private void SetIsMoving(bool value)
    {
        if (isMoving == value) return;
        isMoving = value;
        switch (isMoving)
        {
            case true:
                OnStartMove?.Invoke();
            break;
            case false:
                OnStopMove?.Invoke();
            break;
        }
    }

    private float desiredSpeed;
    private void ChangeSpeed()
    {
        desiredSpeed = isCrouching ? moveData.crouchSpeed : isSprinting ? moveData.sprintSpeed : moveData.walkSpeed;
        if (!changeSpeed) return;
        if (!Mathf.Approximately(speed, desiredSpeed))
        {
            speed = Mathf.Lerp(speed, desiredSpeed, moveData.acceleration * Time.deltaTime);
            groundSmoothen = isCrouching ? moveData.crouchDamp : isSprinting ? moveData.sprintDamp : moveData.walkDamp;
        }
    }

    private void Move()
    {
        ChangeSpeed();
        SetIsMoving(velocity != Vector3.zero && moveInput != Vector2.zero);
        if (fallVelocity < 0 && controller.isGrounded) fallVelocity = moveData.antiBump;
        moveSmoothen = Vector3.MoveTowards(moveSmoothen, currentMoveState.MoveVector(moveInput), (CoyoteGrounded() ? groundSmoothen : moveData.airSmoothen) * Time.deltaTime);
        slopeSmoothen = Vector3.MoveTowards(slopeSmoothen, groundState.slopeMoveDir, groundSmoothen * Time.deltaTime);
        moveProject = Vector3.ProjectOnPlane(slopeSmoothen, groundState.slopeForce);
        var groundMoveVector = groundState.onSlope ? moveProject : moveSmoothen;
        if (currentMoveState != null && allowMove && controller.enabled) controller.Move((!CoyoteGrounded() ? moveSmoothen : groundMoveVector) * speed * Time.deltaTime);
        velocity = controller.velocity;
    }

    public void StartTag()
    {
        if (!moveData.tagMovement) return;
        if(coyoteTime < moveData.fallTimeMax) return;
        if (tagCoroutine != null) StopCoroutine(TagMovement());
        tagCoroutine = StartCoroutine(TagMovement());
    }

    private IEnumerator TagMovement()
    {
        changeSpeed = false;
        speed = moveData.tagSpeed;
        yield return new WaitForSeconds(moveData.tagDelay);
        if(CoyoteGrounded()) changeSpeed = true;
    }
    #endregion

    #region Sprint Logic
    private void Sprint() => isSprinting = (controller.isGrounded || CoyoteGrounded()) && isMoving && !isCrouching && (attemptingSprint || PlayerInput.Instance.joystick.FullInput);

    #endregion

    #region Jump Logic
    private void Jump()
    {
        if (!attemptingJump) return;
        if (!allowMove) return;
        if (jumped) return;
        if (!controller.isGrounded && !CoyoteGrounded()) return;
        if (!canUncrouch && isCrouching) return;
        jumped = true;
        OnStartJump?.Invoke();
        attemptingJump = false;
        fallVelocity = Mathf.Sqrt(moveData.jumpForce * -3.0f * Physics.gravity.y);
    }
    #endregion

    #region  Crouch Logic
    #region Crouch Input Logic
    public bool QueueStopCrouch
    {
        get { return attemptingCrouch; }
        set
        {
            if (!isCrouching) return;
            if (value == canUncrouch && !attemptingCrouch) Crouch();
        }
    }
    #endregion

    private bool CanUncrouch() => canUncrouch = isCrouching && !Physics.SphereCast(transform.position, groundDistance, Vector3.up, out _, controller.height + (moveData.standHeight - controller.height) - 0.1f, moveData.groundMask);
    
    private void Crouch()
    {
        switch (isCrouching)
        {
            case true when (!controller.isGrounded || !attemptingCrouch) && canUncrouch:
                isCrouching = false;
                OnCrouchAction?.Invoke();
            break;
            case false when attemptingCrouch && controller.isGrounded:
                isCrouching = true;
                OnCrouchAction?.Invoke();
            break;
        }
    }

    private void CalculateHeight()
    {
        if (crouchCoroutine != null) StopCoroutine(crouchCoroutine);
        crouchCoroutine = StartCoroutine(AdjustHeight(isCrouching ? moveData.crouchHeight : moveData.standHeight));
    }

    private IEnumerator AdjustHeight(float height)
    {
        while(!Mathf.Approximately(controller.height, height))
        {
            controller.height = Mathf.Lerp(controller.height, height, moveData.heightLerp * Time.deltaTime);
            controller.center = Vector3.Lerp(controller.center, new(0, height * 0.5f, 0), moveData.heightLerp * Time.deltaTime);
            playerManager.GetOrientation().transform.localPosition = controller.center;
            playerManager.GetHead().transform.localPosition = new(0f, controller.height, 0f);
            yield return null;
        }
    }
    #endregion
}