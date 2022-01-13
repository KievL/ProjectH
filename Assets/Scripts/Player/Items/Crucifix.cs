using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crucifix : MonoBehaviour
{
    public bool isUsing = false;
    public float health;
    public float timeUsing;

    // Start is called before the first frame update
    void Start()
    {
        health = 100f;
    }

    // Update is called once per frame
    void Update()
    {
        if (isUsing)
        {
            timeUsing += Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            UseCrucifix();
        }
    }

    public void UseCrucifix()
    {
        isUsing = !isUsing;

        // coloca o crucifixo na cena

        if (!isUsing)
        {
            timeUsing = 0f;
        }
    }
}
