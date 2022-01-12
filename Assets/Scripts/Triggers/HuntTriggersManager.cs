using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HuntTriggersManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> triggersGO;
    private List<TriggerCol> triggerCollider;

    private GameManager gameManager;
    private Collider2D playerCollider;
    private DemonBehaviour demonBehaviour;

    [SerializeField] private Vector2 teleportPlace;

    private float timeToPullAnotherTrigger = 0f;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        demonBehaviour = GameObject.Find("Demon").GetComponent<DemonBehaviour>();
        playerCollider = GameObject.Find("ombro").GetComponent<Collider2D>();

        gameManager.OnPlayerGoingCrazy += LetTheTriggersStartToWork;

        triggerCollider = new List<TriggerCol>();
        CreateTriggers();
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    private void LetTheTriggersStartToWork()
    {
        InvokeRepeating("CheckCollisions", 0.1f, 0.4f);

    }

    //ON INVOKE
    private void CheckCollisions()
    {
        if (demonBehaviour.demonState == 2)
        {
            bool trigger0Actv = triggerCollider[0].GetTriggerEnable();
            bool trigger1Actv = triggerCollider[1].GetTriggerEnable();

            if (!trigger0Actv)
            {
                if (triggerCollider[0].GetTriggerCollider().IsTouching(playerCollider))
                {
                    triggerCollider[0].SetTriggerEnable(true);
                }
            }
            else
            {                
                if (trigger1Actv)
                {
                    demonBehaviour.Teleport(teleportPlace);
                    triggerCollider[0].SetTriggerEnable(false);
                    triggerCollider[1].SetTriggerEnable(false);
                }
                else
                {
                    if (triggerCollider[1].GetTriggerCollider().IsTouching(playerCollider))
                    {
                        triggerCollider[1].SetTriggerEnable(true);
                    }
                }

                timeToPullAnotherTrigger += Time.deltaTime;
                if (timeToPullAnotherTrigger >= 4.5f)
                {
                    triggerCollider[0].SetTriggerEnable(false);
                    timeToPullAnotherTrigger = 0f;
                }
            }
        }                    
    }

    private void CreateTriggers()
    {        
        foreach (GameObject trigger in triggersGO)
        {
            TriggerCol triggerComponent = new TriggerCol(trigger.GetComponent<Collider2D>(), false);
            triggerCollider.Add(triggerComponent);
            
        }
    }
}
