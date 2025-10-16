using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class SolarFighter : FightingPlayerController,IPunObservable //solar moveset.
{
    void Awake() //set character specific stats
    {
        maxHealth = 150f;
        health = maxHealth;
        moveSpeed = 1.2f;
        maxBlockMeter = 200f;
        blockMeter = maxBlockMeter;
        blockRegenRate = 8f;
        gravityScale = 0.9f;
        maxSpecialMeter = 150f;
        specialMeter = 0f;
        crouchedSpecialCost = 60f;//heal
        aerialSpecialCost = 0f; //none
        neutralSpecialCost = 30f;//slash
        stunTimer = 0.5f; //locks player into attack
        
    }

    // ---ATTACKS--- \\
    public override void NeutralLightAttack() //data for attack
    {
        isInAttack = true;
        notCancellable = true;
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
        notCancellable = true;
        currentAttackDamage = 10f;
        currentAttackStun = 1.2f;
        currentAttackProperty = "n/a";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 5f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 0.5f;
        AttackReward = 6f;
    }
    public override void NeutralSpecialAttack()
    {

        isInAttack = true;
        notCancellable = true;
        stunTimer = 1f;
        
    }
    public override void CrouchedLightAttack()
    {
        isInAttack = true;
        notCancellable = true;
        currentAttackDamage = 10f;
        currentAttackStun = 2f;
        currentAttackProperty = "launch";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 5f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 1.5f;
        AttackReward = 8f;
        
    }
    public override void CrouchedHeavyAttack()
    {
        isInAttack = true;
        notCancellable = true;
        currentAttackDamage = 15f;
        currentAttackStun = 3.5f;
        currentAttackProperty = "low";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 5f;
        currentAttackBlockStunDuration = 1.5f;
        stunTimer = 2.5f;
        AttackReward = 6f;
    }
    public override void CrouchedSpecialAttack()
    {
        isInAttack = true;
        notCancellable = true;
        currentAttackDamage = 10f;
        currentAttackStun = 6f;
        currentAttackProperty = "knockdown";
        currentAttackProperty2 = "low";
        currentAttackKnockbackForce = 5f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 0.5f;
        AttackReward = 4f;
    }
    public override void AerialLightAttack()
    {
        EndAttack();
    }
    public override void AerialHeavyAttack()
    {
        EndAttack(); ;
    }
    public override void AerialSpecialAttack()
    {
        EndAttack();
    }
    public override void ForwardLightAttack()
    {
        isInAttack = true;
        notCancellable = true;
        currentAttackDamage = 5f;
        currentAttackStun = 1f;
        currentAttackProperty = "n/a";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 5f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 0.5f;
    }
    public override void ForwardHeavyAttack()
    {
        isInAttack = true;
        notCancellable = true;
        currentAttackDamage = 20f;
        currentAttackStun = 6f;
        currentAttackProperty = "high"; //overhead attack
        currentAttackProperty2 = "knockdown";
        currentAttackKnockbackForce = 5f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 1.5f;
        
    }
    public override void GuardBreakSuccess(FightingPlayerController target) //unfinished
    {
        return;
    }

    public override void CounterSuccess()
    {
        photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "Counter");
        isInAttack = true;
        notCancellable = true;
        currentAttackDamage = 40f;
        currentAttackStun = 6f;
        currentAttackProperty = "unblockable";
        currentAttackProperty2 = "knockdown";
        currentAttackKnockbackForce = 10f;
        currentAttackBlockStunDuration = 0.5f;
        
    }

}
