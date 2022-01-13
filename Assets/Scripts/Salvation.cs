using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Salvation : MonoBehaviour
{
    public event Action<int> OnKidSaved;

    private List<GameObject> kidsSaved = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("KidBody") && !kidsSaved.Contains(collision.gameObject) )
        {
            KidBehaviour kid = collision.transform.GetComponentInParent<KidBehaviour>();
            if (!kid.isDead)
            {
                OnKidSaved(kid.id);
                kidsSaved.Add(kid.gameObject);
            }            
        }
    }

}
