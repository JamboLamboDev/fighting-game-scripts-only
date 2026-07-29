using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ProjHitbox : MonoBehaviour
{
    public Projectile owner;
    public FightingPlayerController playerOwner;

    private void Awake()
    {
        if (owner == null)
        {
            owner = GetComponentInParent<Projectile>();
        }
        if (playerOwner == null)
        {
            playerOwner = GetComponentInParent<FightingPlayerController>();
        }
    }
    public virtual void OnTriggerEnter(Collider other)
    {
        Hurtbox hurtbox = other.GetComponent<Hurtbox>();
        if (hurtbox != null && hurtbox.owner != owner)
        {
            Debug.Log(" Projectile Hitbox triggered by " + other.name);
            owner.TargetHit(hurtbox.owner);
            Destroy(gameObject);
        }
    }

    public virtual void OnTriggerStay(Collider other) //to double check
    {
        Hurtbox hurtbox = other.GetComponent<Hurtbox>();
        if (hurtbox != null && hurtbox.owner != playerOwner) //fixes bug where projectiles could hit their own player owner since projectile owner is not a player
        {
            Debug.Log("Projectile Hitbox triggered by " + other.name);
            owner.TargetHit(hurtbox.owner);
            //destroy hitbox
            Destroy(gameObject);
            
        }
    }

    private void OnDrawGizmos() //for testing hitbox in editor
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null && col.enabled)
        {
            Gizmos.color = Color.blue;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.center, col.size);
        }
    }
}
