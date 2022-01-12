using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Flashlight : MonoBehaviour
{

    private bool demonIsClose = false;
    private bool devilIsClose = false;

    private GameManager gameManager;
    private DemonBehaviour demonBehaviour;

    private Light2D flashlight;
    private Light2D lightAround;

    [SerializeField] private float lightIntensity = 1f;
    [SerializeField] private float nextIntensity;

    // Start is called before the first frame update
    void Start()
    {
        nextIntensity = 1f;

        demonBehaviour = GameObject.Find("Demon").GetComponent<DemonBehaviour>();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        flashlight = this.GetComponent<Light2D>();
        lightAround = GameObject.Find("LightAround").GetComponent<Light2D>();

        //events
        gameManager.OnPlayerGoingCrazy += StopCheckingIfThereReIllusionsClose;
        gameManager.OnPlayerGoingCrazy += DemonIsGoingAfterThePlayer;

        gameManager.OnPlayerSpotted += SetPlayerWasSpotted;

        demonBehaviour.OnHuntIsOver += StopCheckingIfTheresTheDevilClose;
    }

    // Update is called once per frame
    void Update()
    {
        SetFlashlightIntensity();
    }

    //Começa a checkar se tem demon próximo do player
    private void SetPlayerWasSpotted()
    {
        InvokeRepeating("CheckIfTheresDemonsClose", 0.3f, 0.8f);
    }

    //Cancela a call de invoke do método anterior
    private void StopCheckingIfThereReIllusionsClose()
    {
        CancelInvoke("CheckIfTheresDemonsClose");
    }

    //Começa a checkar se tem o Devil próximo do player
    private void DemonIsGoingAfterThePlayer()
    {
        InvokeRepeating("CheckIfTheDevilIsClose", 0.3f, 0.8f);
    }

    //Cancela a call de invoke do método anterior
    private void StopCheckingIfTheresTheDevilClose()
    {
        CancelInvoke("CheckIfTheDevilIsClose");
    }

    //ON INVOKE -- checkar se o demon está próximo do player e se eles se enxergam 
    private void CheckIfTheresDemonsClose() {
        float radius = 10f;

        RaycastHit2D[] hits = new RaycastHit2D[6];        

        int layerMask = LayerMask.GetMask("Demon");

        Physics2D.CircleCastNonAlloc(this.transform.position, radius, new Vector3(0, 0, 1), hits, Mathf.Infinity, layerMask);

        int demonsHide = 0;
        int totalDemonsClose = 0;

        foreach(RaycastHit2D hit in hits)
        {
            if (hit)
            {
                totalDemonsClose++;
                int wallsBetween = gameManager.RaycastBetweenTwoObjects(this.gameObject, hit.collider.gameObject, "Walls");
                if (wallsBetween == 0)
                {
                    if (!demonBehaviour.devilIsActing) demonIsClose = true;                    
                }
                else
                {
                    demonsHide++;
                }
            }            
        }
        if (demonsHide == totalDemonsClose)
        {
            if(!demonBehaviour.devilIsActing)demonIsClose = false;
        }
    }

    //Checka se o Devil está perto ou não
    private void CheckIfTheDevilIsClose()
    {
        if(Vector2.Distance(this.transform.position, demonBehaviour.transform.position) <= 10f)
        {
            int wallsBetween = gameManager.RaycastBetweenTwoObjects(this.gameObject, demonBehaviour.gameObject, "Walls");
            if (wallsBetween == 0)
            {
                if (!demonBehaviour.devilIsActing) devilIsClose = true;
            }
            else
            {
                if (!demonBehaviour.devilIsActing) devilIsClose = false;
            }
        }
        else
        {
            if (!demonBehaviour.devilIsActing) devilIsClose = false;
        }
    }

    //Seta a intensidade da flashlight. Se ela está falhando ou não
    private void SetFlashlightIntensity()
    {
        flashlight.intensity = lightIntensity;

        if (!demonBehaviour.devilIsActing)
        {
            if (demonBehaviour.demonState == 1)
            {
                if (demonIsClose)
                {
                    FlashlightFailing();

                }
                else
                {
                    lightIntensity = 1f;
                }
            }
            else if (demonBehaviour.demonState == 2)
            {
                if (devilIsClose)
                {
                    FlashlightFailing();
                }
                else
                {
                    lightIntensity = 1f;
                }
            }
            else
            {
                lightIntensity = 1f;
            }
            lightAround.intensity = 1f;
        }
        else
        {
            lightIntensity = 0f;
            lightAround.intensity = 0f;
            //fazer aqui a modificação para os jumpscares
        }             
    }

    //Se a flashlight está falando ou não
    private void FlashlightFailing()
    {
        const float coeficientSpeed = 3f;

        if (Mathf.Abs(nextIntensity - lightIntensity) > 0.03f)
        {
            if (nextIntensity > lightIntensity)
            {
                lightIntensity += Time.deltaTime*coeficientSpeed;
            }
            else
            {
                lightIntensity -= Time.deltaTime * coeficientSpeed;
            }
        }
        else
        {
            nextIntensity = Random.Range(0f, 0.6f);
        }                
    }        
}
