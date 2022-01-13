using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.Rendering.Universal;
using System;

public class IllusionBehaviour : MonoBehaviour
{
    // illusionType = 1 -> illusion created in room
    // illusionType = 2 -> illusion created behind player
    public int illusionType;
    private int id;

    [SerializeField] private bool isIllusionActive = false;

    private PlayerBehaviour playerBehaviour;
    private Crucifix crucifix;

    private bool isOnSightCollider = false;
    [SerializeField] private float timeBeingSeen = 0;
    [SerializeField] private float timeNotBeingSeen = 0;

    [SerializeField] private float timeBeingCrucifixed;
    [SerializeField] private bool isBeingSlowed = false;
    [SerializeField] private bool isNotBeingSlowed = true;

    public event Action<int> OnIllusionSeen;
    public event Action OnIllusionNotSeen;

    [SerializeField] private bool goingAfterPlayer = false;

    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {     
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

        playerBehaviour = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerBehaviour>();
        crucifix = GameObject.FindGameObjectWithTag("Player").GetComponent<Crucifix>();

        //Se for do tipo 1, ele já começa visível mas inativo
        if (illusionType == 2)
        {
            EnableOrDisableIllusion(false);
        }
        else if (illusionType == 1)
        {
            isIllusionActive = false;
        }

        OnIllusionNotSeen += ProcessToDestroyIllusion;
        OnIllusionSeen += WalkToPlayer;
        OnIllusionSeen += IncreasePlayerFear;
        OnIllusionSeen += DestroyAllOthersIllusons;
    }

    // Update is called once per frame
    void Update()
    {
        LookToPlayer();
        if (isIllusionActive)
        {
            IsIllusionBeingSeen();
        }

        //por enquanto...
        StartSelfSpontaneousCombustion();

        DieByCrucifix();
    }    

    //Faz a illusion olhar sempre para o player
    void LookToPlayer()
    {
        if (isIllusionActive && goingAfterPlayer==false)
        {
            Vector3 playerPos = new Vector3(playerBehaviour.transform.position.x, playerBehaviour.transform.position.y, -10);
            Vector3 relativePos = playerPos - this.transform.position;

            var angle = (Mathf.Atan2(relativePos.y, relativePos.x) * Mathf.Rad2Deg) - 90f;
            this.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isIllusionActive && collision.gameObject.CompareTag("Light"))
        {
            if (illusionType == 2)
            {
                EnableOrDisableIllusion(true);
            }
            else if (illusionType == 1)
            {
                isIllusionActive = true;
            } 
            
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isIllusionActive)
        {
            if (collision.gameObject.CompareTag("PlayerSight"))
            {
                isOnSightCollider = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isIllusionActive)
        {
            if (collision.gameObject.CompareTag("PlayerSight"))
            {
                isOnSightCollider = false;
            }
        }
    }

    //Testa se a illusion ta sendo vista ou não pelo player 
    private void IsIllusionBeingSeen()
    {        
        int hit = gameManager.RaycastBetweenTwoObjects(playerBehaviour.gameObject, this.gameObject, "Walls");
        
        if (isOnSightCollider && hit==0)
        {
            OnIllusionSeen(this.GetInstanceID());
        }
        else
        {
            OnIllusionNotSeen();
        }
        
    }

    //Processo de destruição da illusion por muito tempo desativada
    private void ProcessToDestroyIllusion()
    {
        timeNotBeingSeen += Time.deltaTime;
                
        if(timeNotBeingSeen >= 2f)
        {
            //Destroy(this.gameObject);
        }       
    }

    //Faz a illusion andar até o player
    private void WalkToPlayer(int id)
    {
        if (id == this.GetInstanceID())
        {
            timeBeingSeen += Time.deltaTime;
            if (timeBeingSeen > 0.1f)
            {
                goingAfterPlayer = true;
                this.GetComponent<NavMeshAgent>().SetDestination(playerBehaviour.transform.position);
            }
        }                
    }

    private void DieByCrucifix()
    {
        if (crucifix.isUsing && isOnSightCollider)
        {
            timeBeingCrucifixed += Time.deltaTime;

            if (!isBeingSlowed)
            {
                this.GetComponent<NavMeshAgent>().speed *= 0.1f;
                isBeingSlowed = true;
                isNotBeingSlowed = false;
            }

            if (timeBeingCrucifixed >= 2.5f)
            {
                Destroy(this.gameObject);
            }
        } else if (!crucifix.isUsing || !isOnSightCollider)
        {
            if (!isNotBeingSlowed)
            {
                this.GetComponent<NavMeshAgent>().speed *= 10f;
                isNotBeingSlowed = true;
            }

            timeBeingCrucifixed = 0f;
            isBeingSlowed = false;
        }
    }

    //Aumenta a fear do player se estiverem se olhando

    private void IncreasePlayerFear(int id)
    {       
        if(id == this.GetInstanceID())
        {
            playerBehaviour.setFear(0.05f);
        }        
    }

    private void DestroyAllOthersIllusons(int idToDontKill)
    {
        if (idToDontKill != this.GetInstanceID() && illusionType==2)
        {
            Destroy(this.gameManager);
        }
    }

    //Ativa ou desativa a illusion
    private void EnableOrDisableIllusion(bool trueOrFalse)
    {
        isIllusionActive = trueOrFalse;
        this.transform.GetComponentInChildren<ShadowCaster2D>().castsShadows = trueOrFalse;
        for (int i = 0; i < this.transform.childCount; i++)
        {
            this.transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = trueOrFalse;
        }
    }

    //REFATORAR -- mata a ilusão ao iniciar a hunt
    private void StartSelfSpontaneousCombustion()
    {
        if (gameManager.killEveryIllusion)
        {
            Destroy(this.gameObject);
        }                
    }
}
