using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class GregFighter : FightingPlayerController,IPunObservable //inherits from FightingPlayerController, to make gregs moveset. Greg is a balanced all-around fighter who has a simple moveset with lots of tools and good aerial pressure.
{
    void Awake() //set character specific stats
    {
        maxHealth = 100f;//average hp
        jumpStrengthMult = 1.1f;//average jump
        health = maxHealth;
        moveSpeed = 1.7f;//slightly above average speed
        maxBlockMeter = 100f;//default block
        blockMeter = maxBlockMeter;
        blockRegenRate = 5f;//slightly above average block regen
        gravityScale = 0.3f;//slightly lower gravity for better aerial control
        maxSpecialMeter = 100f; //average special meter
        specialMeter = 0f;
        specialMeterRate = 1f; // average special meter gain
        crouchedSpecialCost = 20f; //cheap cc mixup
        aerialSpecialCost = 40f; // strong aerial attack at a reasonable cost
        neutralSpecialCost = 40f;// relatively cheap neutral special that is a counter, to give a strong tool to allow for defensive play if needed, which grants better adaptability
        stunTimer = 0.5f; //locks player into attack

    }

    // ---ATTACKS--- \\
    // greg's attacks are balanced all around, except his aerials which are stronger to give him more options in neutral, by having better aerial pressure than other characters
    public override void NeutralLightAttack() //data for attack
    {
        isInAttack = true;
        currentAttackDamage = 5f;
        currentAttackStun = 1f;
        currentAttackProperty = "n/a";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 5f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 0.2f;
        AttackReward = 4f;


    }
    public override void NeutralHeavyAttack()
    {
        isInAttack = true;
        currentAttackDamage = 10f;
        currentAttackStun = 1.2f;
        currentAttackProperty = "n/a";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 3f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 0.5f;
        AttackReward = 6f;
    }
    public override void NeutralSpecialAttack()
    {

        isInAttack = true;
        stunTimer = 100f;
        
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
        AttackReward = 8f;
        
    }
    public override void CrouchedHeavyAttack()
    {
        isInAttack = true;
        currentAttackDamage = 15f;
        currentAttackStun = 3.5f;
        currentAttackProperty = "low";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 0.2f;
        currentAttackBlockStunDuration = 1.5f;
        stunTimer = 2.5f;
        AttackReward = 6f;
    }
    public override void CrouchedSpecialAttack()
    {
        isInAttack = true;
        currentAttackDamage = 10f;
        currentAttackStun = 6f;
        currentAttackProperty = "knockdown";
        currentAttackProperty2 = "low";
        currentAttackKnockbackForce = 0.1f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 0.5f;
        AttackReward = 4f;
    }
    public override void AerialLightAttack()
    {
        isInAttack = true;
        currentAttackDamage = 10f;
        currentAttackStun = 3f;
        currentAttackProperty = "high";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 1f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 1f;
    }
    public override void AerialHeavyAttack()
    {
        isInAttack = true;
        currentAttackDamage = 20f;
        currentAttackStun = 1f;
        currentAttackProperty = "high";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 3f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 1.5f;
    }
    public override void AerialSpecialAttack()
    {
        isInAttack = true;
        currentAttackDamage = 10f;
        currentAttackStun = 5f;
        currentAttackProperty = "air";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 5f;
        currentAttackBlockStunDuration = 3f;
        stunTimer = 3f;
    }
    public override void ForwardLightAttack()
    {
        isInAttack = true;
        currentAttackDamage = 5f;
        currentAttackStun = 1f;
        currentAttackProperty = "n/a";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 1f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 0.5f;
    }
    public override void ForwardHeavyAttack()
    {
        isInAttack = true;
        currentAttackDamage = 20f;
        currentAttackStun = 6f;
        currentAttackProperty = "high"; //overhead attack
        currentAttackProperty2 = "knockdown";
        currentAttackKnockbackForce = 3f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 1.5f;
        
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
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 0f;
        
    }

}
