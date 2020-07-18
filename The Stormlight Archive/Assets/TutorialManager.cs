using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject[] popups;
    [SerializeField] private int popUpIndex;

    [SerializeField] StormlightSource TutorialStormlightSource;
    [SerializeField] PlayerController Player;
    [SerializeField] Breakable TutorialWall;
    [SerializeField] EnemyRoomManager TutorialEnemyRoom;
    [SerializeField] Golem TutorialGolem;

    private bool _freezeTime = true;
    // Start is called before the first frame update
    void Start()
    {
       // Debug.Log("Tutorial starting");
        popUpIndex = 0;
        popups[popUpIndex].SetActive(true);
        Player.tutorialHandicap = true;
        Player.jump = false;
        Player.lash = false;
        Player.heal = false;
        Player.attack = false;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < popups.Length; i++)
        {
            if (i == popUpIndex)
            {
                //Debug.Log("Activating Popup index: " + popUpIndex);
                if (popUpIndex != 8 && popUpIndex != 7)
                {
                    popups[i].SetActive(true);
                }
                else if (TutorialEnemyRoom.GetRoomActive() && popUpIndex == 7)
                {
                    popups[i].SetActive(true);
                }
                else if (TutorialGolem.GetStunned() && popUpIndex == 8)
                {
                    popups[i].SetActive(true);
                }
            }
            else
            {
                popups[i].SetActive(false);
            }
        }

        if (popUpIndex == 0)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                popUpIndex++;
                Player.jump = true;
            }
        }
        else if (popUpIndex == 1)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                popUpIndex++;
            }
        }
        else if (popUpIndex == 2)
        {
            if (TutorialStormlightSource.GetStormlight() <= 0f)
            {
                popUpIndex++;
                Player.heal = true;
            }
        }
        else if (popUpIndex == 3)
        {
            //Check if healed -> Make sure to say you can press R to stop healing
            if (Input.GetKeyDown(KeyCode.R))
            {
                popUpIndex++;
            }
        }
        else if (popUpIndex == 4)
        {
            //Check if tutorial wall is broken
            if (!TutorialWall)
            {
                popUpIndex++;
                Player.lash = true;
            }
        }
        else if (popUpIndex == 5)
        {
            //Check if player has lashed to a surface
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
            {
                popUpIndex++;
            }
        }
        else if (popUpIndex == 6)
        {
            //Check if player has pressed Q to cancel lashings
            if (Input.GetKeyDown(KeyCode.Q))
            {
                popUpIndex++;
                Player.attack = true;
            }
        }
        else if (popUpIndex == 7)
        {

            if (_freezeTime && TutorialEnemyRoom.GetRoomActive())
            {
                Time.timeScale = 0f;
                _freezeTime = false;
            }
            //Freeze time and indicate that youve entered a barricaded room
            //Player can press F to continue
            //Player can press E to attack
            if (Input.GetKeyDown(KeyCode.E))
            {
                Time.timeScale = 1f;
                popUpIndex++;
            }
        }
        else if (popUpIndex == 8)
        {
            //Freeze when golem is stunned
            if (TutorialGolem.GetStunned())
            {
                Time.timeScale = 0f;
            }
            //After damaging enemies enough, some may get stunned. When enemies are stunned they will begin to flash yellow. When an enemy is stunned you can grab them and lash them just like rocks. But be careful, after a short time they will begin to attack again. Hitting a stunned enemy also un-stuns them.
            if (Input.GetKeyDown(KeyCode.F))
            {
                Time.timeScale = 1f;
                popUpIndex++;
            }
        }
        else if (popUpIndex == 9 && !TutorialGolem)
        {
            //Enemies may drop stormlight pickups to replenish your stormlight.
            if (Input.GetKeyDown(KeyCode.F))
            {
                popUpIndex++;
            }
        }
        else if (popUpIndex == 10)
        {
            //Now all thats left is to find our way out of this cave!
            if (Input.GetKeyDown(KeyCode.F))
            {
                popUpIndex++;
            }
        }
        
    }
}
