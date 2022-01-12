using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DemonAura : MonoBehaviour
{
    public List<GameObject> placesOnSight;

    public event Action OnPlacesSeenChanges;

    public event Action OnPlayerIsInAura;

    GameObject demon;

    // Start is called before the first frame update
    void Start()
    {
        demon = GameObject.FindObjectOfType<DemonBehaviour>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = demon.transform.position;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        
    }

    //Dá um uptade no lista de colliders da visão do demonio
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Places"))
        {            
            placesOnSight.Add(collision.gameObject);
            OnPlacesSeenChanges();
        }
        
    }    

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Places"))
        {
            placesOnSight.Remove(collision.gameObject);
            OnPlacesSeenChanges();
        }        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && demon.GetComponent<DemonBehaviour>().demonState == 0)
        {
            OnPlayerIsInAura();
        }
    }
}
