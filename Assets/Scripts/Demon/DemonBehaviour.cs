using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.Rendering.Universal;
using System;

public class DemonBehaviour : MonoBehaviour
{ 
    NavMeshAgent demonMesh;

    public Vector2 newSpotToFind;

    public bool devilIsActing=false;

    public int demonState = 0;    
    //demonState =0:finding Player, demonState=1:player Found, demonState=2: hunt mode

    private float crono = 0;

    public bool freeToGo = false;

    private RandomWalk rndWalk;
    private GameManager gameManager;
    private PlayerBehaviour playerBehaviour;
    private DemonBody demonBody;

    public event Action OnHuntIsOver;
    public event Action<int> OnKidKilled;
    public event Action OnPlayerKilled;

    private float huntDuration = 0;
    [SerializeField] private float huntMaxDuration;

    [SerializeField] private GameObject jumpScare1;

    private Crucifix crucifix;
    private bool isOnSightCollider = false;
    [SerializeField] private bool isBeingSlowed = false;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        playerBehaviour = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerBehaviour>();
        demonBody = this.GetComponentInChildren<DemonBody>();
        rndWalk = this.GetComponent<RandomWalk>();
        demonMesh = GetComponent<NavMeshAgent>();
        crucifix = GameObject.FindGameObjectWithTag("Player").GetComponent<Crucifix>();

        StartCoroutine(SetVisible(false));

