using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crucifix : MonoBehaviour
{
    public bool isUsing = false;
    public float health;
    public Slider healthBar;
    GameObject fill;
    Image background;

    // Start is called before the first frame update
    void Start()
    {
        health = 10f;
        healthBar.maxValue = health;
        healthBar.value = health;

        fill = healthBar.GetComponent<Slider>().fillRect.gameObject;
        background = healthBar.GetComponentInChildren<Image>();

        background.color = new Color(1f, 1f, 1f, 0.3f);
        fill.GetComponent<Image>().color = Color.green;
    }

    // Update is called once per frame
    void Update()
    {
        if (isUsing)
        {
            health -= Time.deltaTime;
            healthBar.value = health;

            if (health <= 0)
            {
                isUsing = false;
            }
        }

        if (health <= healthBar.maxValue / 4 && health > 0)
        {
            fill.GetComponent<Image>().color = Color.red;
        } else if (health <= 3*healthBar.maxValue/4 && health > healthBar.maxValue/4)
        {
            fill.GetComponent<Image>().color = Color.yellow;
        } else if (health > 3 * healthBar.maxValue / 4)
        {
            fill.GetComponent<Image>().color = Color.green;
        } else
        {
            background.color = new Color(0.73f, 0.23f, 0.23f, 0.3f);
            Destroy(fill);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            UseCrucifix();
        }
    }

    public void UseCrucifix()
    {
        if (health > 0)
        {
            isUsing = !isUsing;

            // coloca o crucifixo na cena
        }
    }
}
