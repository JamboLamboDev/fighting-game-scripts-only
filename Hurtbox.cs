using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    public FightingPlayerController owner;

    private void Awake()
    {
        if (owner == null)
        {
            owner = GetComponentInParent<FightingPlayerController>();
        }
    }

    private void OnDrawGizmos() //for testing hurtbox in editor
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null && col.enabled)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.center, col.size);
        }
    }
    
    


}
