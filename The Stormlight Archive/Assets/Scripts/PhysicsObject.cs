using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour
{
    //Physics Object as a class should handle the basic application of gravity on all physics objects in the scene
    //Should also manage and control the lashings applied to said object as in theory, all physics objects are lashable
    //Note that not all physics objects are lashable at all times.
    // Start is called before the first frame update
    public float minGroundNormalY = .65f;
    public float gravityModifier = 1f;

    protected Vector2 targetVelocity;
    protected bool grounded;
    protected Vector2 groundNormal;
    protected Rigidbody2D myRigidbody2D;
    protected Vector2 velocity;
    protected ContactFilter2D contactFilter;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);
    protected Vector2 currentOrientation = Vector2.down;
    protected int gravity = 10;
    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.01f;
    
    //public OrientationVectors myOrientation = new OrientationVectors(Vector2.right, Vector2.down);
    
    /*
    public struct OrientationVectors
    {
        Vector2 h;//Vector to store horizontal vector direction
        Vector2 v;//Vector to store vertical vector direction

        public OrientationVectors(Vector2 horizontal, Vector2 vertical)
        {
            this.h = horizontal;
            this.v = vertical;
        }

        public Vector2 GetHorizontal()
        {
            return this.h;
        }
        public Vector2 GetVertical()
        {
            return this.v;
        }
    }
    */

    void OnEnable()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useLayerMask = true;
        //OrientationVectors myOrientation = new OrientationVectors(Vector2.right, Vector2.down);
    }

    void Update()
    {
        targetVelocity = Vector2.zero;
        ComputeVelocity();
    }

    protected virtual void ComputeVelocity()
    {

    }

    void FixedUpdate()
    {
        //velocity += gravityModifier * gravity * myOrientation.GetVertical() * Time.fixedDeltaTime;
        velocity += gravityModifier * Physics2D.gravity * Time.fixedDeltaTime;
        velocity.x = targetVelocity.x;

        grounded = false;

        Vector2 deltaPosition = velocity * Time.fixedDeltaTime;

        //Store the direction we are trying to move along the ground
        Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);

        Vector2 move = moveAlongGround * deltaPosition.x;

        Movement(move, false);

        move = Vector2.up * deltaPosition.y;

        Movement(move, true);
    }

    void Movement(Vector2 move, bool vMovement)
    {
        float distance = move.magnitude;

        if (distance > minMoveDistance)
        {
            int count = myRigidbody2D.Cast(move, contactFilter, hitBuffer, distance + shellRadius);
            hitBufferList.Clear();
            for (int i = 0; i < count; i++)
            {
                hitBufferList.Add(hitBuffer[i]);
            }

            for (int i = 0; i < hitBufferList.Count; i++)
            {
                Vector2 currentNormal = hitBufferList[i].normal;
                if (currentNormal.y > minGroundNormalY)
                {
                    grounded = true;
                    if (vMovement)
                    {
                        groundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                }

                float projection = Vector2.Dot(velocity, currentNormal);
                if (projection < 0)
                {
                    velocity = velocity - projection * currentNormal;
                }

                float modifiedDistance = hitBufferList[i].distance - shellRadius;
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }


        }

        myRigidbody2D.position = myRigidbody2D.position + move.normalized * distance;
    }

}
