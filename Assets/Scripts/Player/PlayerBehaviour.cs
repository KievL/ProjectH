using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    [SerializeField] private float fear = 0;
    
    public Vector2 currentInteractionPlace;

    private GameManager gameManager;
    private GameObject playerCamera;
    private GameObject closestDoor;
    private GameObject interactDoorBtn;
    private DemonBehaviour demonBehaviour;
    private Salvation salvation;

    private GameObject closestKid;
    private GameObject commandKidsBtn;

    public GameObject currentPlace;

    public GameObject kidBeingHelped;

    private bool playerIsDead =false;

    // Start is called before the first frame update
    void Start()
    {
        closestDoor = null;
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        demonBehaviour = GameObject.Find("Demon").GetComponent<DemonBehaviour>();
        salvation = FindObjectOfType<Salvation>();

        playerCamera = GameObject.FindGameObjectWithTag("MainCamera");
        currentInteractionPlace = playerCamera.transform.position;

        interactDoorBtn = GameObject.FindGameObjectWithTag("DoorBtn");
        interactDoorBtn.SetActive(false);

        closestKid = null;
        commandKidsBtn = GameObject.Find("commandKidsBtn");
        commandKidsBtn.SetActive(false);

        //EVENTS
        gameManager.OnPlayerSpotted += IncreaseFearByTime;
        gameManager.OnPlayerGoingCrazy += StopIncreasingFear;

        demonBehaviour.OnHuntIsOver += FearRelief;
        demonBehaviour.OnKidKilled += MakeCommandButtonDisappear;
        demonBehaviour.OnPlayerKilled += DisableButtons;

        salvation.OnKidSaved += SetKidBeingHelpedNull;

        kidBeingHelped = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && closestDoor != null)
        {
            OpenDoor();
        }

        if (Input.GetKeyDown(KeyCode.Q) && closestKid != null)
        {
            CommandKidFollowOrStay();
        }
        
    }

    //Abrir a porta e aumentar o Fear
    public void OpenDoor()
    {
        if (closestDoor != null)
        {
            StartCoroutine(closestDoor.GetComponent<DoorBehaviour>().interactDoor(true));
            IncreaseFear();
        }        
    }
    
    //Faz a kid começar a seguir ou parar de seguir
    public void CommandKidFollowOrStay()
    {
        if (!kidBeingHelped)
        {
            kidBeingHelped = closestKid;
            kidBeingHelped.GetComponent<KidBehaviour>().ChangeFollowPlayerOrStay();
            
            commandKidsBtn.SetActive(true);
        } else
        {
            kidBeingHelped.GetComponent<KidBehaviour>().ChangeFollowPlayerOrStay();
            kidBeingHelped = null;
        }
    }

    private void MakeCommandButtonDisappear(int id)
    {
        commandKidsBtn.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Seta a Room atual do player
        if (collision.gameObject.CompareTag("Places"))
        {
            currentPlace = collision.gameObject;
        }

        //Faz aparecer o botão se chegar perto da porta
        if (collision.gameObject.CompareTag("DoorCollider") && !playerIsDead)
        {
            closestDoor = GameObject.Find(collision.gameObject.name.Replace("collider", "door"));
            interactDoorBtn.SetActive(true);
        }

        //Faz aparecer o botão se chegar perto da kid
        if (!kidBeingHelped)
        {
            if (collision.gameObject.name == "kid_collider"&& !playerIsDead)
            {
                KidBehaviour kid = collision.GetComponentInParent<KidBehaviour>();
                if (!kid.saved)
                {
                    closestKid = kid.gameObject;
                    commandKidsBtn.SetActive(true);
                }                
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //Faz o botão de abrir a porta desaparecer
        if (collision.gameObject.CompareTag("DoorCollider"))
        {
            closestDoor = null;
            interactDoorBtn.SetActive(false);
        }

        //Faz o botão do kid desaparecer
        if (!kidBeingHelped)
        {
            if (collision.gameObject.name == "kid_collider")
            {
                closestKid = null;
                commandKidsBtn.SetActive(false);
            }
        }        
    }

    //Desabilita os botões após a morte do player
    private void DisableButtons()
    {
        playerIsDead = true;
        commandKidsBtn.SetActive(false);
        interactDoorBtn.SetActive(false);        
    }

    //Aumenta o medo a cada 3sec. É ativado quando o Player é spottado pelo demon
    private void IncreaseFearByTime()
    {
        InvokeRepeating("IncreaseFear", 0.1f, 3f);
        
    }
    
    //Cancela o método de aumentar a fear
    private void StopIncreasingFear()
    {
        CancelInvoke("IncreaseFear");
    }


    public float getFear()
    {
        return fear;
    }

    public void setFear(float newFear)
    {
        fear = fear + newFear;
    }
    
    //Faz aumentar a fear. LIMITE DE 100
    public void IncreaseFear()
    {
        if (fear < 100)
        {
            setFear(2);
        }        
    }  

    //Diminui o Fear toda vez que uma hunt acaba, dependendo do tanto de criança que falta
    private void FearRelief()
    {
        fear = fear * ((gameManager.totalKids + 1) - gameManager.kidsRemaining) * 0.25f;
        if (fear >= 100f)
        {
            fear = 75f;
        }
    }

    private void SetKidBeingHelpedNull(int _id)
    {
        closestKid = null;
        kidBeingHelped = null;
        commandKidsBtn.SetActive(false);
    }

    public void getSetKidBeingHelpedNull()
    {
        this.SetKidBeingHelpedNull(999);
    }
}
