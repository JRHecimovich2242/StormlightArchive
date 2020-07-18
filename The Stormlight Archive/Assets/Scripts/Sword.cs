using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] float _swordDamage = 50f;
    

   

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("Trigger entered");
        if(collision.tag == "Enemy")
        {
            collision.GetComponent<IEnemy>().OnHit(_swordDamage, transform.position);
        }
    }


}
