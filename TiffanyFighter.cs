using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class TiffanyFighter : FightingPlayerController,IPunObservable // tiffany moveset, ninja who casts projectiles and uses agility as a zoner.
{
    void Awake() //set character specific stats
    {
        maxHealth = 100f; //default hp
        jumpStrengthMult = 1.2f; //agile jump
        health = maxHealth;
        moveSpeed = 1.6f; //slightly slow
        maxBlockMeter = 60f; //weak block because zoner
        blockMeter = maxBlockMeter;
        blockRegenRate = 3f; //worse regen but stronger block, focus on aggresion after getting an opening
        gravityScale = 0.25f; //good aerial control
        maxSpecialMeter = 200f;  //can save more special for projectiles
        specialMeterRate = 5f; // INCREDIBLY FAST special meter gain
        specialMeter = 0f;
        crouchedSpecialCost = 20f;
        aerialSpecialCost = 10f;
        neutralSpecialCost = 20f;
        stunTimer = 0.5f;

    }

    // ---ATTACKS--- in progress
    // tiff has lots of projectiles and zoning options with an invincible CS move to escape from a bad situation
    public override void NeutralLightAttack() //data for attacK -- slow startup but safe on block and good combo starter
    {
        isInAttack = true;
        currentAttackDamage = 5f;
        currentAttackStun = 2.3f;
        currentAttackProperty = "n/a";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 1f;
        currentAttackHitstun = 0.1f;
        currentAttackBlockStunDuration = 1.1f;
        stunTimer = 1f;
        AttackReward = 8f;


    }
    public override void NeutralHeavyAttack() //proj
    {
        isInAttack = true;
        stunTimer = 2.5f;
    }
    public override void NeutralSpecialAttack() //proj
    {

        isInAttack = true;
        stunTimer = 3.5f;
        
    }
    public override void CrouchedLightAttack()
    {
        isInAttack = true;
        currentAttackDamage = 10f;
        currentAttackStun = 2f;
        currentAttackProperty = "launch";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 0.6f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 1.5f;
        currentAttackHitstun = 0.1f;
        AttackReward = 8f;
        
    }
    public override void CrouchedHeavyAttack() //proj
    {
        isInAttack = true;
        stunTimer = 2.5f;
    }
    public override void CrouchedSpecialAttack() // INVINCIBLE ESCAPE MOVE
    {
        isInAttack = true;
        stunTimer = 0.5f;

    }
    public override void AerialLightAttack()
    {
        isInAttack = true;
        currentAttackDamage = 10f;
        currentAttackStun = 2f;
        currentAttackProperty = "high";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 1f;
        currentAttackBlockStunDuration = 1.5f;
        currentAttackHitstun = 0.1f;
        stunTimer = 1f;
    }
    public override void AerialHeavyAttack()//strong combo start but unsafe on block
    {
        isInAttack = true;
        currentAttackDamage = 20f;
        currentAttackStun = 4f;
        currentAttackProperty = "high";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 3f;
        currentAttackBlockStunDuration = 1.5f;
        currentAttackHitstun = 0.2f;
        stunTimer = 1.5f;
    }
    public override void AerialSpecialAttack() //proj
    {
        isInAttack = true;
        stunTimer = 3f;
    }
    public override void ForwardLightAttack() //knockback kick to push enemy away, slow and unsafe but knocks back far and rewarding
    {
        isInAttack = true;
        currentAttackDamage = 10f;
        currentAttackStun = 4f;
        currentAttackProperty = "n/a";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 3f;
        currentAttackBlockStunDuration = 0.2f; // VERY UNSAFE
        currentAttackHitstun = 0.1f;
        stunTimer = 2f;
    }
    public override void ForwardHeavyAttack() //proj
    {
        isInAttack = true;
        stunTimer = 2f;
        
    }


    public override void CounterSuccess()
    {
        photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "Counter");
        isInAttack = true;
        currentAttackDamage = 40f;
        currentAttackStun = 6f;
        currentAttackProperty = "unblockable";
        currentAttackProperty2 = "knockdown";
        currentAttackKnockbackForce = 10f;
        currentAttackHitstun = 0.1f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 0f;
        
    }

    public void smokeBomb()//unique move, teleports up and becomes intangible, called in animation event
    {
        PlayParticleSystem(5);
        DisableAllHurtboxes();
        Vector3 teleportPosition = transform.position + new Vector3(0, 3f, 0);
        transform.position = teleportPosition;
        gravityScale = -0.1f;
        
    } 

    public void EndSmoke()
    {
        EnableAllHurtboxes();
        StopParticleSystem(5);
        CancellableMove();
        gravityScale = 0.25f;
    }

}
