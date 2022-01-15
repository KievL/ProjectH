using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.AI;

public class KidBehaviour : MonoBehaviour
{
    private PlayerBehaviour playerBehaviour;
    private DemonBehaviour demonBehaviour;
    private GameManager gameManager;
    private Salvation salvation;

    NavMeshAgent kidMesh;
    bool isFollowingPlayer = false;

    public int id;

    public bool isDead = false;

    public bool saved = false;

    // Start is called before the first frame update
    void Start()
    {
        playerBehaviour = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerBehaviour>();
        demonBehaviour = GameObject.Find("Demon").GetComponent<DemonBehaviour>();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        salvation = FindObjectOfType<Salvation>();

        kidMesh = this.GetComponent<NavMeshAgent>();
        kidMesh.stoppingDistance = 3f;

        //events
        demonBehaviour.OnKidKilled += Die;
        salvation.OnKidSaved += SaveKid;

    }

    // Update is called once per frame
    void Update()
    {
        if (!isFollowingPlayer && !isDead)
        {
            LookToPlayer();
        }

        if (isFollowingPlayer)
        {
            float distance = Vector2.Distance(this.transform.position, playerBehaviour.gameObject.transform.position);

            if (distance >= 18f)
            {
                ChangeFollowPlayerOrStay();
                playerBehaviour.getSetKidBeingHelpedNull();
            }
        }
    }

    //ON INVOKE - Bateu na porta e abriu
    private void KidBodyCollisionWithTheDoors()
    {
        RaycastHit2D[] hits = new RaycastHit2D[3];
        int cols = Physics2D.CircleCastNonAlloc(this.transform.position, 1f, new Vector3(0, 0, 1), hits, Mathf.Infinity,
            LayerMask.GetMask("Walls"));
        if (cols > 0)
        {
            foreach(RaycastHit2D hit in hits)
            {
                if (hit)
                {
                    if (hit.collider.gameObject.TryGetComponent(out DoorBehaviour door))
                    {
                        StartCoroutine(door.interactDoor(false));
                        break;
                    }
                }                
            }
        }
    }

    private void LookToPlayer()
    {
        Vector3 playerPos = new Vector3(playerBehaviour.transform.position.x, playerBehaviour.transform.position.y, -10);
        Vector3 relativePos = playerPos - this.transform.position;

        var angle = Mathf.Atan2(relativePos.y, relativePos.x) * Mathf.Rad2Deg;
        this.transform.rotation = Quaternion.AngleAxis(angle-90, Vector3.forward);
    }

    public void ChangeFollowPlayerOrStay()
    {
        if (!isDead && !saved)
        {
            isFollowingPlayer = !isFollowingPlayer;

            if (isFollowingPlayer)
            {
                StartCoroutine(setKidDestination());
                InvokeRepeating("KidBodyCollisionWithTheDoors", 0, 5f);
            } else
            {
                CancelInvoke("KidBodyCollisionWithTheDoors");
            }
                
        }
    }

    private IEnumerator setKidDestination()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            if(isFollowingPlayer)
            {
                int collisions = gameManager.RaycastBetweenTwoObjects(this.gameObject, playerBehaviour.gameObject, "Walls");
                
                if (collisions == 0)
                {
                    kidMesh.SetDestination(playerBehaviour.transform.position);
                }
                else
                {
                    float distance = Vector2.Distance(this.transform.position, playerBehaviour.transform.position);
                    if(distance <= 7f)
                    {
                        if(!playerBehaviour.GetComponent<Collider2D>().IsTouching(gameManager.createContactFilter("DoorColliders", true))){
                            kidMesh.SetDestination(playerBehaviour.transform.position);                            
                        }
                        else
                        {
                            kidMesh.SetDestination(this.transform.position);
                            
                        }
                    }
                    else
                    {
                        kidMesh.SetDestination(playerBehaviour.transform.position);
                    }
                }
            }
            else
            {
                kidMesh.SetDestination(this.transform.position);
                break;
            }
        }
    }

    private void Die(int _id)
    {
        if (_id == id)
        {
            this.GetComponent<ShadowCaster2D>().enabled = false;
            this.GetComponent<NavMeshAgent>().enabled = false;
            this.GetComponent<Collider2D>().enabled = false;

            for(int i = 0; i<this.transform.childCount; i++)
            {
                GameObject child = this.transform.GetChild(i).gameObject;
                if(child.TryGetComponent(out Collider2D col))
                {
                    col.enabled = false;
                }

                if (child.TryGetComponent(out SpriteRenderer renderer))
                {
                    renderer.color = Color.magenta;
                }
            }

            StopAllCoroutines();
            isFollowingPlayer = false;
            isDead = true;

            Debug.Log("kid " + id + " is dead.");
        }
    }

    private void SaveKid(int _id)
    {
        if (_id == id)
        {
            saved = true;
            isFollowingPlayer = false;
            //Andar
        }
    }
}
