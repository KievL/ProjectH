using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SimpleAIMovement : MonoBehaviour
{
    private Vector2 nextWaypoint;
    private float angleDifference;
    private float targetAngle;
    private float rotateSpeed;
    NavMeshAgent demon;

    // Start is called before the first frame update
    void Start()
    {
        demon = GetComponent<NavMeshAgent>();
        demon.updateRotation = false;
        demon.updateUpAxis =false;

        rotateSpeed = 80f;
    }

    // Update is called once per frame
    void Update()
    {               
        if (demon.hasPath)
        {
            if (nextWaypoint != (Vector2)demon.path.corners[1])
            {
                StartCoroutine("RotateToWaypoints");
                nextWaypoint = demon.path.corners[1];
            }
        }
    }

    IEnumerator RotateToWaypoints()
    {
        Vector2 targetVector = demon.path.corners[1] - transform.position;
        angleDifference = Vector2.SignedAngle(transform.up, targetVector);
        targetAngle = transform.localEulerAngles.z + angleDifference;

        if (targetAngle >= 360) { targetAngle -= 360; }
        else if (targetAngle < 0) { targetAngle += 360; }

        while (transform.localEulerAngles.z < targetAngle - 0.1f || transform.localEulerAngles.z > targetAngle + 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, targetAngle), rotateSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
