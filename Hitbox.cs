using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Hitbox : MonoBehaviour
{
    public FightingPlayerController owner;
    public float lastHitTime; //to prevent multiple triggers consecutively

    private void Awake()
    {
        if (owner == null)
        {
            owner = GetComponentInParent<FightingPlayerController>();
        }
    }
    public virtual void OnTriggerEnter(Collider other)
    {
        
        Hurtbox hurtbox = other.GetComponent<Hurtbox>();
        if (hurtbox != null && hurtbox.owner != owner && Time.time > lastHitTime + 0.1f) //prevent multiple hits in quick succession
        {
            Debug.Log("Hitbox triggered by " + other.name);
            owner.TargetHit(hurtbox.owner);
            lastHitTime = Time.time;
        }
    }

    public virtual void OnTriggerStay(Collider other) //to double check
    {
        Hurtbox hurtbox = other.GetComponent<Hurtbox>();
        if (hurtbox != null && hurtbox.owner != owner && Time.time > lastHitTime + 0.1f) //prevent multiple hits in quick succession
        {
            Debug.Log("Hitbox triggered by " + other.name);
            owner.TargetHit(hurtbox.owner);
            lastHitTime = Time.time;
        }
    }

    private void OnDrawGizmos() //for testing hitbox in editor
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null && col.enabled)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.center, col.size);
        }
    }
}
