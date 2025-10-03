using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightingPlayerController : MonoBehaviour
{
    public Collider[] hitboxes; // array of hitbox colliders
    public Collider[] hurtboxes; // array of hurtbox colliders
    public float moveSpeed;
    private bool isBlocking; // if blocking
    private bool isCrouched; // is crouching
    public bool isGrounded;
    public bool isKnockedDown; //hard knockdown when hit by low heavy, grabs, special moves, etc
    private float stunTimer; // frame count for stun, attacks, blocks, etc
    private CharacterController characterController;
    public Animator animator;
    public float health;
    private bool isWalking = false;
    public float maxHealth; //changed in child class to set character specific health
    public float blockMeter; // depletes when blocking, regenerates when not blocking
    public float maxBlockMeter;
    public float blockRegenRate;
    public float gravityScale; // to adjust gravity for character controller
    private float fallingspeed; // to apply gravity
    private bool isJumping; // to queue jump input
    public bool isGrabbed; // check if grabbed to allow escape attempts
    public int comboCount; // counts hits in a combo, resets when stun ends
    private float lastBlockTime; // to track time since last block for regen delay

    private float damageScale; // scales damage taken based on combo count
    private float stunScale; // scales stun duration based on combo count
    public float specialMeter;
    public float maxSpecialMeter;
    private bool isInAttack;//to track current attack state
    private float crouchedSpecialCost; // changed in child class to set character specific costs
    private float aerialSpecialCost;
    private float neutralSpecialCost;
    // Attack properties of current attack
    private float currentAttackdamage; // damage of current attack
    private float currentAttackStun; // stun duration of current attack
    private string currentAttackProperty; // property of current attack (high, low, launch, knockdown, etc)
    private string currentAttackProperty2;
    private float currentAttackKnockbackForce;
    private float currentAttackBlockStunDuration;
    

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        isCrouched = false;
        animator = GetComponent<Animator>();


    }

    // Update is called once per frame
    void Update()
    {
        if (!isInAttack) // double checks if hitboxes and hurtboxes are correctly enabled/disabled outside of attack 
        {
            DisableAllHitboxes();
            EnableAllHurtboxes();
        }
        if (!isBlocking && blockMeter < maxBlockMeter && Time.time - lastBlockTime > 2f) //regen block meter if not blocking and after delay
        {
            blockMeter += blockRegenRate * Time.deltaTime;
            if (blockMeter > maxBlockMeter)
            {
                blockMeter = maxBlockMeter; // cap block meter
            }
        }
        if (specialMeter > maxSpecialMeter)
        {
            specialMeter = maxSpecialMeter; // cap special meter
        }

        if (stunTimer > 0) //can't do anything if stunned
        {
            stunTimer -= Time.deltaTime;
        }
        else
        {
            comboCount = 0; // reset combo count 
            if (Input.GetKeyDown("w") && characterController.isGrounded && !isJumping)//jump if grounded and not already queued to jump
            {
                isJumping = true;
            }
            isKnockedDown = false; // get up if knocked down and stun is over

            if (Input.GetKey("s") && characterController.isGrounded) //crouch only if grounded 
            {
                isCrouched = true;
                animator.SetBool("isCrouched", true);
            }
            else //if crouch not held, stand
            {
                isCrouched = false;
                animator.SetBool("isCrouched", false);
            }

            if (Input.GetKey("h")) //attacks
            {
                Attack("light");
            }
            else if (Input.GetKey("j"))
            {
                Attack("heavy");
            }
            else if (Input.GetKey("k"))
            {
                Attack("special");
            }
            moveChar();
        }

    }

    void moveChar()
    {
        if (!isCrouched)
        {
            if (!characterController.isGrounded)
            {
                animator.SetBool("isGrounded", false);
                fallingspeed += Physics.gravity.y * gravityScale * Time.deltaTime;


            }
            else if (isJumping)
            {
                fallingspeed = 3f; // jump strength
                animator.Play("Jump"); // play jump animation ONLY when jump is initiated
                isJumping = false;
            }
            else
            {
                fallingspeed = -1f; // small downward force to keep grounded
                animator.SetBool("isGrounded", true);
            }

            float horizontalInput = Input.GetAxis("Horizontal");
            Vector3 movement = new Vector3(horizontalInput, fallingspeed, 0);

            if (horizontalInput != 0)
            {
                animator.SetBool("isWalking", true);
                isWalking = true;
            }
            else
            {
                animator.SetBool("isWalking", false);
                isWalking = false;
            }
            characterController.Move(movement * Time.deltaTime * moveSpeed);
        }

    }
    public void TakeDamage(float damage, float stunDuration, string attackProperty, string attackProperty2, float knockbackForce, float blockStunDuration) //called by attacker when attack hitbox connects
    {
        if (isInAttack) // if hit during own attack, it's a counter hit
        {
            isInAttack = false; // cancel attack if hit
            damage *= 1.4f; // take extra damage when counter hit
            stunDuration *= 1.4f;
            //play counter hit sound/animation
        }
        if (!isKnockedDown) // can't take damage if already knocked down
        {
            if (isBlocking && !blockSuccess(attackProperty, attackProperty2)) //check if block is unsuccessful
            {
                isBlocking = false; // block unsuccessful, stop blocking
                damage *= 1.2f; // take extra damage for failed block
                stunDuration *= 1.2f;

            }

            if (isBlocking && blockMeter > 0)
            {

                float reducedDamage = damage * 0.1f;
                health -= reducedDamage;
                blockMeter -= damage; // Block meter depletes based on damage taken
                specialMeter += damage * 0.2f;

                //insert logic for block timer regen

                if (blockMeter < 0) // if block is broken
                {
                    blockMeter = 0;
                    //insert block break animation
                    stunTimer = 10f; //long stun on block break
                    health -= damage * 0.4f; // take extra damage on block break up to 50% total damage
                }
                stunTimer = blockStunDuration; //negative frames on block hit
            }
            else
            {

                if (stunTimer > 0)
                {
                    comboCount++; // increase combo count if already in stun
                    damageScale = 1 - (comboCount * 0.05f); // reduce damage by 5% per hit in combo
                    if (damageScale < 0.5f) damageScale = 0.5f; // cap damage reduction 
                    stunScale = 1 - (comboCount * 0.1f);
                    if (stunScale < 0.3f) stunScale = 0.3f;
                }
                else
                {
                    damageScale = 1f; // reset damage scale if not in stun
                    stunScale = 1f;
                    comboCount = 1; // reset combo count if not in stun
                }

                health -= damage * damageScale;
                stunTimer = stunDuration * stunScale;
                specialMeter += damage * 0.5f; // gain special meter when taking damage
                animator.Play("Damaged");

                if (attackProperty == "launch" || attackProperty2 == "launch")
                {
                    // Apply launch effect
                    fallingspeed = knockbackForce; // Example: set vertical speed for launch
                    //animator.Play("Launched"); // Play launched animation
                }
                else if (attackProperty == "knockdown" || attackProperty2 == "knockdown")
                {
                    isKnockedDown = true;
                    animator.Play("HardKnockdown"); // Play knockdown animation

                }

            }
        }

    }

    public bool blockSuccess(string attackProperty, string attackProperty2) //check if block is unsuccessful
    {
        if (attackProperty == "high" || attackProperty2 == "high")
        {
            if (isCrouched)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else if (attackProperty == "low" || attackProperty2 == "low")
        {
            if (!isCrouched)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else if (attackProperty == "unblockable" || attackProperty2 == "unblockable")
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    void Attack(string attackType)
    {
        if (attackType == "light")
        {
            if (isCrouched)
            {
                animator.Play("CrouchedLightAttack");
                CrouchedLightAttack();
            }
            else if (!characterController.isGrounded)
            {
                animator.Play("AerialLightAttack");
                AerialLightAttack();
            }
            else if (isWalking)
            {
                animator.Play("ForwardLightAttack");
                ForwardLightAttack();
            }
            else
            {
                animator.Play("LightAttack");
                NeutralLightAttack();
            }

        }
        else if (attackType == "heavy")
        {
            if (isCrouched)
            {
                animator.Play("CrouchedHeavyAttack");
                CrouchedHeavyAttack();
            }
            else if (!characterController.isGrounded)
            {
                animator.Play("AerialHeavyAttack");
                AerialHeavyAttack();
            }
            else if (isWalking)
            {
                animator.Play("ForwardHeavyAttack");
                ForwardHeavyAttack();
            }
            else
            {
                animator.Play("HeavyAttack");
                NeutralHeavyAttack();

            }

        }
        else if (attackType == "special")
        {
            if (isCrouched)
            {
                //crouching special move logic
                if (specialMeter > crouchedSpecialCost)
                {
                    specialMeter -= crouchedSpecialCost; // consume special meter
                    animator.Play("CrouchedSpecialAttack");
                    CrouchedSpecialAttack();
                }
                else
                {
                    return; // not enough meter, exit function
                }
            }
            else if (!characterController.isGrounded)
            {
                //jumping special move logic
                if (specialMeter > aerialSpecialCost)
                {
                    specialMeter -= aerialSpecialCost;
                    animator.Play("AerialSpecialAttack");
                    AerialSpecialAttack();
                }
                else
                {
                    return; // not enough meter, exit function
                }
            }
            else
            {
                //standing special move logic
                if (specialMeter > neutralSpecialCost)
                {
                    specialMeter -= neutralSpecialCost;
                    animator.Play("SpecialAttack");
                    NeutralSpecialAttack();
                }
                else
                {
                    return; // not enough meter, exit function
                }
            }
        }
    }

    // Hitbox and Hurtbox Management
    public void EnableHitbox(int index)
    {
        if (index >= 0 && index < hitboxes.Length)
            hitboxes[index].enabled = true;
    }

    public void DisableAllHitboxes()
    {
        foreach (var hb in hitboxes)
            hb.enabled = false;
    }
    public void EnableHurtbox(int index)
    {
        if (index >= 0 && index < hurtboxes.Length)
            hurtboxes[index].enabled = true;
    }

    public void DisableHurtbox(int index)
    {
        if (index >= 0 && index < hurtboxes.Length)
            hurtboxes[index].enabled = false;
    }

    public void DisableAllHurtboxes()
    {
        foreach (var hb in hurtboxes)
            hb.enabled = false;
    }

    public void EnableAllHurtboxes()
    {
        foreach (var hb in hurtboxes)
            hb.enabled = true;
    }

    //placeholder attack functions to be overridden by child class
    public void NeutralLightAttack()
    {
        return; //set by child class
    }
    public void NeutralHeavyAttack()
    {
        return;
    }
    public void NeutralSpecialAttack()
    {
        return;
    }
    public void CrouchedLightAttack()
    {
        return;
    }
    public void CrouchedHeavyAttack()
    {
        return;
    }
    public void CrouchedSpecialAttack()
    {
        return;
    }
    public void AerialLightAttack()
    {
        return;
    }
    public void AerialHeavyAttack()
    {
        return;
    }
    public void AerialSpecialAttack()
    {
        return;
    }
    public void ForwardLightAttack()
    {
        return;
    }
    public void ForwardHeavyAttack()
    {
        return;
    }
    public void Grab()
    {
        return;
    }

    public void TargetHit(FightingPlayerController target) // called by hitbox when it collides with opponent
    {

        if (isInAttack && target != null) //can only hit once per attack and verify target exists
        {
            isInAttack = false; // reset attack state so can only hit once
            target.TakeDamage(currentAttackdamage, currentAttackStun, currentAttackProperty, currentAttackProperty2, currentAttackKnockbackForce, currentAttackBlockStunDuration);
            //insert hit sound/visual effects
            DisableAllHitboxes(); // disable hitboxes after hit
        }
        
    
    }
}

