using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardBreakHitbox : Hitbox //only for guard break attack
{
    
  public override void OnTriggerEnter(Collider other)
    {

        Hurtbox hurtbox = other.GetComponent<Hurtbox>();
        Debug.Log("Guard Break Hitbox triggered by " + other.name);
        if (hurtbox != null && hurtbox.owner != owner)
        {
            Debug.Log("Guard Break Hitbox triggered by " + other.name);
            hurtbox.owner.GuardBreakHit(owner); //call to check if guard break success
        }
    }

    public override void OnTriggerStay(Collider other) //to double check
    {
        Hurtbox hurtbox = other.GetComponent<Hurtbox>();
        if (hurtbox != null && hurtbox.owner != owner)
        {
            Debug.Log("Guard Break Hitbox triggered by " + other.name);
            hurtbox.owner.GuardBreakHit(owner); 
        }
    }
}
