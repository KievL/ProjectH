using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Thermometer : MonoBehaviour
{
    RectTransform thermoTransform;

    Vector2 playerPosition;
    Vector2 demonPosition;

    float distance; // distance between player and demon
    public double temperature;

    public Text TemperatureText;

    public bool isOpen = false;

    // Start is called before the first frame update
    void Start()
    {
        thermoTransform = GameObject.FindGameObjectWithTag("Thermometer").GetComponent<RectTransform>();

        InvokeRepeating("CalculateTemperature", 0.1f, 2.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CalculateTemperature()
    {
        playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        demonPosition = GameObject.FindGameObjectWithTag("Enemy").transform.position;

        distance = Vector2.Distance(playerPosition, demonPosition);

        float min = 20f;
        float max = 20f;
        
        if (distance >= 90f)
        {
            min = 16f;
            max = 23f;
        } else if (distance < 90f && distance >= 60f)
        {
            min = 14f;
            max = 17f;
        } else if (distance < 60f && distance >= 40f)
        {
            min = 8f;
            max = 14f;
        } else if (distance < 40f && distance >= 30f)
        {
            min = 5f;
            max = 10f;
        } else if (distance < 30f && distance >= 18f)
        {
            min = -4f;
            max = 2f;
        } else if (distance < 18f)
        {
            min = -10f;
            max = -5f;
        }

        temperature = System.Math.Round(Random.Range(min, max), 1);
        TemperatureText.text = temperature.ToString()+" °C";
    }

    public void SeeThermo()
    {
        if (!isOpen)
        {
            thermoTransform.Translate(4, 0, 0);
        } else
        {
            thermoTransform.Translate(-4, 0, 0);
        }

        isOpen = !isOpen;
    }
}
