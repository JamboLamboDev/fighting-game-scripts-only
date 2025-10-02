using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightingPlayerController : MonoBehaviour
{
    public float moveSpeed;
    private bool isBlocking; // if blocking
    private bool isCrouched; // is crouching
    public bool isGrounded;
    public bool isKnockedDown; //hard knockdown when hit by low heavy, grabs, special moves, etc
    private float stunTimer; // frame count for stun, attacks, blocks, etc
    private CharacterController characterController;
    public Animator animator;
    public float health;
    public float maxHealth;
    public float blockMeter; // depletes when blocking, regenerates when not blocking
    public float maxBlockMeter;
    public float blockRegenRate;
    public float gravityScale; // to adjust gravity for character controller
    private float fallingspeed; // to apply gravity
    private bool isJumping; // to queue jump input
    public bool isGrabbed; // check if grabbed to allow escape attempts
    public int comboCount; // counts hits in a combo, resets when stun ends

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
        if (stunTimer > 0) //can't do anything if stunned
        {
            stunTimer -= Time.deltaTime;
        }
        else
        {
            if (Input.GetKeyDown("w") && characterController.isGrounded && !isJumping)//jump if grounded and not already queued to jump
            {
                isJumping = true;
            }
            isKnockedDown = false; // get up if knocked down and stun is over
            moveChar();

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
            }
            else
            {
                animator.SetBool("isWalking", false);
            }
            characterController.Move(movement * Time.deltaTime * moveSpeed);
        }

    }
    public void TakeDamage(float damage, float stunDuration, string attackProperty, string attackProperty2, float knockbackForce, float blockStunDuration) //called by attacker when attack hitbox connects
    {
        if (!isKnockedDown) // can't take damage if already knocked down
        {
            if (isBlocking && !blockSuccess(attackProperty, attackProperty2))
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

                //insert logic for block timer regen

                if (blockMeter < 0) // if block is broken
                {
                    blockMeter = 0;
                    //insert block break animation
                    stunTimer = 10f; //long stun on block break
                    health -= damage * 0.4f; // take extra damage on block break up to 50% total damage
                }
                // Play block hit animation
                //animator.Play("BlockHit");
                stunTimer = blockStunDuration; //negative frames on block hit
            }
            else
            {
                health -= damage;
                if (stunTimer > 0)
                {
                    comboCount++; // increase combo count if already in stun
                }
                else
                {
                    comboCount = 1; // reset combo count if not in stun
                }

                stunTimer = stunDuration;
                //animator.Play("Hit"); // Play hit animation

                if (attackProperty == "launch" || attackProperty2 == "launch")
                {
                    // Apply launch effect
                    fallingspeed = knockbackForce; // Example: set vertical speed for launch
                    //animator.Play("Launched"); // Play launched animation
                }
                else if (attackProperty == "knockdown" || attackProperty2 == "knockdown")
                {
                    isKnockedDown = true;
                    //animator.Play("Knockdown"); // Play knockdown animation

                }

            }
        }

    }

    public blockSuccess(string attackProperty, string attackProperty2) //check if block is unsuccessful
    {
        if (attackProperty == "high" || attackProperty2 == "high")
        {
            if (isCrouched)
            {
                return false;
            }
        }
        else if (attackProperty == "low" || attackProperty2 == "low")
        {
            if (!isCrouched)
            {
                return false;
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
}
