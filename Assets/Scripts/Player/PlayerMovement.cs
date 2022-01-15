using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool toggleMove = false;
    public bool sprintAvailable = true;

    private GameObject player, cameraScene, demonBehaviour;
    private Thermometer thermo;
    private Crucifix crucifix;

    private bool isCrucifixing = false;

    public float speed;

    FixedJoystick movementJoystick;
    FixedJoystick losJoystick;

    float movementDirectionX = 0;
    float movementDirectionY = 0;

    float losDirectionX = 0;
    float losDirectionY = 0;

    // keyboard variables
    int direcX = 0;
    int direcY = 0;

    // Start is called before the first frame update
    void Start()
    {
        player = this.gameObject;
        cameraScene = GameObject.FindGameObjectWithTag("MainCamera");
        movementJoystick = GameObject.Find("MovementJoystick").GetComponent<FixedJoystick>();
        losJoystick = GameObject.Find("LOSJoystick").GetComponent<FixedJoystick>();
        demonBehaviour = GameObject.FindGameObjectWithTag("Enemy");
        demonBehaviour.GetComponent<DemonBehaviour>().OnPlayerKilled += DisableMovement;
        thermo = GameObject.FindGameObjectWithTag("Thermometer").GetComponent<Thermometer>();
        crucifix = GetComponent<Crucifix>();
    }

    // Update is called once per frame
    void Update()
    {
        //MoveCharacter();
        MoveCharacterWithKeyboard();
        //LineOfSight();
        LookToMouse();

        if (crucifix.isUsing)
        {
            speed = 2f;
            isCrucifixing = true;
        } else if (!crucifix.isUsing && isCrucifixing == true)
        {
            speed = 3.5f;
            isCrucifixing = false;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Sprint();
        }
    }

    void MoveCharacter()
    {
        Vector2 spdPlayer = cameraScene.GetComponent<Rigidbody2D>().velocity;

        if (toggleMove)
        {
            movementDirectionX = movementJoystick.Horizontal;
            movementDirectionY = movementJoystick.Vertical;

            cameraScene.GetComponent<Rigidbody2D>().velocity = new Vector2(movementDirectionX * speed, movementDirectionY * speed);
        }
    }

    void MoveCharacterWithKeyboard()
    {
        Vector2 spdPlayer = cameraScene.GetComponent<Rigidbody2D>().velocity;

        if (toggleMove)
        {
            //Vertical movement
            if (Input.GetKeyDown(KeyCode.W))
            {
                direcY++;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                direcY--;
            }
            if (Input.GetKeyUp(KeyCode.W))
            {
                direcY--;
            }
            if (Input.GetKeyUp(KeyCode.S))
            {
                direcY++;
            }
            //Horizontal movement
            if (Input.GetKeyDown(KeyCode.D))
            {
                direcX++;
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                direcX--;
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                direcX--;
            }
            if (Input.GetKeyUp(KeyCode.A))
            {
                direcX++;
            }

            if(!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.W) && 
                !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
            {
                direcX = 0;
                direcY = 0;
            }
            

            cameraScene.GetComponent<Rigidbody2D>().velocity = new Vector2(direcX * speed, direcY * speed);
        }
    }

    public void Sprint()
    {
        if (!thermo.isOpen && !crucifix.isUsing)
        {
            if (sprintAvailable)
            {
                speed *= 1.4f;
                sprintAvailable = false;
                Invoke("StopSprint", 4f);
            }
        }
    }

    void StopSprint()
    {
        speed /= 1.4f;
        Invoke("EndSprintCooldown", 8f);
    }

    void EndSprintCooldown()
    {
        sprintAvailable = true;
    }

    void LineOfSight()
    {
        float angle;

        if(!(losJoystick.Horizontal == 0 && losJoystick.Vertical == 0))
        {
            losDirectionX = losJoystick.Horizontal;
            losDirectionY = losJoystick.Vertical;
        }

        angle = Mathf.Atan2(losDirectionY, losDirectionX) * Mathf.Rad2Deg;
        player.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void LookToMouse()
    {
        var dir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        player.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void DisableMovement()
    {
        toggleMove = false;
        losJoystick.gameObject.SetActive(false);
        movementJoystick.gameObject.SetActive(false);
    }
}
