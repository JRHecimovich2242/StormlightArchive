using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LashHandler : MonoBehaviour
{

    const int UP = 'w';
    const int DOWN = 's';
    const int RIGHT = 'd';
    const int LEFT = 'a';

    const float FLOORED_ANGLE = 0f;
    const float CEILING_ANGLE = 180f;
    const float RIGHT_ANGLE = 90f;
    const float LEFT_ANGLE = -90f;

    public bool _lashed = false;

    [SerializeField] float gravityScale = 1f;

    ParticleSystem stormlightCloud;

    public float ObjectLashCost = 10f;
    private float heldStormlight = 0f;


    Rigidbody2D myRigidbody2D;
    ConstantForce2D myConstantForce2D;
    ObjectRotator myObjectRotator;

    // Start is called before the first frame update
    void Start()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        myConstantForce2D = GetComponent<ConstantForce2D>();
        myObjectRotator = GetComponent<ObjectRotator>();
        if (!myObjectRotator)
        {
            Debug.Log(gameObject.name + " has no object rotator. Disabling its lashhandler");
            this.enabled = false;
            //We cant run without a rotator, so disable selt
        }

        stormlightCloud = GetComponentInChildren<ParticleSystem>();
        stormlightCloud.Pause();
    }

    // Update is called once per frame
    void Update()
    {
        heldStormlight -= Time.deltaTime;
        if(heldStormlight <= 0 & _lashed)
        {
            myRigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            //End lash
            Lash(DOWN, true);
            _lashed = false;
            heldStormlight = 0f;
            stormlightCloud.Stop();
        }
    }

    public void Lash(int direction, bool rotate)
    {

        if (!stormlightCloud.isPlaying)
        {
            stormlightCloud.Play();
        }
        _lashed = true;
        heldStormlight = ObjectLashCost;
        switch (direction)
        {
            case UP:
                if (rotate)
                {
                    if (!myObjectRotator.RotateObject(CEILING_ANGLE))
                    {
                        break;
                    }
                }
                
                myRigidbody2D.gravityScale = -1f * gravityScale;
                myConstantForce2D.force = Vector2.zero;
                myRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
                break;
            case DOWN:
                if (rotate)
                {
                    if (!myObjectRotator.RotateObject(FLOORED_ANGLE))
                    {
                        break;
                    }
                }
                myRigidbody2D.gravityScale = 1f * gravityScale;
                myConstantForce2D.force = Vector2.zero;
                myRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
                break;
            case RIGHT:
                if (rotate)
                {
                    if (!myObjectRotator.RotateObject(RIGHT_ANGLE))
                    {
                        break;
                    }
                }
                myRigidbody2D.gravityScale = 0f;
                myConstantForce2D.force = Vector2.right * (myRigidbody2D.mass * 9.81f * gravityScale);
                myRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
                break;
            case LEFT:
                if (rotate)
                {
                    if (!myObjectRotator.RotateObject(LEFT_ANGLE))
                    {
                        break;
                    }
                }
                myRigidbody2D.gravityScale = 0f;
                myConstantForce2D.force = Vector2.left * (myRigidbody2D.mass * 9.81f * gravityScale);
                myRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
                break;
            default:
                break;
        }
    }
}
