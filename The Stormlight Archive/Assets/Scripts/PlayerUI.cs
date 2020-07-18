using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject healthBar;
    [SerializeField] private float healthBarWidth;
    [SerializeField] private float healthBarSmooth;
    [SerializeField] private float healthBarSmoothEase;

    [SerializeField] private GameObject stormlightBar;
    [SerializeField] private float stormlightBarWidth;
    [SerializeField] private float stormlightBarSmooth;
    [SerializeField] private float stormlightBarSmoothEase;

    // Start is called before the first frame update
    void Start()
    {
        healthBarWidth = 1;
        healthBarSmooth = healthBarWidth;
        stormlightBarWidth = 1;
        stormlightBarSmooth = stormlightBarWidth;
    }

    // Update is called once per frame
    void Update()
    {
        healthBarWidth = FindObjectOfType<PlayerController>().GetHealthFraction();
        healthBarSmooth += (healthBarWidth - healthBarSmooth) * Time.deltaTime * healthBarSmoothEase;
        healthBar.transform.localScale = new Vector2(healthBarSmooth, transform.localScale.y);

        stormlightBarWidth = FindObjectOfType<PlayerController>().GetStormlightFraction();
        //stormlightBarSmooth += (stormlightBarWidth - stormlightBarSmooth) * Time.deltaTime * stormlightBarSmoothEase;
        stormlightBar.transform.localScale = new Vector2(stormlightBarWidth, transform.localScale.y);

    }
}
