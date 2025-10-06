using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GregFighter : FightingPlayerController //inherits from FightingPlayerController, to make a specific character
{
    void Awake() //set character specific stats
    {
        maxHealth = 100f;
        health = maxHealth;
        moveSpeed = 1.7f;
        maxBlockMeter = 100f;
        blockMeter = maxBlockMeter;
        blockRegenRate = 5f;
        gravityScale = 0.4f;
        maxSpecialMeter = 100f;
        specialMeter = 0f;
        crouchedSpecialCost = 20f;
        aerialSpecialCost = 40f;
        neutralSpecialCost = 60f;
        stunTimer = 0.5f; //locks player into attack
        
    }

// ---ATTACKS--- \\
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


    }
    public override void NeutralHeavyAttack()
    {
        isInAttack = true;
        currentAttackDamage = 10f;
        currentAttackStun = 1f;
        currentAttackProperty = "n/a";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 5f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 0.5f;
    }
    public override void NeutralSpecialAttack()
    {

        isInAttack = true;
        currentAttackDamage = 10f;
        currentAttackStun = 1f;
        currentAttackProperty = "n/a";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 5f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 0.5f;
        
    }
    public override void CrouchedLightAttack()
    {
        isInAttack = true;
        currentAttackDamage = 10f;
        currentAttackStun = 1f;
        currentAttackProperty = "n/a";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 5f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 0.5f;
        
    }
    public override void CrouchedHeavyAttack()
    {
        isInAttack = true;
        currentAttackDamage = 10f;
        currentAttackStun = 1f;
        currentAttackProperty = "n/a";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 5f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 0.5f;
    }
    public override void CrouchedSpecialAttack()
    {
        isInAttack = true;
        currentAttackDamage = 10f;
        currentAttackStun = 1f;
        currentAttackProperty = "n/a";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 5f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 0.5f;
    }
    public override void AerialLightAttack()
    {
        isInAttack = true;
        currentAttackDamage = 10f;
        currentAttackStun = 1f;
        currentAttackProperty = "n/a";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 5f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 0.5f;
    }
    public override void AerialHeavyAttack()
    {
        isInAttack = true;
        currentAttackDamage = 10f;
        currentAttackStun = 1f;
        currentAttackProperty = "n/a";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 5f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 0.5f;
    }
    public override void AerialSpecialAttack()
    {
        isInAttack = true;
        currentAttackDamage = 10f;
        currentAttackStun = 1f;
        currentAttackProperty = "n/a";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 5f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 0.5f;
    }
    public override void ForwardLightAttack()
    {
        isInAttack = true;
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
        currentAttackDamage = 20f;
        currentAttackStun = 1f;
        currentAttackProperty = "high"; //overhead attack
        currentAttackProperty2 = "HardKnockdown";
        currentAttackKnockbackForce = 5f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 1.5f;
        
    }
    public override void GuardBreakSuccess(FightingPlayerController target) //unfinished
    {
        return;
    }

}
