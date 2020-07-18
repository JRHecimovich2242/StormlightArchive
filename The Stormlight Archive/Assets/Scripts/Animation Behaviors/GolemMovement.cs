using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemMovement : StateMachineBehaviour
{
    public float _moveSpeed;
    public float _detectionRange;
    public float _attackRange;
    private bool _followingPlayer = false;
    private bool _attackPlayer = false;
    private Golem _golem;

    private float _playerPosition = 0f; //_playerPosition will be positive if the player is on the right and negative if on the left, 0 if directly above or below.
    private PlayerController _player;
    private float _playerDistance;

    private GameObject _gameObject;
    private Rigidbody2D myRigidbody2D;
    private Animator myAnimator;
    private CapsuleCollider2D myCapsuleCollider2D;

    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _gameObject = animator.gameObject;
        _golem = _gameObject.GetComponent<Golem>();
        myRigidbody2D = _gameObject.GetComponent<Rigidbody2D>();
        myAnimator = _gameObject.GetComponent<Animator>();
        myCapsuleCollider2D = _gameObject.GetComponent<CapsuleCollider2D>();
        _player = FindObjectOfType<PlayerController>();
    }



    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Move();
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}

    private void SearchForPlayer()
    {

        _playerDistance = new Vector2(Mathf.Abs(_player.transform.position.x - _gameObject.transform.position.x), Mathf.Abs(_player.transform.position.y - _gameObject.transform.position.y)).magnitude;
        _followingPlayer = _playerDistance <= _detectionRange;
        _attackPlayer = _playerDistance <= _attackRange;
        if (_followingPlayer)
        {
            _playerPosition = Mathf.Sign(_player.transform.position.x - _gameObject.transform.position.x);
        }
    }

    private void Move()
    {
        //Move towards player, attack if player is in range.
        _gameObject.transform.localScale = new Vector2(_playerPosition, 1); //Face the player
        myRigidbody2D.velocity = new Vector2(_playerPosition * _moveSpeed, myRigidbody2D.velocity.y); //Need to account for hit knockback
        myAnimator.SetBool("running", Mathf.Abs(myRigidbody2D.velocity.x) > 0);
    }
}
