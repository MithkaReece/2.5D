using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Player;

public class PlayerAnimator : MonoBehaviour
{

    private Animator _animator;
    private Player _player;

    [SerializeField] private float slideDuration;
    [SerializeField] private float standUpDuration;
    [SerializeField] private float jumpDuration;
    private float readyRatio = 12.0f / 41.0f;

    public float GetReadyDuration() { return readyRatio * jumpDuration; }

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _player = GetComponentInParent<Player>();

        _player.OnTransition += Player_OnTransition;
    }

    private void Player_OnTransition(object sender, System.EventArgs e)
    {
        _animator.CrossFade(StateToClipName(_player.CurrentState),0,0);

        switch (_player.CurrentState) {
            case PlayerState.Sliding:
                //Set duration of animation
                _animator.speed = _animator.GetCurrentAnimatorStateInfo(0).length / slideDuration;
                break;
            case PlayerState.StandingUp:
                _animator.speed = _animator.GetCurrentAnimatorStateInfo(0).length / standUpDuration;
                break;
            case PlayerState.Jumping:
                _animator.speed = _animator.GetCurrentAnimatorStateInfo(0).length / jumpDuration;
                break;
            default:
                _animator.speed = 1.0f;
                break;
        }        
    }

    private string StateToClipName(PlayerState state) {
        switch (state) {
            case PlayerState.Idle:
                return "idle";
            case PlayerState.Walking:
                return "run";
            case PlayerState.Jumping:
                return "jump";
            case PlayerState.Falling:
                return "fall";
            case PlayerState.Sliding:
                return "slide";
            case PlayerState.StandingUp:
                return "stand";
            case PlayerState.Attacking:
                return "attack1";
            default:
                return "idle";
        }
    }
}
