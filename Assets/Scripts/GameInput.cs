using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnSlideAction;
    public event EventHandler OnAttackAction;

    public enum Binding {
        Move_Up,
        Move_Left,
        Move_Down,
        Move_Right,
        Slide,
        Jump,
        Attack,
        Attack_Alt,
        Pause,
    }

    private static PlayerInputActions playerInputActions;

    private void Awake()
    {
        Instance = this;

        playerInputActions = new PlayerInputActions();


        playerInputActions.Player.Enable();

        //Add events for input values 
        playerInputActions.Player.Slide.performed += Slide_performed;
        playerInputActions.Player.Attack.performed += Attack_performed;
    }

    private void Attack_performed(InputAction.CallbackContext obj)
    {
        OnAttackAction?.Invoke(this, EventArgs.Empty);
    }

    private void Slide_performed(InputAction.CallbackContext obj)
    {
        OnSlideAction?.Invoke(this, EventArgs.Empty);
    }

    public static Vector3 GetMovementVector() {
        return playerInputActions.Player.Move.ReadValue<Vector2>();
    }

    public static bool IsJumping() {
        return playerInputActions.Player.Jump.triggered;
    }

    private void OnDestroy()
    {
        //Unsubscribe events
        playerInputActions.Player.Slide.performed -= Slide_performed;
        
    }

}