        //events
        demonBody.OnPlayerGetCaught += KillHumans;
        gameManager.OnPlayerGoingCrazy += StartHuntSomewhereNextToThePlayer;
    }    

    // Update is called once per frame
    void Update()
    {
        DemonAction();
        BeCrucifixed();

        //Libera o monstro pra caçar depois de 4 segundos
        crono += Time.deltaTime;
        if (crono >= 4f)
        {
            freeToGo = true;

            if (!demonMesh.isActiveAndEnabled)
            {
                this.GetComponent<NavMeshAgent>().enabled = true;
            }
        }

        OnHeardInteraction();
    }        

    //As ações do monstro com base no estado dele
    public void DemonAction()
    {
        if (freeToGo)
        {
            if (demonState == 0)
            {
                //find player
                if (newSpotToFind != null && demonMesh.isActiveAndEnabled)
                {                    
                    demonMesh.SetDestination(newSpotToFind);
                }

                if (IsInvoking("ChooseATargetToKill"))
                {
                    CancelInvoke("ChooseATargetToKill");
                }
            }
            else if (demonState == 1)
            {
                demonMesh.SetDestination(GameObject.FindGameObjectWithTag("Player").transform.position);
                //haunt player

            }
            else
            {

                huntDuration += Time.deltaTime;
                InvokeRepeating("ChooseATargetToKill", 0f, 0.4f);

                demonMesh.SetDestination(newSpotToFind);

                //Acaba a hunt
                if (huntDuration >= huntMaxDuration)
                {
                    BackToStateOne();
                }
                //hunt player
            }
        }        
    }    

    //Pega o atual Vector Target da classe RandomWalk
    public void SetNewSpotToFindPlayer(Vector2 newSpot)
    {        
        if (newSpot != null)
        {
            newSpotToFind = newSpot;
        }
    }        

    //Seta um novo lugar para procurar o player baseado numa interação do player no mapa
    public IEnumerator OnHeardInteraction()
    {
        rndWalk.SetNewVectorTargetByInteraction(gameManager.lastPlayerInteraction);
        yield return null;
        
    }

    //Seta o demon invisível e visível no mapa
    private IEnumerator SetVisible(bool isVisible)
    {
        this.transform.GetComponentInChildren<ShadowCaster2D>().castsShadows = isVisible;
        for (int i = 0; i < this.transform.childCount; i++)
        {
            this.transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = isVisible;
        }
        yield return null;
    }

    //ON INVOKE - Escolhe o target mais próximo do demon. A criança só é atacada se tiver sendo ajudada pelo player
    //NOVA IDEIA
    private void ChooseATargetToKill()
    {
        Vector2 closestTarget = playerBehaviour.transform.position;

        GameObject kidBeingHelped = playerBehaviour.kidBeingHelped;

        if (kidBeingHelped != null)
        {
            Vector2 kidPos = kidBeingHelped.transform.position;
            float kidDistanceFromDemon = Vector2.Distance(kidPos, this.transform.position);
            float playerDistanceFromDemon = Vector2.Distance(closestTarget, this.transform.position);

            if (kidDistanceFromDemon < playerDistanceFromDemon)
            {                
                closestTarget = kidPos;
            }
        }
        newSpotToFind = closestTarget;                
    }
    
    //Teleporta para um local próximo (mas nem tanto) ao player para iniciar a hunt.
    private void StartHuntSomewhereNextToThePlayer()
    {
        bool validPlace = false;
        Vector2 huntingStartingSpot = new Vector2(-1000, -1000);
        while (!validPlace)
        {
            float xPlayer = playerBehaviour.transform.position.x;
            float yPlayer = playerBehaviour.transform.position.y;

            float xSpot = UnityEngine.Random.Range(xPlayer - 28, xPlayer + 28);
            float ySpot = UnityEngine.Random.Range(yPlayer - 28, yPlayer + 28);

            if(xSpot>= xPlayer-11 && xSpot<= xPlayer+11 && ySpot>=yPlayer-11 && ySpot <= yPlayer + 11)
            {
                break;
            }
            else
            {
                Vector2 originTest = new Vector2(xSpot, ySpot);
  
                RaycastHit2D[] hits = new RaycastHit2D[2];

                int validPlacesToSpawn = Physics2D.CircleCast(originTest, 1f, new Vector3(0, 0, 1),
                    gameManager.createContactFilter("Place", true), hits, Mathf.Infinity);

                if (validPlacesToSpawn > 0)
                {
                    huntingStartingSpot = originTest;
                    validPlace = true;
                }
                else
                {
                    break;
                }
            }
        }
        StartCoroutine(SetVisible(true));
        Teleport(huntingStartingSpot);
    }

    //Teleporta o demon para o local desejado; 
    public void Teleport(Vector2 place)
    {
        if (!devilIsActing)
        {
            this.transform.position = new Vector3(place.x, place.y, this.transform.position.z);
        }
    }

    //ON INVOKE -- Deixa o demon invisível - Spawna ele longe
    private void HuntIsOver()
    {
        //BURN THE DEMON
        StartCoroutine(SetVisible(false));

        GameObject[] respawnSpots;
        respawnSpots = GameObject.FindGameObjectsWithTag("Respawn");

        GameObject newSpotToSpawn = respawnSpots[UnityEngine.Random.Range(0, respawnSpots.Length)];

        while(Vector2.Distance(newSpotToSpawn.transform.position, playerBehaviour.transform.position) <= 40f)
        {
            newSpotToSpawn = respawnSpots[UnityEngine.Random.Range(0, respawnSpots.Length)];
        }

        this.transform.position = newSpotToSpawn.transform.position; 
    }

    private void KillHumans()
    {
        devilIsActing = true;
        if (playerBehaviour.kidBeingHelped == null)
        {
            KillPlayer();
        }
        else
        {
            KillKid(playerBehaviour.kidBeingHelped);
        }

    }

    private void KillPlayer()
    {
        OnPlayerKilled();
        jumpScare1.SetActive(true);
        
    }

    private void KillKid(GameObject kid)
    {
        OnKidKilled(kid.GetComponent<KidBehaviour>().id);
        Invoke("BackToStateOne", 2f);
        playerBehaviour.kidBeingHelped = null;

    }

    //ON INVOKE - Retorna o demon para o state 0 para procurar o jogador 
    private void BackToStateOne()
    {
        HuntIsOver();
        huntDuration = 0;
        OnHuntIsOver();
        demonState = 0;
        devilIsActing = false;
        

    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerSight"))
        {
            isOnSightCollider = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerSight"))
        {
            isOnSightCollider = false;
        }
    }

    private void BeCrucifixed()
    {
        if (crucifix.isUsing && isOnSightCollider)
        {
            if (!isBeingSlowed)
            {
                this.GetComponent<NavMeshAgent>().speed *= 0.1f;
                isBeingSlowed = true;
            }
        }
        else if (!crucifix.isUsing || !isOnSightCollider)
        {
            if (isBeingSlowed)
            {
                this.GetComponent<NavMeshAgent>().speed *= 10f;
            }

            isBeingSlowed = false;
        }
    }
}
