using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MovingProjectile : Projectile //generic moving projectile
{
    public float projSpeed;
    // Attack properties of this projectile - set in prefab.
    // Start is called before the first frame update
    // Update is called once per frame
    public override void Update()
    {
        if (!photonView.IsMine) return;
        lifetime -= Time.deltaTime;
        if (moveDirection != Vector3.zero)
        {
            transform.Translate(moveDirection * projSpeed * Time.deltaTime, Space.World);
        }
        if (lifetime <= 0)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
    }
}
