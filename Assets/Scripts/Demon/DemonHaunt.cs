using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DemonHaunt : MonoBehaviour
{
    public GameObject demonClonePrefab;
    public GameObject cloneSpotCheckerPrefab;

    private GameManager gmManager;
    private PlayerBehaviour playerBehaviour;
    private DemonBehaviour demonBehaviour;

    float crono = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        gmManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        gmManager.OnPlayerSpotted += PreconfigurateHaunt;
        gmManager.OnPlayerGoingCrazy += StartHauntInHuntMode;
        gmManager.OnPlayerGoingCrazy += StopKillingTheIllusions;

        demonBehaviour = this.GetComponent<DemonBehaviour>();
        playerBehaviour = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerBehaviour>();


    }

    // Update is called once per frame
    void Update()
    {
        crono += Time.deltaTime;
    }

    //This method runs every time Demon Spots player
    private void PreconfigurateHaunt()
    {
        //Spawn demons in rooms (or not)
        StartCoroutine(SpawnDemonClonesInRooms());

        //Keep invoking the method to spawn illusions behind the player every 2 secs        
        InvokeRepeating("TryToSpawnCloneBehindPlayer", 2f, 4f);
        
    }

    //Method that spawns the demons in rooms spots
    private IEnumerator SpawnDemonClonesInRooms()
    {
        while (demonBehaviour.demonState == 1)
        {
            //list of possible rooms to spawn illusions
            GameObject[] rooms = GameObject.FindGameObjectsWithTag("Places");

            //for each room found, try to spawn the illusion 
            foreach (GameObject gmObj in rooms)
            {
                float luckyNumber = UnityEngine.Random.Range(0, 100);

                if (gmObj != GameObject.Find("Park") && gmObj != GameObject.Find("Hall"))
                {
                    ContactFilter2D cf = gmManager.createContactFilter("Demon", true);
                    ContactFilter2D cf2 = gmManager.createContactFilter("Player", false);

                    if (!gmObj.GetComponent<Collider2D>().IsTouching(cf) && !gmObj.GetComponent<Collider2D>().IsTouching(cf2))
                    {
                        //Menor que 60: cria o clone na sala
                        if (luckyNumber <= 60)
                        {
                            int spotsQuantity = gmObj.transform.childCount;

                            //testa se a room tem algum spot de spawn de illusion
                            if (spotsQuantity > 0)
                            {
                                // escolhe um spot na sala para spawnar a illusion
                                GameObject spotGameObject = gmObj.transform.Find(
                                    "CloneSpawn" + UnityEngine.Random.Range(1, spotsQuantity).ToString()).gameObject;

                                IllusionBehaviour newIllusion;
                                newIllusion = Instantiate(demonClonePrefab, spotGameObject.transform.position, Quaternion.identity)
                                    .gameObject.GetComponent<IllusionBehaviour>();

                                //Seta que o tipo da illusion é 1 (illusion de salas)
                                newIllusion.illusionType = 1;
                            }
                        }
                    }
                }
            }
            yield return new WaitForSeconds(30f);
        }               
    }

    //Tenta spawnar uma illusion atrás do player (ESTÁ NO INVOKER REPEATING NO START)
    private void TryToSpawnCloneBehindPlayer()
    {
        const float timeCoeficient= 0.7f;
        const float fearCoeficient = 0.5f;
        //LuckyLimit é um número dado com base no tempo e no medo do jogador
        float luckyLimit = (timeCoeficient * crono) + (fearCoeficient * playerBehaviour.getFear());
        float luckyPick = UnityEngine.Random.Range(0, 100);

        //A chance de criar a ilusão atrás do player
        if (luckyPick<= luckyLimit)
        {            
            Vector2 spawnSpot = GetCloneSpawnSpotBehindPlayer();
            GameObject spotChecker =Instantiate(cloneSpotCheckerPrefab, spawnSpot, Quaternion.identity);            

            Collider2D[] colList = new Collider2D[12];

            int overlapCount = countsOfOverlaps("Walls", false, colList, spotChecker);

            //Se não está tocando nas paredes
            if (overlapCount == 0)
            {
                Collider2D[] colList2 = new Collider2D[1];

                int overlapCount2 = countsOfOverlaps("Place", true, colList2, spotChecker);
                //Se está tocando em algum lugar válido
                if (overlapCount2 > 0)
                {
                    //Se está na mesma sala do player
                    if (playerBehaviour.currentPlace == colList2[0].gameObject)
                    {
                        //Check if theres another illusion near

                        StartCoroutine(TryToSpawnCloneAwayFromOthers(spotChecker));                                        
                    }
                }                               
            }
            Destroy(spotChecker);
        }      
    }

    //retorna o número de overlaps do spotChecker e coloca na lista colList os gameobjects que estão colidindo na layer 
    private int countsOfOverlaps(string layer, bool areTriggersUsed, Collider2D[] colList, GameObject spotGmObj)
    {
        ContactFilter2D cf = gmManager.createContactFilter(layer, areTriggersUsed);        

        return Physics2D.OverlapCollider(spotGmObj.GetComponent<Collider2D>(), cf, colList);
    }   

    //Retorna um vector2 do local onde será spawnada a illusion
    private Vector2 GetCloneSpawnSpotBehindPlayer()
    {
        const float desiredDistanceFromPlayer = 4;
        float angleRotation = playerBehaviour.transform.rotation.eulerAngles.z;

        float tgAngle = Mathf.Tan((angleRotation*Mathf.PI)/180) ;       

        float xSpawn = Mathf.Sqrt((Mathf.Pow(desiredDistanceFromPlayer, 2)) / (Mathf.Pow(tgAngle, 2) + 1));
        if((angleRotation>=0 && angleRotation<90)|| (angleRotation>270 && angleRotation < 360))
        {
            xSpawn = -xSpawn;
        }
        float ySpawn = Mathf.Abs(xSpawn * tgAngle);
        if((angleRotation >= 0 && angleRotation < 180))
        {
            ySpawn = -ySpawn;
        }
        Vector2 playerPos = new Vector2(playerBehaviour.transform.position.x, playerBehaviour.transform.position.y);

        return new Vector2(playerPos.x+xSpawn, playerPos.y+ySpawn);
    }    

    //Checka se tem clones perto do outro para evitar colisões
    private IEnumerator TryToSpawnCloneAwayFromOthers(GameObject spawnChecker)
    {
        bool isThereAnotherIllusionClose = false;

        ContactFilter2D cf = gmManager.createContactFilter("Demon", true);

        RaycastHit2D[] col = new RaycastHit2D[3];

        int collides = Physics2D.CircleCast(spawnChecker.transform.position, 10f, new Vector3(0, 0, 1), cf, col);

        if (collides > 0)
        {
            foreach (RaycastHit2D hit in col)
            {
                if (hit)
                {
                    int hits = gmManager.RaycastBetweenTwoObjects(spawnChecker, hit.collider.gameObject, "Walls");
                    if (hits == 0)
                    {
                        isThereAnotherIllusionClose = true;
                    }
                }                
            }
            if (!isThereAnotherIllusionClose)
            {
                SpawnClone();
            }
        }
        else
        {
            SpawnClone();
        }

        yield return null;

    }   

    //Instancia um clone
    private void SpawnClone()
    {
        IllusionBehaviour newIllusion;
        newIllusion = Instantiate(demonClonePrefab, GetCloneSpawnSpotBehindPlayer(), Quaternion.identity)
        .gameObject.GetComponent<IllusionBehaviour>();

        //Tipo de ilusão 2 que aparece atrás do player
        newIllusion.illusionType = 2;
        crono = 0;
    }    

    //Método para parar de matar as illusions
    private void StopKillingTheIllusions()
    {
        Invoke("SetKillIllusionsBoolFalse", 0.5f);
    }

    //ON INVOKE ABOVE
    private void SetKillIllusionsBoolFalse()
    {
        gmManager.killEveryIllusion = false;
    }
    
    //Para os invokes da Haunt quando inicia a Hunt
    private void StartHauntInHuntMode()
    {
        if (IsInvoking())
        {
            crono = 0;
            CancelInvoke();
        }
    }
    //Tipos de HAUNT:
    //CLONES POR TODO O MAPA
    //CLONES ATRÁS DO PLAYER AO ENTRAR NUMA SALA
    //PORTAS ABRINDO E FECHANDO
    //INTERAÇÃO NOS OBJETOS DO MAPA
    //LUZ PISCANDO
}
