using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //State Variables
    bool isAlive = true;
    public bool isLashing = false;
    private bool airStunned = false;
    private bool _grabbing = false;
    bool _healing = false;
    //The following four booleans track which surface the player is currently lashed to
    bool leftWall = false;
    bool rightWall = false;
    bool ceiling = false;
    bool floor = true;
    bool grounded = true;
    bool needToRotate = false;

    private bool _attacking = false;
    
    Vector2 velocityBeforeLash;
    //The following 4 ints will be updated so that pressing a key always lashes in the same direction relative to the player
    const int UP = 'w';
    const int DOWN = 's';
    const int RIGHT = 'd';
    const int LEFT = 'a';
    int relativeUp = UP;
    int relativeDown = DOWN;
    int relativeRight = RIGHT;
    int relativeLeft = LEFT;
    const float FLOORED_ANGLE = 0f;
    const float CEILING_ANGLE = 180f;
    const float RIGHT_ANGLE = 90f;
    const float LEFT_ANGLE = -90f;



    //Config Variables
    //Config variables
    [SerializeField] float _maxHealth = 100f;
    [SerializeField] float _health;
    [SerializeField] float _maxStormlight = 100f;
    [SerializeField] float _stormlight = 0f;
    [SerializeField] float _stormlightDrainRate = 1f; //Per update drain while lashing
    [SerializeField] float stormlightHealRate = 5f;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] float lashingFactor = .5f;
    [SerializeField] float hoverForce = 1f;
    [SerializeField] int lashingCost = 5;
    [SerializeField] float continuousLashDecay = .5f;
    [SerializeField] float horizLashSpeed = 5f;
    [SerializeField] float launchPower = 3f;
    float swordDamage;
    [SerializeField] float lashCooldown = 2f;
    [SerializeField] float attackCooldown = .5f;
    [SerializeField] float airStunCooldown = 1.5f;
    private float airStunTime = 0f;
    [SerializeField] ParticleSystem HitEffects;
    float timeSinceLash = 0f;
    float timeBetweenAttacks = 0f;
    private Vector2 _move;

    private LashHandler _lashHandler; //Lashhandler of grabbed object
    private GameObject grabbedObject;

    [SerializeField] AudioClip[] footstepSounds;
    [SerializeField] AudioClip deathSound;
    [SerializeField] AudioClip painSound;
    [SerializeField] AudioClip[] swordSwingSounds;
    [SerializeField] ParticleSystem stormlightBurst;

    //Cached Component References
    //Cached component references
    Rigidbody2D myRigidBody;
    Animator myAnimator;
    Collider2D myCapsuleCollider2D;
    BoxCollider2D myBoxCollider2D;
    ConstantForce2D myConstantForce2D;
    ObjectRotator myObjectRotator;
    AudioSource myAudioSource;
    ParticleSystem stormlightCloud;
    private bool emitting = false;

    public bool jump = true;
    public bool lash = true;
    public bool heal = true;
    public bool attack = true;
    public bool tutorialHandicap = false;


    // Start is called before the first frame update
    void Start()
    {
        myObjectRotator = GetComponent<ObjectRotator>();
        myRigidBody = GetComponent<Rigidbody2D>(); myAnimator = GetComponent<Animator>();
        myConstantForce2D = GetComponent<ConstantForce2D>();
        myCapsuleCollider2D = GetComponent<CapsuleCollider2D>();
        myBoxCollider2D = GetComponentInChildren<BoxCollider2D>();
        myAnimator = GetComponent<Animator>();
        myAudioSource = GetComponent<AudioSource>();
        //_health = _maxHealth;
        stormlightCloud = GetComponentInChildren<ParticleSystem>();
        if(_stormlight > 0)
        {
            stormlightCloud.Play();
        }
    }


    // Update is called once per frame
    void Update()
    {
        
        if (!isAlive) { return; }
        UpdateTimers();
        if(_stormlight > 0 & _health < _maxHealth & heal)
        {
            Heal();
        }
        if (isLashing)
        {
            _stormlight -= _stormlightDrainRate * Time.fixedDeltaTime;
            if (_stormlight <= 0f)
            {
                if (tutorialHandicap)
                {
                    _stormlight += _maxStormlight;
                }
                else
                {
                    Lash(DOWN);
                    isLashing = false;
                }
                
                
            }
        }
        if(_stormlight <= 0)
        {
            stormlightCloud.Pause();
            emitting = false;
            _healing = false;
            _stormlight = 0f;
        }
        


        if (!_attacking & !_grabbing)
        {
            if (!airStunned & lash)
            {
                LashInput();
            }
            //Move();
            if (Input.GetKeyDown(KeyCode.E) & attack)
            {
                myAnimator.SetTrigger("attack");
            }
        }
        Move();

        if (_grabbing)
        {
            if(grabbedObject.transform.parent.tag != null)
            {
                if (grabbedObject.transform.parent.tag == "Enemy")
                {
                    LashGrabbedEnemy();
                }
                else if (grabbedObject.transform.parent.tag == "Interactable")
                {
                    LashGrabbedInteractable();
                }
            }
        }
    }

    private void Heal()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            _healing = !_healing; // Toggle healing when the healing key is pressed
        }
        if (_healing)
        {
            _stormlight -= stormlightHealRate * Time.fixedDeltaTime;
            if(_stormlight <= 0f && tutorialHandicap)
            {
                _stormlight = _maxStormlight;
            }
            _health += stormlightHealRate * Time.fixedDeltaTime;
            if (_health > _maxHealth)
            {
                _health = _maxHealth;
                _healing = false;
            }
        }
    }

    public float GetHealthFraction()
    {
        return _health / _maxHealth;
    }

    public float GetStormlightFraction()
    {
        return _stormlight / _maxStormlight;
    }

    private void Move()
    {
        //myBoxCollider2D.
        if (myBoxCollider2D.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            grounded = true;
            myAnimator.SetBool("falling", false);
            myAnimator.SetBool("grounded", true);
        }
        else
        {
            grounded = false;
            myAnimator.SetBool("falling", true);
            myAnimator.SetBool("grounded", false);
        }
        if (Input.GetKeyDown(KeyCode.Space) & grounded & !_attacking & !_grabbing & jump)
        {
            Jump();
        }
        Run();
        
    }

    private void LashInput()
    {
        if(_stormlight <= 0f)
        {
            return;
        }


        //throw new NotImplementedException();
        //So we need 4 lash cases plus a cancellation
        //Relative Up
        //Relative Right
        //Relative Left
        //Relative Down (will do nothing)
        if (Input.GetKeyDown(KeyCode.W)) //We want to lash to the relative up
        {
            Lash(relativeUp);
        }
        else if (Input.GetKeyDown(KeyCode.S)) //We want to lash to the relative down
        {
            Lash(relativeDown);
        }
        else if (Input.GetKeyDown(KeyCode.D)) //We want to lash to the relative right
        {
            Lash(relativeRight);
        }
        else if (Input.GetKeyDown(KeyCode.A))//We want to lash to the relative left
        {
            Lash(relativeLeft);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            Lash(DOWN);
        }
        
    }

    private void Lash(int direction)
    {
        
        velocityBeforeLash = myRigidBody.velocity;
        switch (direction)
        {
            case UP:
                if (!myObjectRotator.RotateObject(CEILING_ANGLE))
                {
                    break;
                }
                myRigidBody.gravityScale = -1f;
                myConstantForce2D.force = Vector2.zero;
                isLashing = true;
                OnCeiling();
                break;
            case DOWN:
                if (!myObjectRotator.RotateObject(FLOORED_ANGLE))
                {
                    break;
                }
                myRigidBody.gravityScale = 1f;
                myConstantForce2D.force = Vector2.zero;
                isLashing = false;
                OnFloor();
                break;
            case RIGHT:
                if (!myObjectRotator.RotateObject(RIGHT_ANGLE))
                {
                    break;
                }
                myRigidBody.gravityScale = 0f;
                myConstantForce2D.force = Vector2.right * (myRigidBody.mass * 9.81f);
                isLashing = true;
                OnRightWall();
                break;
            case LEFT:
                if (!myObjectRotator.RotateObject(LEFT_ANGLE))
                {
                    break;
                }
                myRigidBody.gravityScale = 0f;
                myConstantForce2D.force = Vector2.left * (myRigidBody.mass * 9.81f);
                isLashing = true;
                OnLeftWall(); //Turn these into coroutines that only call the orientation changers only when no direction key is held OR the player lands OR just put a "wait for release" coroutine before 
                break;
            default:
                break;
        }
    }

    private void Run()
    {
        float controlThrow = Input.GetAxis("Horizontal");
        if (controlThrow == 0 | _attacking | _grabbing & !Input.GetKeyDown(KeyCode.Space))
        {
            DampenVelocity();
        }
        else if (floor) //Handle movement on floor
        {
            Vector2 playerVelocity = new Vector2(controlThrow * moveSpeed + velocityBeforeLash.x, myRigidBody.velocity.y);
            if (Mathf.Sign(controlThrow) != Mathf.Sign(velocityBeforeLash.x)) { velocityBeforeLash.x += controlThrow * moveSpeed * .5f; }
            myRigidBody.velocity = playerVelocity;
            myAnimator.SetBool("running", Mathf.Abs(myRigidBody.velocity.x) > 0.1f);
            transform.localScale = new Vector2(Mathf.Sign(myRigidBody.velocity.x), 1);
        }
        else if (ceiling)//handle movement on ceiling
        {
            controlThrow = -1 * controlThrow;
            Vector2 playerVelocity = new Vector2(controlThrow * moveSpeed + velocityBeforeLash.x, myRigidBody.velocity.y);
            if (Mathf.Sign(controlThrow) != Mathf.Sign(velocityBeforeLash.x)) { velocityBeforeLash.x += controlThrow * moveSpeed * .5f; }
            myRigidBody.velocity = playerVelocity;
            myAnimator.SetBool("running", Mathf.Abs(myRigidBody.velocity.x) > 0.1f);
            transform.localScale = new Vector2(-1 * Mathf.Sign(myRigidBody.velocity.x), 1);

        }
        else if (rightWall) //handle movement on right wall
        {
            Vector2 playerVelocity = new Vector2(myRigidBody.velocity.x, controlThrow * moveSpeed + velocityBeforeLash.y);
            if (Mathf.Sign(controlThrow) != Mathf.Sign(velocityBeforeLash.y)) { velocityBeforeLash.y += controlThrow * moveSpeed * .5f; }
            myRigidBody.velocity = playerVelocity;
            myAnimator.SetBool("running", Mathf.Abs(myRigidBody.velocity.y) > 0.1f);
            transform.localScale = new Vector2(Mathf.Sign(myRigidBody.velocity.y), 1);
        }
        else if (leftWall) //handle movement on left wall
        {
            controlThrow = -1 * controlThrow;
            Vector2 playerVelocity = new Vector2(myRigidBody.velocity.x, controlThrow * moveSpeed + velocityBeforeLash.y);
            if (Mathf.Sign(controlThrow) != Mathf.Sign(velocityBeforeLash.y)) { velocityBeforeLash.y += controlThrow * moveSpeed * .5f; }
            myRigidBody.velocity = playerVelocity;
            myAnimator.SetBool("running", Mathf.Abs(myRigidBody.velocity.y) > 0.1f);
            transform.localScale = new Vector2(-1 * Mathf.Sign(myRigidBody.velocity.y), 1);
        }
        
    }

    private void DampenVelocity() //When no input is recieved and player is on ground, slow player to a stop
    {
        if (grounded)
        {
            if (_attacking)
            {
                //Debug.Log("Slowing for attack");
                myRigidBody.velocity *= .9f;
                return;
            }
            myRigidBody.velocity = myRigidBody.velocity * .9f;
            if(floor | ceiling)
            {
                if(Mathf.Abs(myRigidBody.velocity.x) < .5f)
                {
                    myRigidBody.velocity = new Vector2(0f, myRigidBody.velocity.y);
                    velocityBeforeLash = Vector2.zero;
                    myAnimator.SetBool("running", Mathf.Abs(myRigidBody.velocity.x) > 0.1f);
                }
            }
            else if(rightWall | leftWall)
            {
                if(Mathf.Abs(myRigidBody.velocity.y) < .5f)
                {
                    myRigidBody.velocity = new Vector2(myRigidBody.velocity.x, 0f);
                    velocityBeforeLash = Vector2.zero;
                    myAnimator.SetBool("running", Mathf.Abs(myRigidBody.velocity.y) > 0.1f);
                }
            }
            
        }
    }

    private void Jump()
    {
        myAnimator.SetTrigger("jump");
        StartCoroutine(DisableFootHitbox());
        Vector2 jumpVelocityToAdd = new Vector2(0f, jumpSpeed);
        if (floor)
        {
            myRigidBody.velocity += jumpVelocityToAdd;
        }
        else if (ceiling)
        {
            jumpVelocityToAdd = -1f * jumpVelocityToAdd;
            myRigidBody.velocity += jumpVelocityToAdd;
        }
        else if (rightWall)
        {
            jumpVelocityToAdd = new Vector2(-1 * jumpSpeed, 0f);
            myRigidBody.velocity += jumpVelocityToAdd;
        }
        else if (leftWall)
        {
            jumpVelocityToAdd = new Vector2(jumpSpeed, 0f);
            myRigidBody.velocity += jumpVelocityToAdd;
        }
    }




    public void OnHit(float damage, Vector3 attackerPosition, bool knockback)
    {
        //float knockbackScale = 1f;
        myAudioSource.PlayOneShot(painSound);
        //Debug.Log("Player hit");
        Instantiate(HitEffects, gameObject.transform.position, new Quaternion());
        StartCoroutine(HurtFlash(Color.red)); //When you get a chance, use a less dark red
        _health -= damage;
        if(_health <= 0 & !tutorialHandicap)
        {
            //Death
            HandleDeath();
        }
        else if(tutorialHandicap & _health <= 0){
            _health += damage;
        }
        if (knockback)
        {
            float direction = Mathf.Sign(attackerPosition.x - transform.position.x); //If negative, knockback to the right. Otherwise knockback to the left
            if (floor)
            {
                if (direction < 0) //This needs to be adjusted for knockback on walls?
                {
                    //Knockback to the right
                    myRigidBody.AddForce(new Vector2(launchPower, launchPower * .5f), ForceMode2D.Impulse);
                }
                else
                {
                    //knockback to the left
                    myRigidBody.AddForce(new Vector2(-1f * launchPower, launchPower * .5f), ForceMode2D.Impulse);
                }
            }
            else if (ceiling)
            {
                if (direction < 0) //This needs to be adjusted for knockback on walls?
                {
                    //Knockback to the right
                    myRigidBody.AddForce(new Vector2(launchPower, launchPower * -.5f), ForceMode2D.Impulse);
                }
                else
                {
                    //knockback to the left
                    myRigidBody.AddForce(new Vector2(-1f * launchPower, launchPower * -.5f), ForceMode2D.Impulse);
                }
            }
            else
            {
                direction = Mathf.Sign(attackerPosition.y - transform.position.y);
                if (rightWall)
                {
                    if(direction < 0)
                    {
                        myRigidBody.AddForce(new Vector2(launchPower * .5f, launchPower), ForceMode2D.Impulse);
                    }
                    else
                    {
                        myRigidBody.AddForce(new Vector2(launchPower * .5f, -1f * launchPower), ForceMode2D.Impulse);
                    }
                }
                else if (leftWall)
                {
                    if (direction < 0)
                    {
                        myRigidBody.AddForce(new Vector2(launchPower * 1.5f, launchPower), ForceMode2D.Impulse);
                    }
                    else
                    {
                        myRigidBody.AddForce(new Vector2(launchPower * 1.5f, -1f * launchPower), ForceMode2D.Impulse);
                    }
                }
            }
        }
        //Stun effect, reset lashing if in air
        if (!grounded)
        {
            //Lash(DOWN);
            airStunned = true;
            airStunTime = Time.time;
            myAnimator.SetTrigger("hurt");
            //myAnimator.ResetTrigger("hurt");
        }
    }

    private void UpdateTimers()
    {
        //Air stun timer
        if(Time.time - airStunTime >= airStunCooldown)
        {
            airStunned = false;
        }
    }

    private void HandleDeath()
    {
        isAlive = false;
        Instantiate(stormlightBurst, transform.position, new Quaternion());
        GetComponentInChildren<SpriteRenderer>().enabled = false;
        Time.timeScale = .5f;
        
        StartCoroutine(WaitToFail(1.5f));
        Destroy(gameObject, 1.7f);

    }



    IEnumerator WaitToFail(float time)
    {
        yield return new WaitForSeconds(time);
        FindObjectOfType<GameSession>().PlayerDeath();
    }

    IEnumerator HurtFlash(Color flashColor) //Called by OnHit, will temporarily darken the player sprite to show the player getting hurt by an enemy
    {
        
        SpriteRenderer mySpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        mySpriteRenderer.color = flashColor;
        yield return new WaitForSeconds(.1f);
        mySpriteRenderer.color = Color.white;
    }

    public void FreezePlayer()
    {
        //myRigidBody.constraints = RigidbodyConstraints2D.FreezePositionX;
        //myRigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
        _attacking = true;
       // Debug.Log("Player frozen");
        
    }
    public void UnfreezePlayer()
    {
        //Debug.Log("Player unfrozen");
        _attacking = false;
        //myRigidBody.constraints = RigidbodyConstraints2D.None;
        //myRigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
        myAnimator.ResetTrigger("attack");

    }

    IEnumerator DisableFootHitbox() //Velocity is dampened when grounded which sometimes cancels jump. This disables the foot hitbox to stop from being grounded right at jump start
    {
        myBoxCollider2D.enabled = false;
        yield return new WaitForSeconds(.2f);
        myBoxCollider2D.enabled = true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.tag == "Stun Trigger")
        {
            //Debug.Log("Player in stun trigger");
            
            if (collision.GetComponentInParent<LashHandler>())
            {
                //Debug.Log("Got the lash handler");
                if (Input.GetKey(KeyCode.LeftShift) & grounded)
                {
                    if (!_grabbing)
                    {
                        _grabbing = true;
                        myAnimator.SetBool("grabbing", true);
                        _lashHandler = collision.gameObject.GetComponentInParent<LashHandler>();
                        grabbedObject = collision.gameObject;
                        //myAnimator.SetBool("grabbing", true);
                    }
                }
                else
                {
                    _grabbing = false;
                    myAnimator.SetBool("grabbing", false);
                }
            }
        }
        else if(collision.tag == "Interactable Trigger")
        {
            if (collision.GetComponentInParent<LashHandler>())
            {
                //Debug.Log("Got the lash handler");
                if (Input.GetKey(KeyCode.LeftShift) & grounded)
                {
                    if (!_grabbing)
                    {
                        _grabbing = true;
                        myAnimator.SetBool("grabbing", true);
                        _lashHandler = collision.gameObject.GetComponentInParent<LashHandler>();
                        grabbedObject = collision.gameObject;
                       
                    }
                }
                else
                {
                    _grabbing = false;
                    myAnimator.SetBool("grabbing", false);
                }
            }
        }
        else if(collision.tag == "Source")
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                Debug.Log("Absorbing");
                AbsorbStormlight(collision.gameObject.GetComponent<StormlightSource>().TakeStormlight(_maxStormlight - _stormlight));
            }
        }
    }

    private void LashGrabbedEnemy()
    {
        if(_stormlight >= _lashHandler.ObjectLashCost)
        {
            if (Input.GetKeyDown(KeyCode.W)) //We want to lash to the relative up
            {
                _lashHandler.Lash(relativeUp, true);
                grabbedObject.GetComponentInParent<IEnemy>().SetLashing(true, _lashHandler.ObjectLashCost);
                _stormlight -= _lashHandler.ObjectLashCost;
            }
            else if (Input.GetKeyDown(KeyCode.S)) //We want to lash to the relative down
            {
                _lashHandler.Lash(relativeDown, true);
                grabbedObject.GetComponentInParent<IEnemy>().SetLashing(true, _lashHandler.ObjectLashCost);
                _stormlight -= _lashHandler.ObjectLashCost;
            }
            else if (Input.GetKeyDown(KeyCode.D)) //We want to lash to the relative right
            {
                _lashHandler.Lash(relativeRight, true);
                grabbedObject.GetComponentInParent<IEnemy>().SetLashing(true, _lashHandler.ObjectLashCost);
                _stormlight -= _lashHandler.ObjectLashCost;
            }
            else if (Input.GetKeyDown(KeyCode.A))//We want to lash to the relative left
            {
                _lashHandler.Lash(relativeLeft, true);
                grabbedObject.GetComponentInParent<IEnemy>().SetLashing(true, _lashHandler.ObjectLashCost);
                _stormlight -= _lashHandler.ObjectLashCost;
            }
        }
    }

    private void LashGrabbedInteractable()
    {
        if(_stormlight >= _lashHandler.ObjectLashCost)
        {
            if (Input.GetKeyDown(KeyCode.W)) //We want to lash to the relative up
            {
                _lashHandler.Lash(relativeUp, false);
                grabbedObject.GetComponentInParent<Interactable>().LashInteractable(true, _lashHandler.ObjectLashCost);
                _stormlight -= _lashHandler.ObjectLashCost;
            }
            else if (Input.GetKeyDown(KeyCode.S)) //We want to lash to the relative down
            {
                _lashHandler.Lash(relativeDown, false);
                grabbedObject.GetComponentInParent<Interactable>().LashInteractable(true, _lashHandler.ObjectLashCost);
                _stormlight -= _lashHandler.ObjectLashCost;
            }
            else if (Input.GetKeyDown(KeyCode.D)) //We want to lash to the relative right
            {
                _lashHandler.Lash(relativeRight, false);
                grabbedObject.GetComponentInParent<Interactable>().LashInteractable(true, _lashHandler.ObjectLashCost);
                _stormlight -= _lashHandler.ObjectLashCost;
            }
            else if (Input.GetKeyDown(KeyCode.A))//We want to lash to the relative left
            {
                _lashHandler.Lash(relativeLeft, false);
                grabbedObject.GetComponentInParent<Interactable>().LashInteractable(true, _lashHandler.ObjectLashCost);
                _stormlight -= _lashHandler.ObjectLashCost;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Stun Trigger")
        {
            _grabbing = false;
            myAnimator.SetBool("grabbing", false);
            //myAnimator.SetBool("grabbing", false);
        }
        if (collision.tag == "Interactable Trigger" | collision.tag == "Interactable")
        {
            _grabbing = false;
            myAnimator.SetBool("grabbing", false);
            //myAnimator.SetBool("grabbing", false);
        }
    }

    public void AbsorbStormlight(float stormlight)
    {
        Debug.Log(stormlight);
        
        _stormlight += stormlight;
        if(_stormlight > _maxStormlight)
        {
            _stormlight = _maxStormlight;
        }
        if(_stormlight > 0 & !emitting)
        {
            stormlightCloud.Play();
            emitting = true;
        }
    }

    public void PlayFootstepSound()
    {
        myAudioSource.PlayOneShot(footstepSounds[UnityEngine.Random.Range(0, footstepSounds.Length)]);
    }
    public void PlaySwordSound()
    {
        myAudioSource.PlayOneShot(swordSwingSounds[UnityEngine.Random.Range(0, swordSwingSounds.Length)]);
    }


    //The four following functions update the orientation booleans for the player
    private void OnRightWall()
    {
        ceiling = false;
        floor = false;
        leftWall = false;
        rightWall = true;
        relativeUp = LEFT;
        relativeDown = RIGHT;
        relativeRight = UP;
        relativeLeft = DOWN;

    }
    private void OnLeftWall()
    {
        ceiling = false;
        floor = false;
        leftWall = true;
        rightWall = false;
        relativeUp = RIGHT;
        relativeDown = LEFT;
        relativeRight = DOWN;
        relativeLeft = UP;

    }

    private void OnFloor()
    {
        ceiling = false;
        floor = true;
        leftWall = false;
        rightWall = false;
        relativeUp = UP;
        relativeDown = DOWN;
        relativeRight = RIGHT;
        relativeLeft = LEFT;

    }

    private void OnCeiling()
    {
        ceiling = true;
        floor = false;
        leftWall = false;
        rightWall = false;
        relativeUp = DOWN;
        relativeDown = UP;
        relativeRight = LEFT;
        relativeLeft = RIGHT;
    }
}
