using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    //Rotation Definitions
    const float FLOORED_ANGLE = 0f;
    const float CEILING_ANGLE = 180f;
    const float RIGHT_ANGLE = 90f;
    const float LEFT_ANGLE = -90f;


    private bool busyRotating = false;

    [SerializeField] float rotationSpeed = 700f;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool RotateObject(float goalRotation) //Returns a bool to inform the caller if it was able to set rotation or not
    {
        if (!busyRotating)
        {
            //Debug.Log("RotateObject was called");
            busyRotating = true;
            float startRotation = transform.eulerAngles.z;
            float distance = Mathf.Abs(startRotation - goalRotation);
            if (distance > 180f)
            {
                distance = 360f - distance;
                startRotation = startRotation - 360f;
            }
            StartCoroutine(LerpRotation(startRotation, goalRotation, distance, Time.time));
            //Debug.Log("LerpRotation was just called");
            return true;
        }
        return false;
    }

    IEnumerator LerpRotation(float startRotation, float goalRotation, float distance, float startTime)
    {
        //Debug.Log("Entered LerpRotation");
        //Debug.Log("The starting rotation is: " + startRotation);
        //bool greater = startRotation > goalRotation; //A bool that will let us check if we have rotated too far
        //Debug.Log(transform.eulerAngles.z - goalRotation);
        while (Mathf.Abs(transform.eulerAngles.z - goalRotation) > Mathf.Epsilon)
        {
            //Debug.Log("Looping");
            //Debug.Log(Mathf.Abs(transform.eulerAngles.z - goalRotation));
            //Debug.Log("The current rotation is: " + transform.eulerAngles.z);
            //if((transform.eulerAngles.z > goalRotation) != greater)
            //{
            //    break;
            //}
            //float distanceRotated = Mathf.Abs(transform.eulerAngles.z - startRotation);
            float distanceRotated = (Time.time - startTime) * rotationSpeed;
            float fractionRotated = distanceRotated / distance;
            if (fractionRotated > 1f)
            {
                fractionRotated = 1f;
            }
            float newRotation = Mathf.Lerp(startRotation, goalRotation, fractionRotated);
            //Debug.Log("The new rotation is: " + newRotation);
            transform.eulerAngles = new Vector3(transform.rotation.x, transform.rotation.y, newRotation);
            //Debug.Log("The current rotation is: " + transform.eulerAngles.z);
            if (fractionRotated == 1f) { break; }
            yield return null;
        }
        //transform.
        transform.eulerAngles = new Vector3(transform.rotation.x, transform.rotation.y, goalRotation);
        busyRotating = false;
        //Debug.Log("Exiting LerpRotation");
        yield return null;
    }
}
