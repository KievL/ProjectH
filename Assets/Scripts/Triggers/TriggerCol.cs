using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerCol
{
    private Collider2D collider;
    private bool activated;

    // Start is called before the first frame update
    public TriggerCol(Collider2D col, bool _activated)
    {
        this.collider = col;
        this.activated = false;
    }

    public Collider2D GetTriggerCollider()
    {
        return this.collider;
    }

    public void SetTriggerEnable(bool yesOrNo)
    {
        activated = yesOrNo;
    }

    public bool GetTriggerEnable()
    {
        return activated;
    }
}
