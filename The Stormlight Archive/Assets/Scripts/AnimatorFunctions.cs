using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorFunctions : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

    IEnumerator HurtFlash(Color flashColor)
    {
        SpriteRenderer mySpriteRenderer = GetComponent<SpriteRenderer>();
        mySpriteRenderer.color = flashColor;
        yield return new WaitForSeconds(.5f);
        mySpriteRenderer.color = Color.white;
    }
}
