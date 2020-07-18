using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Golem : MonoBehaviour, IEnemy
{
    [SerializeField] float _health = 200f;
    [SerializeField] float _stunThreshold = 75f;
    [SerializeField] float _stunThresholdDecrementFactor = .333f; //How much the stun threshold is decreased each hit
    //[SerializeField] float _stunTime = 3f;
    [SerializeField] float _knockbackDamper = .2f;
    [SerializeField] ParticleSystem HitEffects;
    [SerializeField] ParticleSystem SmashEffects;
    [SerializeField] float _attackDamage = 50f;
    [SerializeField] float _attackCooldown = 1f;
    [SerializeField] float _hitKnockbackDuration = .2f;
    [SerializeField] float _stunDuration = 3f;
    [SerializeField] float deathCooldown = 3f;
    [SerializeField] Vector2 raycastOffset = new Vector2(0f,0f);

    [SerializeField] StormlightPickup _pickupDrop;
    [SerializeField] float _minPickupValue = 10f;
    [SerializeField] float _maxPickupValue = 20f;
    public float _detectionRange = 1f;
    public float _attackRange = 1f;
    public float _moveSpeed = 1f;

    private float _stunThresholdConstant;
    
    private float hitStartTime = 0f;
    private float stunStartTime = 0f;
    private bool cooldown = false;
    private bool isAlive = true;
    private bool _stunned = false;
    private bool _recovering = false;
    private bool _grabbed = false;
    private bool _lashed = false;
    private bool _hit = false;
    private float _wallBuffer = 2f;
    private bool _facingWallOrLedge = false;
    [SerializeField] float launchPower = 3f;
    private float _launch = 0f;
    [SerializeField] float _recoveryTime = 2f;
    private float _recoveryCounter = 0f;
    private float _stunCounter = 0f;
    private bool _followingPlayer = false;
    private bool _attackPlayer = false;
    private float _playerPosition = 0f; //_playerPosition will be positive if the player is on the right and negative if on the left, 0 if directly above or below.
    private PlayerController _player;
    private float _playerDistance;
    private float heldStormlight = 0f;

    private Vector2 _move;

    [SerializeField] AudioClip[] hitSounds;
    [SerializeField] AudioClip deathSound;
    [SerializeField] AudioClip painSound;
    [SerializeField] AudioClip[] footstepSounds;

    Rigidbody2D myRigidbody2D;
    Animator myAnimator;
    CapsuleCollider2D myCapsuleCollider2D;
    SpriteRenderer mySpriteRenderer;
    CircleCollider2D myStunTrigger;
    LashHandler myLashHandler;
    AudioSource myAudioSource;

    // Start is called before the first frame update
    void Start()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myCapsuleCollider2D = GetComponent<CapsuleCollider2D>();
        _player = FindObjectOfType<PlayerController>();
        _stunThresholdConstant = _stunThreshold;
        mySpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        myStunTrigger = GetComponentInChildren<CircleCollider2D>();
        myLashHandler = GetComponent<LashHandler>();
        myAudioSource = GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {
        _move = new Vector2(0f, myRigidbody2D.velocity.y);
        if (!isAlive) { return; }
        heldStormlight -= Time.deltaTime;
        if (!_lashed)
        {
            SearchForPlayer();
            Move();

            ApplyMovement();
        }
        else
        {
            heldStormlight -= Time.deltaTime;
            if(heldStormlight <= 0)
            {
                _lashed = false;
            }
        }
                            //May need to move this to late update, it depends when OnTriggerEnter is called during the order of operations
                            //We want _move to feature both the goal velocity from movement and the knockback input.
                            //If onTriggerEnter is called before update, its affect will be overwritten
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
        Gizmos.DrawWireSphere(transform.position, _attackRange);

    }



    private void SearchForPlayer()
    {
        
        _playerDistance = new Vector2(Mathf.Abs(_player.transform.position.x - transform.position.x), Mathf.Abs(_player.transform.position.y - transform.position.y)).magnitude;
        _followingPlayer = _playerDistance <= _detectionRange;
        _attackPlayer = _playerDistance <= _attackRange;

        if (_followingPlayer)
        {
            _playerPosition = Mathf.Sign(_player.transform.position.x - transform.position.x);
        }
    }
    /*//RaycastHit2D[] _rightHitArray = Physics2D.RaycastAll(new Vector2(_gameObject.transform.position.x + RaycastOffset.x, _gameObject.transform.position.y + RaycastOffset.y), Vector2.down, 1f); //Create an array of raycasts hits cast slightly to the right and straight down
            foreach (RaycastHit2D hit in _rightHitArray)
            {
                Debug.Log(hit.collider);
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("EnemyPlatforms") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Default"))
                {
                    _onSurface = true; //If any hits are on the ground layer, or enemy platform layer, we confirm we are on a surface */
    private void Move()
    {
        Debug.DrawRay(new Vector2(transform.position.x + raycastOffset.x, transform.position.y - raycastOffset.y), Vector2.down, Color.blue);
        Debug.DrawRay(new Vector2(transform.position.x, transform.position.y + raycastOffset.y), Vector2.right * transform.localScale.x, Color.green);

        RaycastHit2D[] _checkLedges = Physics2D.RaycastAll(new Vector2(transform.position.x + raycastOffset.x * transform.localScale.x , transform.position.y - raycastOffset.y), Vector2.down, 1f);
        RaycastHit2D[] _checkWalls = Physics2D.RaycastAll(new Vector2(transform.position.x + raycastOffset.x * transform.localScale.x, transform.position.y + raycastOffset.y), Vector2.right * transform.localScale.x, 1f);
        _facingWallOrLedge = false;
        foreach(RaycastHit2D hit in _checkLedges)
        {
            if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                _facingWallOrLedge = false;
                break;
            }
            _facingWallOrLedge = true;
        }
        foreach (RaycastHit2D hit in _checkWalls)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                _facingWallOrLedge = true;
                break;
            }
        }
        //Debug.Log(_facingWallOrLedge);
        //Debug.DrawRay(new Vector3(transform.position.x, transform.position.y, 0f), Vector3.right * transform.localScale.x);
        _move.y = myRigidbody2D.gravityScale * .5f * Time.deltaTime * Time.deltaTime;
        
        if (_recovering)
        {
            myAnimator.SetBool("running", false);
            _recoveryCounter += Time.deltaTime;
            _move.x = _launch;
            _launch += (0 - _launch) * Time.deltaTime * 4;
            if(_recoveryCounter >= _recoveryTime)
            {
                _recoveryCounter = 0f;
                _recovering = false;
            }
        }
        else if (_stunned)
        {
            if (!_grabbed)
            {
                //We dont want the enemy breaking out of the grab.
                //Maybe we do, but lets try this.
                //Could also be fun to have a breakout animation and damage the player then
                _stunCounter += Time.fixedDeltaTime;
            }
            _move.x = _launch;
            _launch += (0 - _launch) * Time.deltaTime * 4;
            if (_stunCounter >= _stunDuration)
            {
                _stunCounter = 0f;
                _stunned = false;
                myStunTrigger.enabled = false;
                myAnimator.SetBool("stunned", false);
            }

        }
        else if (_attackPlayer & !cooldown)
        {
            myAnimator.SetTrigger("attack");
            StartCoroutine(AttackCooldown());
        }
        else if(_attackPlayer & cooldown)
        {
            //myAnimator.SetBool("running", Mathf.Abs(myRigidbody2D.velocity.x) > 0);
            //Handle idle behavior
            //myAnimator.SetBool("running", false);
            //myAnimator.SetBool("running", Mathf.Abs(myRigidbody2D.velocity.x) > Mathf.Epsilon);
            _move *= .9f; //If time to idle, will slow the enemy down
        }
        else if (_followingPlayer & !_facingWallOrLedge)
        {
            //myAnimator.SetBool("running", Mathf.Abs(myRigidbody2D.velocity.x) > Mathf.Epsilon);
            //Move towards player, attack if player is in range.
            transform.localScale = new Vector2(_playerPosition, 1); //Face the player
            _move += new Vector2(_playerPosition * _moveSpeed, myRigidbody2D.velocity.y); //Need to account for hit knockback
            
        }
        else if(_followingPlayer & _facingWallOrLedge)
        {
            transform.localScale = new Vector2(_playerPosition, 1);
            _move *= .9f;
        }
        else
        {
            //myAnimator.SetBool("running", Mathf.Abs(myRigidbody2D.velocity.x) > 0);
            //Handle idle behavior
            //myAnimator.SetBool("running", false);
            //myAnimator.SetBool("running", Mathf.Abs(myRigidbody2D.velocity.x) > Mathf.Epsilon);
            _move = new Vector2(_move.x * .9f, _move.y); //If time to idle, will slow the enemy down
        }

    }

    private void ApplyMovement()
    {
        myRigidbody2D.velocity = _move;
        if (!_attackPlayer & !_recovering & !_stunned)
        {
            myAnimator.SetBool("running", Mathf.Abs(myRigidbody2D.velocity.x) > Mathf.Epsilon);
        }
        else if (_recovering | _stunned)
        {
            myAnimator.SetBool("running", false);
        }
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(_attackCooldown);
        cooldown = false;
    }
    public void OnHit(float damage, Vector3 attackerPosition)
    {
        myAudioSource.PlayOneShot(painSound);
        UnfreezeGolem();
        _health -= damage;
        Instantiate(HitEffects, gameObject.transform.position, new Quaternion());
        if (_stunned)
        {
            myStunTrigger.enabled = false;
            _stunned = false;
            myAnimator.SetBool("stunned", false);
            //If we hit an already stunned enemy, want them to snap back to action
        }
        if (_health <= 0f)
        {
            //Death
            HandleDeath();
            return;
        }
        else
        {
            //StopCoroutine(HurtFlash(Color.red));
            //StartCoroutine(HurtFlash(Color.red));
            if (damage >= _stunThreshold)
            {
                //Stun
                _stunThreshold = _stunThresholdConstant; //Reset the stun threshold
                _stunned = true;
                myStunTrigger.enabled = true;
                _stunCounter = 0f;
                myAnimator.SetBool("stunned", true);
                //Handle knockback
                if (attackerPosition.x < transform.position.x) //Knockback to Right
                {
                    _launch = launchPower * _knockbackDamper;
                    myRigidbody2D.velocity += new Vector2(0f, launchPower);
                }
                else //Knockback to the left
                {
                    _launch = launchPower * _knockbackDamper;
                    _launch = _launch * -1;
                    myRigidbody2D.velocity += new Vector2(0f, launchPower);
                }
            }
            else if (attackerPosition.x < transform.position.x) //Knockback to Right
            {
                _launch = launchPower * _knockbackDamper;
                hitStartTime = Time.time;
                _hit = true;
                _recovering = true;
                _recoveryCounter = 0f;
                myRigidbody2D.velocity += new Vector2(0f, launchPower);
            }
            else //Knockback to the left
            {
                _launch = launchPower * _knockbackDamper;
                _launch = _launch * -1;
                hitStartTime = Time.time;
                _hit = true;
                _recovering = true;
                _recoveryCounter = 0f;
                myRigidbody2D.velocity += new Vector2(0f, launchPower);
            }
            _stunThreshold -= _stunThresholdConstant * _stunThresholdDecrementFactor;

        }
    }

    IEnumerator HurtFlash(Color flashColor) //Called by OnHit, will temporarily darken the player sprite to show the player getting hurt by an enemy
    {
        Debug.Log("Flashing color in golem");
        //SpriteRenderer mySpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        Debug.Log(mySpriteRenderer.color);
        mySpriteRenderer.color = flashColor;
        Debug.Log(mySpriteRenderer.color);
        yield return new WaitForSeconds(1f);
        mySpriteRenderer.color = Color.white;
        Debug.Log("Golem color restored");
    }

    public void FreezeGolem()
    {
        myRigidbody2D.constraints = RigidbodyConstraints2D.FreezePositionX;
        myRigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        cooldown = true;
    }
    public void UnfreezeGolem()
    {
        myRigidbody2D.constraints = RigidbodyConstraints2D.None;
        myRigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        myAnimator.ResetTrigger("attack");
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log(_player.isLashing);
        //Debug.Log(collision.tag);

        if (collision.tag == "Player Foot" & _player.isLashing)
        {
            _player.OnHit(_attackDamage / 4, transform.position, true);
        }
        else if (collision.tag == "Player Attack Trigger" | !_attackPlayer)
        {
            return;
        }
        else if (collision.tag == "Player" & collision.tag != "Player Foot" & !_stunned)
        {
            //Debug.Log("Player entered in attack trigger");
            _player.OnHit(_attackDamage, transform.position, true);
        }
        
    }

    private void HandleDeath()
    {
        if (_pickupDrop)
        {
            Debug.Log("Dropping");
            _pickupDrop.SetPickupValue(Random.Range(_minPickupValue, _maxPickupValue));
            Instantiate(_pickupDrop, transform.position, new Quaternion());
        }
        myAudioSource.PlayOneShot(deathSound);
        isAlive = false;
        myAnimator.SetBool("stunned", false);
        myAnimator.SetTrigger("death");
        Destroy(gameObject, deathCooldown);
        myCapsuleCollider2D.enabled = false;
        myRigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public void SetLashing(bool value, float lashCost)
    {
        heldStormlight = lashCost;
        _lashed = value;
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("We be colliding yo");
        if(collision.gameObject.tag != "Player" & _lashed)
        {
            //Debug.Log("Hurt me, gravity!");
            
            if (collision.gameObject.tag == "Enemy")
            {
                //Debug.Log(collision.gameObject.tag);
                Instantiate(SmashEffects, gameObject.transform.position, new Quaternion());
                collision.gameObject.GetComponentInParent<IEnemy>().OnHit(myRigidbody2D.velocity.magnitude, transform.position);
                deathCooldown = 0f;
            }
            OnHit(_health, Vector3.zero);
        }
    }

    public void PlayFootstepSound()
    {
        myAudioSource.PlayOneShot(footstepSounds[UnityEngine.Random.Range(0, footstepSounds.Length)]);
    }

    public void PlayAttackSound()
    {
        myAudioSource.PlayOneShot(hitSounds[UnityEngine.Random.Range(0, hitSounds.Length)]);
    }

    public bool GetStunned()
    {
        return _stunned;
    }
}
