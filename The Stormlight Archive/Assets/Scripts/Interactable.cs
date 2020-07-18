using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Interactable : MonoBehaviour
{
    const int UP = 'w';
    const int DOWN = 's';
    const int RIGHT = 'd';
    const int LEFT = 'a';

    [SerializeField] float _objectLashCost = 10f;
    [SerializeField] float _terminalVelocity = 5f;
    [SerializeField] ParticleSystem DestructionEffect;

    private float _stormlightHeld = 0f;
    private bool _isLashed = false;
     


    Rigidbody2D myRigidbody2D;
    ConstantForce2D myConstantForce2D;
    LashHandler myLashHandler;
    CapsuleCollider2D triggerCollider;
    // Start is called before the first frame update
    void Start()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        myConstantForce2D = GetComponent<ConstantForce2D>();
        myLashHandler = GetComponent<LashHandler>();
        triggerCollider = GetComponentInChildren<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_isLashed)
        {
            _stormlightHeld -= Time.deltaTime;
            if(_stormlightHeld <= 0)
            {
                LashInteractable(false, 0f);
            }
        }
    }

    public bool GetLashing()
    {
        return _isLashed;
    }

    public void LashInteractable(bool value, float stormlight)
    {
        _isLashed = value;
        _stormlightHeld = stormlight;
        if (_isLashed)
        {
            //triggerCollider.enabled = false;
            myRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        }
        else
        {
            triggerCollider.enabled = true;
            myRigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        }
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isLashed & collision.gameObject.tag == "Enemy")
        {
            Debug.Log(myRigidbody2D.velocity.sqrMagnitude);
            collision.gameObject.GetComponent<IEnemy>().OnHit(myRigidbody2D.velocity.sqrMagnitude, transform.position);
            if(myRigidbody2D.velocity.sqrMagnitude >= _terminalVelocity)
            {
                DestroyInteractable();
            }
            else
            {
                myLashHandler.Lash(DOWN, true);
                LashInteractable(false, 0f);
            }
        }
        else if(_isLashed & collision.gameObject.tag == "Breakable")
        {
            DestroyInteractable();
        }
        
    }

    private void DestroyInteractable()
    {
        //Play sound
        Instantiate(DestructionEffect, gameObject.transform.position, new Quaternion());
        Destroy(gameObject);
    }
}
