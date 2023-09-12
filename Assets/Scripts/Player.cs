using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Player : MonoBehaviour
{
    const float FLOAT_ERROR = 0.1f;

    public event EventHandler OnTransition;

    public PlayerState CurrentState { get; private set; }

    public enum PlayerState {
        Idle,
        Walking,
        Jumping,
        Falling,
        Sliding,
        StandingUp,
        Attacking
    }

    [SerializeField] private List<PlayerState> _BeforeIdle;
    [SerializeField] private List<PlayerState> _BeforeWalking;
    [SerializeField] private List<PlayerState> _BeforeSliding;
    [SerializeField] private List<PlayerState> _BeforeStandingUp;
    [SerializeField] private List<PlayerState> _BeforeAttacking;

    private Vector3 _facingDirection = Vector3.up;

    [SerializeField] private AnimationCurve _slideCurve;
    [SerializeField] private float _slideDistance;
    private Vector3 _beforeSlidePosition;


    [SerializeField] private Animator _animator;


    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _airMovementSpeed;



    private PlayerJumping _playerJumping;

    private void Awake()
    {
        _playerJumping = GetComponent<PlayerJumping>();
    }

    /**
     * Inputs (Action check) -> Change State -> Run During State -> Run End State 
     * Run End State is empty if loops
     * Transition can be during or at end
     * 
     * Triggered(Event) or Conditional(Checks every update)
     * Action Check (Specify states)
     * State
     * During Function(animationDeltaTime)
     * End State Function
     * 
     */

    private void Start()
    {
        GameInput.Instance.OnSlideAction += GameInput_OnSlideAction;
        GameInput.Instance.OnAttackAction += GameInput_OnAttackAction;
    }

    #region Inputs

    bool IsValidState(List<PlayerState> validPreviousStates) {
        if (validPreviousStates.Count == 0) return true;
        return validPreviousStates.Contains(CurrentState);
    }

    private void GameInput_OnAttackAction(object sender, EventArgs e)
    {
        if (!IsValidState(_BeforeAttacking)) return;
        SetState(PlayerState.Attacking);
    }

    private void GameInput_OnSlideAction(object sender, EventArgs e)
    {
        if (!IsValidState(_BeforeSliding)) return;

        _beforeSlidePosition = transform.position;
        SetState(PlayerState.Sliding);
    }


    #endregion

    float _animationDeltaTime = 0f;
    float _currentAnimationLength;

    // Update is called once per frame
    void Update()
    {
        _currentAnimationLength = _animator.GetCurrentAnimatorStateInfo(0).length;
        EndStates();
        DuringStates();
        _animationDeltaTime += Time.deltaTime;
    }

    #region During States
    //Functionality during the state
    void DuringStates() {
        switch (CurrentState) {
            case PlayerState.Idle:
            case PlayerState.Walking:
                HandleWalking();
                _playerJumping.HandleJumping();
                return;
            case PlayerState.StandingUp:
                return;
            case PlayerState.Jumping:
            case PlayerState.Falling:
                HandleAirMovement();
                _playerJumping.ManualUpdate();
                return;
            case PlayerState.Sliding:
                // Move player along slide curve
                float curveValue = _slideCurve.Evaluate(_animationDeltaTime / _currentAnimationLength);
                transform.position = _beforeSlidePosition + _facingDirection * curveValue * _slideDistance;
                return;
        }

    }

    void HandleWalking() {
        // Move player based on input
        Vector3 movementVector = GameInput.GetMovementVector();
        transform.position += movementVector * _movementSpeed * Time.deltaTime;

        if (movementVector.sqrMagnitude > FLOAT_ERROR) SetState(PlayerState.Walking);
        else SetState(PlayerState.Idle);

        HandleFacingDirection(movementVector);
    }

    void HandleAirMovement() {
        // Move player based on input
        Vector3 movementVector = GameInput.GetMovementVector();
        transform.position += movementVector * _airMovementSpeed * Time.deltaTime;

        HandleFacingDirection(movementVector);
    }

    void HandleFacingDirection(Vector3 movementVector) {
        // Set facing direction
        if (movementVector.magnitude > FLOAT_ERROR)
            _facingDirection = movementVector;

        //Flip left/right
        if (movementVector.x > FLOAT_ERROR)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (movementVector.x < -FLOAT_ERROR)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }



    #endregion



    #region End States
    void EndStates() {
        // When animation finishes
        if (_animationDeltaTime < _currentAnimationLength) return;
       
        _animationDeltaTime = 0f;

        switch (CurrentState) {
            case PlayerState.Sliding:
                // Ensure the player reaches the exact target position
                transform.position = _beforeSlidePosition + _facingDirection * _slideDistance;
                SetState(PlayerState.StandingUp);
                return;
            case PlayerState.StandingUp:
            case PlayerState.Attacking:
                SetState(PlayerState.Idle);
                return;
            case PlayerState.Jumping:
                SetState(PlayerState.Falling);
                return;
        }
    }
    #endregion



    public void SetState(PlayerState newState) {
        if (newState == CurrentState) return;
        //If state changes, send out event, transition
        CurrentState = newState;
        _animationDeltaTime = 0f;
        OnTransition?.Invoke(this, EventArgs.Empty);
    }
}
