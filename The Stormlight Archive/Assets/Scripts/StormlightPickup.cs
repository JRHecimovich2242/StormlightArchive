using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormlightPickup : MonoBehaviour
{
    [SerializeField] float _pickupValue = 10f;
    //[SerializeField] float _minimumValue = 10f;
    //[SerializeField] float _maximumValue = 30f;

    // Start is called before the first frame update
    private void Awake()
    {
        //if(_pickupValue == 0f)
        //{
        //    _pickupValue = Random.Range(_minimumValue, _maximumValue);
        //} 
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPickupValue(float value)
    {
        _pickupValue = value;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log(collision.tag);
        if(collision.tag == "Player")
        {
            PlayerController _player = collision.GetComponent<PlayerController>();
            _player.AbsorbStormlight(_pickupValue);
            //Play some effect when picked up
            Destroy(gameObject);
        }
    }
}
