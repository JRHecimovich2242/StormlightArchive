using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    [SerializeField] ParticleSystem DestructionEffect;
    [SerializeField] float _breakingForce;


    Rigidbody2D myRigidbody2D;
    BoxCollider2D triggerCollider;
    CapsuleCollider2D myCapsuleCollider;
    // Start is called before the first frame update
    void Start()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        triggerCollider = GetComponent<BoxCollider2D>();
        myCapsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void DestroyBreakable()
    {
        Instantiate(DestructionEffect, gameObject.transform.position, new Quaternion());
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log(collision.tag);
        if (collision.tag == "Interactable Trigger")
        {
            Debug.Log("Interactable Trigger hit me");
            Interactable IncomingObject = collision.GetComponentInParent<Interactable>();
            DestroyBreakable();
        }
    }
}
