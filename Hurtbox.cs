using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    public FightingPlayerController owner;

    private void awake()
    {
        if (owner == null)
        {
            owner = GetComponentInParent<FightingPlayerController>();
        }
    }
    


}
