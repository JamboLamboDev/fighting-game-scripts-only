using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class DelayedProjectile : Projectile //inherits from Projectile, this is a projectile that activates after a delay, with the hitbox holding the rest of the object's visual
{
    public float ProjectileDelay;
    public float projSpaceFromOwner;
    // Start is called before the first frame update
    public override void Start()
    {
        photonView = GetComponent<PhotonView>();
        attackHitbox = GetComponentInChildren<ProjHitbox>();
        attackHitbox.enabled = false; //disable projectile hitbox at start
        //teleport a certain amount in move direction from spawn point


    }

    public override void Update()
    {
        if (!photonView.IsMine) return;
        if (ProjectileDelay > 0)
        {
            ProjectileDelay -= Time.deltaTime;
            if (ProjectileDelay <= 0)
            {
                attackHitbox.enabled = true; //enable hitbox after delay
            }
        }
        lifetime -= Time.deltaTime;
        if (lifetime <= 0)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
    }
    
    public override void SetVar(Vector3 dir,int playerLayerMask)
    {
        transform.position += dir.normalized * projSpaceFromOwner;
        attackHitbox.gameObject.layer = playerLayerMask; //needs to be on right layer
        

    }
}
