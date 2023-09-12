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
            default:
                _animator.speed = 1.0f;
                break;
        }        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private string StateToClipName(PlayerState state) {
        switch (state) {
            case PlayerState.Idle:
                return "idle";
            case PlayerState.Walking:
                return "run";
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
