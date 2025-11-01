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
        transform.position += moveDirection.normalized * projSpaceFromOwner; //spawn a certain extra distance away from owner
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

        transform.position += dir.normalized * projSpaceFromOwner;
        attackHitbox.gameObject.layer = playerLayerMask; //needs to be on right layer
    }
}
