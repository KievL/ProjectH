using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RandomWalk : MonoBehaviour
{
    public float crono=0;
    float cronoToStart = 0;

    public bool firstPlace = true;

    NavMeshAgent demonMesh;
    private DemonAura dAura;
    private DemonBehaviour demonBehaviour;
    private DemonBody demonBody;

    public Vector2 currentVectorTarget;
    
    List<GameObject> placesOnSight;
    public GameObject currentPlaceToGo;

    public bool checkingInteraction = false;

    GameObject soundLarge, soundMedium, soundShort;

    // Start is called before the first frame update
    void Start()
    {
        demonMesh = GetComponent<NavMeshAgent>();

        demonBehaviour = this.GetComponent<DemonBehaviour>();

        demonBody = this.GetComponentInChildren<DemonBody>();
        demonBody.OnGetToDestination += ArrivedInRandomDestination;

        dAura = GameObject.Find("DemonAura").GetComponent<DemonAura>();
        dAura.OnPlacesSeenChanges += UpdatePlacesOnSight;

        soundLarge = GameObject.Find("LargeSound");
        soundMedium = GameObject.Find("MediumSound");
        soundShort = GameObject.Find("ShortSound");

        demonBehaviour.OnHuntIsOver += ResetDefaultStateAfterHunt;
        
    }

    private void Update()
    {
        WalkToARandomPlace();        
    }

    //A cada 18 segundos, manda uma mensagem para o DemonBehaviour dizendo para onde ir
    private void WalkToARandomPlace()
    {
        if (demonBehaviour.demonState == 0)
        {
            cronoToStart += Time.deltaTime;

            if (!checkingInteraction)
            {
                crono += Time.deltaTime;
                if ((crono >= 8f || demonMesh.hasPath == false || firstPlace == true) && cronoToStart >= 3.2f)
                {
                    //O firstPlace é só para conseguir entrar nesse if quando iniciar a partida            
                    firstPlace = false;
                    StartCoroutine(MakeDecision());
                    crono = 0;
                }
            }
        }                          
    }    

    //Escolhe um lugar aleatório para o monstro ir quando ele não tem rumo.
    private IEnumerator MakeDecision()
    {
        int placesArrayLenght = 0;
        Collider2D col;

        //Adiciona os GameObjects dos lugares numa lista
        foreach (GameObject place in placesOnSight)
        {
            placesArrayLenght++;
        }

        //Sorteia um lugar para ir da lista
        int randomPlace = Random.Range(0, placesArrayLenght - 1);
        currentPlaceToGo = placesOnSight[randomPlace];

        //Sorteia um ponto do collider do GameObject selecionado para o monstro ir
        col = placesOnSight[randomPlace].GetComponent<Collider2D>();
        currentVectorTarget = GetRandomPointInsideCollider(col, false);
        demonBehaviour.SetNewSpotToFindPlayer(currentVectorTarget);

        yield return null;
    }

    //Da um update na lista de lugares em contato com o collider do monstro
     private void UpdatePlacesOnSight()
    {
        placesOnSight = dAura.placesOnSight;        
        
    }

    //Pega um lugar aleatório na região de um collider
    private Vector2 GetRandomPointInsideCollider(Collider2D collider, bool interactionPoint)
    {
        Vector2 point = GetVectorInCollidersRandomBoundsRange(collider);
                       
        if (interactionPoint) {
            //Fica pegando um ponto aleatório até que ele esteja dentro do mapa e em contato com o collider
            while (!PointOverlapsMapBounds(point) || !collider.OverlapPoint(point))
            {
                point = GetVectorInCollidersRandomBoundsRange(collider);
            }
            return point;
        } 
        else
        {
            //Testa se o ponto selecionado está dentro do collider 
            while (!collider.OverlapPoint(point))
            {
                point = GetVectorInCollidersRandomBoundsRange(collider);
            }
            return point;
        }                                
    }

    Vector2 GetVectorInCollidersRandomBoundsRange(Collider2D collider)
    {
        //Pega um ponto num range de acordo com as bounds do collider
        return new Vector2(
                Random.Range(collider.bounds.min.x, collider.bounds.max.x),
                Random.Range(collider.bounds.min.y, collider.bounds.max.y)
                );
    }

    //Testa se um atributo ponto está dentro dos colliders do mapa e não toca o Park nem o Hall.
    private bool PointOverlapsMapBounds(Vector2 point) {
        GameObject placesColliders = GameObject.Find("PlacesColliders");
        int correctPlaces = 0;
        foreach (Transform child in placesColliders.GetComponent<Transform>())
        {
            Collider2D childCollider = child.gameObject.GetComponent<Collider2D>();
            if (childCollider.OverlapPoint(point))
            {
                if(child.gameObject.name != "Park" && child.gameObject.name != "Hall")
                {
                    correctPlaces++;
                } 
                
            } 
            
        }        

        if (correctPlaces > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //Pega um novo Vector aleatório a cada interação do jogador com algo
    public void SetNewVectorTargetByInteraction(Vector2 interactionPlace)
    {
        //Distância do monstro pra interação
        float distanceFromInteraction = Mathf.Sqrt(Mathf.Pow(interactionPlace.x - this.transform.position.x, 2) +
            Mathf.Pow(interactionPlace.y - this.transform.position.y, 2));

        checkingInteraction = true;
        //Dependendo da distância o monstro vai para lugares diferentes
        if (distanceFromInteraction >= 38f)
        {                                              
            currentVectorTarget = GetRandomPointInsideCollider(soundLarge.GetComponent<Collider2D>(), true);                                        
        }
        else if (distanceFromInteraction <= 19.2f)
        {
            currentVectorTarget = GetRandomPointInsideCollider(soundShort.GetComponent<Collider2D>(), true);
        }
        else
        {
            currentVectorTarget = GetRandomPointInsideCollider(soundMedium.GetComponent<Collider2D>(), true);
        }
        GameObject.Find("DestinationChecker").transform.position = currentVectorTarget;
        demonBehaviour.SetNewSpotToFindPlayer(currentVectorTarget);
    }        
   
    //Quando o monstro chega no destino aleatório
    private void ArrivedInRandomDestination()
    {
        checkingInteraction = false;
        crono = 18f;
    }

    private void ResetDefaultStateAfterHunt()
    {
        firstPlace = true;
        checkingInteraction = false;
    }
}
