using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    const float FLOAT_ERROR = 0.1f;

    public event EventHandler OnTransition;

    public enum PlayerState {
        Idle,
        Walking,
        Sliding,
        StandingUp,
        Attacking
    }

    public PlayerState CurrentState { get; private set; }

    [SerializeField] private AnimationCurve slideCurve;
    [SerializeField] private float slideDistance;
    private Vector3 beforeSlidePosition;

    private Vector3 facingDirection = Vector3.up;

    [SerializeField] private List<PlayerState> SlideableStates;
    [SerializeField] private List<PlayerState> attackableStates;

    [SerializeField] private Animator animator;

    private void Start()
    {
        GameInput.Instance.OnSlideAction += GameInput_OnSlideAction;
        GameInput.Instance.OnAttackAction += GameInput_OnAttackAction;
    }

    private void GameInput_OnAttackAction(object sender, EventArgs e)
    {
        if (!attackableStates.Contains(CurrentState)) return;
        SetState(PlayerState.Attacking);
    }

    private void GameInput_OnSlideAction(object sender, EventArgs e)
    {
        if (!SlideableStates.Contains(CurrentState)) return;

        beforeSlidePosition = transform.position;
        SetState(PlayerState.Sliding);
    }

    float animationTime = 0f;

    // Update is called once per frame
    void Update()
    {
        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;

        //Animation finished (Include any transitions)
        if (animationTime > animationLength) {
            animationTime = 0f;
            switch (CurrentState) {
                case PlayerState.Sliding:
                    // Ensure the player reaches the exact target position
                    transform.position = beforeSlidePosition + facingDirection * slideDistance;
                    SetState(PlayerState.StandingUp);
                    break;
                case PlayerState.StandingUp:
                    SetState(PlayerState.Idle);
                    break;
                case PlayerState.Attacking:
                    SetState(PlayerState.Idle);
                    break;
            }
        }

        //Functionality during the state
        switch (CurrentState) {
            case PlayerState.Idle:
            case PlayerState.Walking:
                HandleMovement();
                break;
            case PlayerState.Sliding:
                // Move player based on slideCurve
                float curveValue = slideCurve.Evaluate(animationTime / animationLength);
                transform.position = beforeSlidePosition + facingDirection * curveValue * slideDistance;
                break;
        }



        animationTime += Time.deltaTime;
    }

    void HandleMovement() {
        Vector3 movementVector = GameInput.GetMovementVector();

        float movementSpeed = 10f;
        transform.position += movementVector * movementSpeed * Time.deltaTime;
        if(movementVector.sqrMagnitude > FLOAT_ERROR) {
            SetState(PlayerState.Walking);
        } else {
            SetState(PlayerState.Idle);
        }

        if (movementVector.magnitude > FLOAT_ERROR)
            facingDirection = movementVector;

        //Flip left/right
        if (movementVector.x > FLOAT_ERROR)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (movementVector.x < -FLOAT_ERROR) 
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }


    void SetState(PlayerState newState) {
        if (newState == CurrentState) return;
        //If state changes, send out event, transition
        CurrentState = newState;
        animationTime = 0f;
        OnTransition?.Invoke(this, EventArgs.Empty);
    }
}
