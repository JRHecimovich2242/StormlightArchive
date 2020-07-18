using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StreamVideo : MonoBehaviour
{
    public RawImage rawImage;
    public VideoPlayer videoPlayer;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PlayVideo());
    }

    IEnumerator PlayVideo()
    {
        videoPlayer.Prepare();
        //videoPlayer.frame = 1;
        WaitForSeconds waitforSeconds = new WaitForSeconds(1f);
        while(!videoPlayer.isPrepared)
        {
            
            yield return waitforSeconds;
        }
        
        rawImage.texture = videoPlayer.texture;
        videoPlayer.Play();
        yield return new WaitForSeconds(.1f);
        FindObjectOfType<Image>().enabled = false;
    }
}
