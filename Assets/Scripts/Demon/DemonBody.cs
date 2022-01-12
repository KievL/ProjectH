using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DemonBody : MonoBehaviour
{

    public event Action OnGetToDestination;

    private RandomWalk rndWalk;

    private DemonBehaviour demonBehaviour;
    private PlayerBehaviour playerBehaviour;

    public event Action OnPlayerGetCaught;

    // Start is called before the first frame update
    void Start()
    {
        GameObject demon = GameObject.Find("Demon");
        rndWalk = demon.GetComponent<RandomWalk>();
        demonBehaviour = demon.GetComponent<DemonBehaviour>();
        playerBehaviour = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerBehaviour>();

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.Equals(GameObject.Find("DestinationChecker")) && rndWalk.checkingInteraction)
        {
            OnGetToDestination();
        }

        if (collision.gameObject.CompareTag("Door") && demonBehaviour.demonState!=1)
        {
            StartCoroutine(collision.gameObject.GetComponent<DoorBehaviour>().interactDoor(false));
        }
        if (demonBehaviour.demonState == 2 && demonBehaviour.devilIsActing==false)
        {
            if(collision.gameObject==playerBehaviour.gameObject || 
                (collision.gameObject==playerBehaviour.kidBeingHelped && playerBehaviour.kidBeingHelped != null))
            {
                OnPlayerGetCaught();
            }
        }

    }
}
