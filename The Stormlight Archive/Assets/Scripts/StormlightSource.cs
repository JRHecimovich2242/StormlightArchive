using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormlightSource : MonoBehaviour
{
    [SerializeField] ParticleSystem stormlightBurst;
    [SerializeField] float _heldStormlight = 30f;

    ParticleSystem myStormlightCloud;
    Animator myAnimator;
    // Start is called before the first frame update
    void Start()
    {
        myStormlightCloud = GetComponentInChildren<ParticleSystem>();
        myStormlightCloud.Play();
        myAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float GetStormlight()
    {
        return _heldStormlight;
    }

    //TakeStormlight returns to the caller (PlayerController) how much stormlight it could supply
    public float TakeStormlight(float amount) //Amount is the total stormlight the caller (player) is missing
    {
        Debug.Log("Player taking my stormlight");
        if(amount >= _heldStormlight)
        {
            float returnValue = _heldStormlight;
            _heldStormlight = 0f;
            myStormlightCloud.Stop();
            GetComponent<CircleCollider2D>().enabled = false;
            Instantiate(stormlightBurst, transform.position, new Quaternion());
            GetComponent<SpriteRenderer>().color = Color.gray;
            return returnValue;

        }
        else
        {
            Instantiate(stormlightBurst, transform.position, new Quaternion());
            _heldStormlight -= amount;
            return amount;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player" & _heldStormlight > 0f)
        {
            myAnimator.SetBool("flash", true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            myAnimator.SetBool("flash", false);
        }
    }
}
