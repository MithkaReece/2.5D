﻿using UnityEngine;
using System.Collections;
using static Player;

public class PlayerJumping : MonoBehaviour
{
    [SerializeField] private float _jumpSpeed = 2f;

    private float _gravity = 9.8f;
    private float _jumpVelocity = 0f;
    private float _heightOffset = 0f;
    private bool _onGround = true;

    private float _currentHeight;
    private float _yJumpedFrom;

    private Player _player;
    [SerializeField] private PlayerAnimator _playerAnimator;



    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    public void HandleJumping() {
        //Cause initial jump
        if (!GameInput.IsJumping() || _onGround == false) return;
        _player.SetState(PlayerState.Jumping);
        _currentReadyDuration = 0;
    }


    float _currentReadyDuration = 0f;

    public void ManualUpdate() {
        if (_onGround) {
            _currentReadyDuration += Time.deltaTime;

            if(_currentReadyDuration >= _playerAnimator.GetReadyDuration()) {
                _currentReadyDuration = 0f;
                // Actually jump
                _jumpVelocity = _jumpSpeed;
                _yJumpedFrom = transform.position.y;
                _onGround = false;
                Debug.Log("JUMP");
            }
        } else {
            HandleGravity();
        }
    }

    void HandleGravity() {
        if (_onGround) return;

        // Apply velocity to position
        _heightOffset += _jumpVelocity * Time.deltaTime;

        // Apply acceleration (gravity) to velocity
        _jumpVelocity -= _gravity * Time.deltaTime;

        // Update player position
        transform.position = new Vector3(transform.position.x,
            transform.position.y + _heightOffset,
            transform.position.z);

        //Track total height
        _currentHeight += _heightOffset;

        // Check for grounded (so far its just when you go back to previous y)
        if (_currentHeight <= 0) {
            _heightOffset = 0;
            _currentHeight = 0;

            transform.position = new Vector3(transform.position.x,
            _yJumpedFrom, transform.position.z);

            _onGround = true;
            _player.SetState(PlayerState.Idle);
        }
    }
}
