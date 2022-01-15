using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doubledoor : MonoBehaviour
{
    private PlayerBehaviour playerBehaviour;
    private DemonBehaviour demonBehaviour;
    private GameObject interactDoorBtn;
    private Transform door1, door2;

    private bool isMoving = false;

    private bool isOpen = false;

    private float rotationAcumulation = 0f;
    // Start is called before the first frame update
    void Start()
    {
        playerBehaviour = FindObjectOfType<PlayerBehaviour>();
        demonBehaviour = FindObjectOfType<DemonBehaviour>();
        interactDoorBtn = GameObject.FindGameObjectWithTag("DoorBtn");
        door1 = transform.GetChild(0);
        door2 = transform.GetChild(1);

        InvokeRepeating("ShowBtns", 0f, 0.4f);
    }

    // Update is called once per frame
    void Update()
    {
        OpeningAnimation();
    }

    //ON INVOKE
    private void ShowBtns()
    {
        float dist = Vector2.Distance(this.transform.position, playerBehaviour.transform.position);

        if (dist <= 3f)
        {
            interactDoorBtn.SetActive(true);
        }
        else if(dist>3 && dist <= 6)
        {
            interactDoorBtn.SetActive(false);
        }        
    }

    public void OpenDoors() {
        if(Vector2.Distance(this.transform.position, playerBehaviour.transform.position) <= 3 && !isMoving) 
        {
            isMoving = true;
            rotationAcumulation = 0;
            door1.gameObject.GetComponent<Collider2D>().enabled = false;
            door2.gameObject.GetComponent<Collider2D>().enabled = false;
        }
    }

    private void OpeningAnimation()
    {
        if (isMoving)
        {            
            if (isOpen)
            {
                door1.Rotate(0, 0, -5f);
                door2.Rotate(0, 0, -5f);
            }
            else
            {
                door1.Rotate(0, 0, 5f);
                door2.Rotate(0, 0, 5f);
            }
            rotationAcumulation += 5;
            if (rotationAcumulation == 90f)
            {
                isMoving = false;
                door1.gameObject.GetComponent<Collider2D>().enabled = true;
                door2.gameObject.GetComponent<Collider2D>().enabled = true;
                isOpen = !isOpen;
            }            
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.TryGetComponent<KidBehaviour>(out KidBehaviour kid))
        {
            if(!kid.isDead && !kid.saved && !isMoving && !isOpen)
            {
                OpenDoors();
            }
        }
    }
}
