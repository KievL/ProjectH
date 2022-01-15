using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBehaviour : MonoBehaviour
{
    PlayerBehaviour playerBehaviour;

    GameObject doorCollider;

    Transform doorTransform;

    bool isDoorOpen = false;
    public float crono = 0;

    public event Action OnDoorTouched;

    private bool moving = false;

    //false -> horizontal || true -> vertical
    [SerializeField] private bool doorOrientation;

    private float rotationAcumulation = 0;

    // Start is called before the first frame update
    void Start()
    {
        playerBehaviour = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerBehaviour>();
        doorCollider = GameObject.Find("collider_" + this.name);
        doorTransform = this.transform;

    }

    // Update is called once per frame
    void Update()
    {
        OpeningDoor();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerBehaviour.currentInteractionPlace = this.transform.position;
        }
    }

    public IEnumerator interactDoor(bool isPlayer)
    {        
        yield return null;
        if (!moving)
        {
            moving = true;
            rotationAcumulation =0f;
            isDoorOpen = !isDoorOpen;
            this.GetComponent<Collider2D>().enabled = false;
        }

        if (isPlayer == true)
        {
            OnDoorTouched();
        }
                
    }   
    
    private void OpeningDoor()
    {
        if (moving)
        {
            if (isDoorOpen)
            {
                doorTransform.Rotate(0, 0, 5f);
            }
            else
            {
                doorTransform.Rotate(0, 0, -5f);
            }
            rotationAcumulation += 5;
            if (rotationAcumulation == 90f)
            {
                moving = false;
                this.GetComponent<Collider2D>().enabled = true;
            }
        }
    }
}
