using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public Vector2 lastPlayerInteraction;

    PlayerBehaviour playerBehaviour;

    private DemonBehaviour demonBehaviour;
    private DemonAura demonAura;
    private SpawnNPCManager spawnManager;

    GameObject[] doors;

    public GameObject Walls;

    public event Action OnPlayerSpotted;

    public event Action OnPlayerGoingCrazy;

    public bool killEveryIllusion = false;

    public int kidsRemaining;
    public int totalKids;

    // Start is called before the first frame update
    void Start()
    {
        demonAura = GameObject.Find("DemonAura").GetComponent<DemonAura>();
        demonAura.OnPlayerIsInAura += CheckIfDemonSeesPlayer;

        playerBehaviour = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerBehaviour>();
        demonBehaviour = GameObject.Find("Demon").GetComponent<DemonBehaviour>();
        spawnManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<SpawnNPCManager>();

        //Pegando o total de kids que iniciaram na partida
        kidsRemaining = spawnManager.kidsQuantity;
        totalKids = kidsRemaining;

        //Adicionando o método de interação a cada porta da lista
        doors = GameObject.FindGameObjectsWithTag("Door");
        foreach(GameObject doorObj in doors)
        {            
            doorObj.GetComponent<DoorBehaviour>().OnDoorTouched += newInteraction;            
        }

        lastPlayerInteraction = playerBehaviour.currentInteractionPlace;
    }
    
    void Update()
    {
        CanDemonCanHuntPlayer();
    }
    
    //Quando ocorre uma nova interação no mapa. Ativado por evento
    private void newInteraction()
    {
        if (demonBehaviour.freeToGo==true && demonBehaviour.demonState==0)
        {
            lastPlayerInteraction = playerBehaviour.currentInteractionPlace;
            StartCoroutine(demonBehaviour.OnHeardInteraction());
        }
    }

    //Checka se o demon enxerga o player 
    private void CheckIfDemonSeesPlayer()
    { 
        int hit = RaycastBetweenTwoObjects(playerBehaviour.gameObject, demonBehaviour.gameObject, "Walls");
                
        if (hit==0 && demonBehaviour.demonState==0)
        {
            demonBehaviour.demonState = 1;
            OnPlayerSpotted();            

        }              
    }

    //cria um raycast entre dois objetos em uma layer
    public int RaycastBetweenTwoObjects(GameObject obj1, GameObject obj2, string layer)
    {
        Vector2 obj1Spot = obj1.gameObject.transform.position;
        Vector2 obj2Spot = obj2.gameObject.transform.position;

        int layerMask = LayerMask.GetMask(layer);

        RaycastHit2D[] hitList= new RaycastHit2D[5];

        return Physics2D.LinecastNonAlloc(obj1Spot, obj2Spot, hitList,layerMask);
    }

    //Cria contact filters com triggers e layers
    public ContactFilter2D createContactFilter(string layerName, bool useTriggers)
    {
        ContactFilter2D cf = new ContactFilter2D();
        cf.useTriggers = useTriggers;
        int layerMask = LayerMask.GetMask(layerName);
        cf.SetLayerMask(layerMask);
        cf.useLayerMask = true;

        return cf;
    }

    //Checka se uma hunt pode ser iniciada e realiza esse inicio
    private void CanDemonCanHuntPlayer()
    {
        if(playerBehaviour.getFear()>=100 && demonBehaviour.demonState == 1)
        {
            demonBehaviour.demonState = 2;

            //Matar todas as illusions no mapa
            killEveryIllusion = true;

            //Evento que ativa tudo que ocorre quando uma hunt é iniciada
            OnPlayerGoingCrazy();

        }        
    }    
}
