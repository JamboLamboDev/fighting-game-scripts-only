using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class VerticalProjectile : MovingProjectile //projectile that spawns a set amount away and goes vertically up or down.
{
    public bool goUp;
    public float projSpaceFromOwner;
    // Start is called before the first frame update
    public override void Start()
    {
        photonView = GetComponent<PhotonView>();
        attackHitbox = GetComponentInChildren<ProjHitbox>();
    }



    public override void SetVar(Vector3 dir,int playerLayerMask)
    {
        if (goUp)
        {
            moveDirection = Vector3.up;
        }
        else
        {
            moveDirection = Vector3.down;
        }
        transform.position += projectileVerticleOffset;//apply offset
        transform.position += dir.normalized * projSpaceFromOwner;//space from owner
        attackHitbox.gameObject.layer = playerLayerMask; //needs to be on right layer
        if (owner != null)
        {
            attackHitbox.playerOwner = owner; //set player owner for hit detection
        }
    }
}
