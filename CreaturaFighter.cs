using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class CreaturaFighter : FightingPlayerController, IPunObservable //creatura moveset, berserker with weak defense but high damage output who uses raw aggression
{
    void Awake() //set character specific stats
    {
        maxHealth = 200f; //tanky because defensive ability is terrible therefore more health to compensate
        jumpStrengthMult = 1.3f;
        health = maxHealth;
        moveSpeed = 1.7f;//average speed
        maxBlockMeter = 0f; // NO BLOCK AT ALL because character is all about raw aggression and being a berserker, they lose in any prolonged fight but has better hp, specials and damage to compensate
        blockMeter = maxBlockMeter;
        blockRegenRate = 0f; //worse regen but stronger block, focus on aggresion after getting an opening
        gravityScale = 0.4f; //normal gravity
        maxSpecialMeter = 100f; //regular special meter cap
        specialMeter = 100f; //starts with full special meter to encourage aggressive playstyle
        crouchedSpecialCost = 40f; //roar attack that buffs and does pressure
        aerialSpecialCost = 40f; //weak aerial, but character is focused on ground aggression
        neutralSpecialCost = 30f; //high damage and combo
        stunTimer = 0.5f;

    }

    // ---ATTACKS--- in progress
    // zlorp's attacks are quick and can inflict status effects
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
        currentAttackKnockbackForce = 3f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 0.5f;
        AttackReward = 6f;
    }
    public override void NeutralSpecialAttack()
    {

        isInAttack = true;
        notCancellable = true;
        currentAttackDamage = 20f;
        currentAttackStun = 2.2f;
        currentAttackProperty = "high";
        currentAttackProperty2 = "n/a";
        currentAttackKnockbackForce = 1f;
        currentAttackBlockStunDuration = 1.2f;
        stunTimer = 0.5f;
        AttackReward = 6f;
        
    }
    public override void CrouchedLightAttack()
    {
        isInAttack = true;
        notCancellable = true;
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
        notCancellable = true;
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
        notCancellable = true;
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
        notCancellable = true;
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
        notCancellable = true;
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
        notCancellable = true;
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
        notCancellable = true;
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
        notCancellable = true;
        currentAttackDamage = 40f;
        currentAttackStun = 6f;
        currentAttackProperty = "unblockable";
        currentAttackProperty2 = "knockdown";
        currentAttackKnockbackForce = 10f;
        currentAttackBlockStunDuration = 0.5f;
        stunTimer = 0f;
        
    }

}
