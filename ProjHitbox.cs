using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ProjHitbox : MonoBehaviour
{
    public Projectile owner;

    private void Awake()
    {
        if (owner == null)
        {
            owner = GetComponentInParent<Projectile>();
        }
    }
    public virtual void OnTriggerEnter(Collider other)
    {
        
        Hurtbox hurtbox = other.GetComponent<Hurtbox>();
        Debug.Log("Hitbox triggered by " + other.name);
        if (hurtbox != null && hurtbox.owner != owner)
        {
            Debug.Log("Hitbox triggered by " + other.name);
            owner.TargetHit(hurtbox.owner);
        }
    }

    public virtual void OnTriggerStay(Collider other) //to double check
    {
        Hurtbox hurtbox = other.GetComponent<Hurtbox>();
        if (hurtbox != null && hurtbox.owner != owner)
        {
            Debug.Log("Hitbox triggered by " + other.name);
            owner.TargetHit(hurtbox.owner);
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
