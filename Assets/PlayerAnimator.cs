using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Player;

public class PlayerAnimator : MonoBehaviour
{

    private Animator animator;
    private Player player;

    [SerializeField] private float slideDuration;
    [SerializeField] private float standUpDuration;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        player = transform.parent.GetComponent<Player>();

        player.OnTransition += Player_OnTransition;
    }

    private void Player_OnTransition(object sender, System.EventArgs e)
    {
        animator.Play(StateToClipName(player.CurrentState),0,0);

        switch (player.CurrentState) {
            case PlayerState.Sliding:
                //Set duration of animation
                animator.speed = animator.GetCurrentAnimatorStateInfo(0).length / slideDuration;
                break;
            case PlayerState.StandingUp:
                animator.speed = animator.GetCurrentAnimatorStateInfo(0).length / standUpDuration;
                break;
            default:
                animator.speed = 1.0f;
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
