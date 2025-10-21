using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class Projectile : MonoBehaviour
{
    public float lifetime;
    public FightingPlayerController owner;
    public float projSpeed;
    // Attack properties of this projectile - set in prefab.
    public float currentAttackDamage; // damage of current attack
    public float currentAttackStun; // stun duration of current attack
    public string currentAttackProperty; // property of current attack (high, low, launch, knockdown, etc)
    public string currentAttackProperty2;
    public float currentAttackKnockbackForce;
    public float currentAttackBlockStunDuration;
    public string currentAttackStatusEffect;
    public float currentAttackStatusEffectDur;
    private Vector3 moveDirection;
    private PhotonView photonView;
    public ProjHitbox attackHitbox;
    

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        attackHitbox = GetComponentInChildren<ProjHitbox>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return; 
        lifetime -= Time.deltaTime;
        if ( moveDirection != Vector3.zero)
        {
            transform.Translate(moveDirection * projSpeed * Time.deltaTime);
        }
        if (lifetime <= 0)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
    }

    public void TargetHit(FightingPlayerController target)
    {
        if (owner.weaknessTimer > 0)
        {
            currentAttackDamage /= 1.3f; // 30% less damage while weak
        }
        target.photonView.RPC("RPC_TakeDamage", target.photonView.Owner, currentAttackDamage, currentAttackStun, currentAttackProperty, currentAttackProperty2, currentAttackKnockbackForce, currentAttackBlockStunDuration, currentAttackStatusEffect, currentAttackStatusEffectDur);
        PhotonNetwork.Destroy(this.gameObject);
    }
    
    public void SetVar(Vector3 dir,int playerLayerMask)
    {
        moveDirection = dir.normalized;
        attackHitbox.gameObject.layer = playerLayerMask; //needs to be on right layer
    }
}
