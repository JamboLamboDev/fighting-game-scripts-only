using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public FightingPlayerController owner;

    private void Awake()
    {
        if (owner == null)
        {
            owner = GetComponentInParent<FightingPlayerController>();
        }
    }
    void OnTriggerEnter(Collider other)
    {
        Hurtbox hurtbox = other.GetComponent<Hurtbox>();
        if (hurtbox != null && hurtbox.owner != owner)
        {
            owner.DealDamage(hurtbox.owner);
        }
    }

    void OnTriggerStay(Collider other) //to double check
    {
        Hurtbox hurtbox = other.GetComponent<Hurtbox>();
        if (hurtbox != null && hurtbox.owner != owner)
        {
            owner.DealDamage(hurtbox.owner);
        }
    }
}
