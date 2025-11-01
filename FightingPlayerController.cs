using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
public abstract class FightingPlayerController : MonoBehaviour, IPunObservable
{
    FightingPlayerController opponent;  //find in start, used to face
    public Collider[] hitboxes; // array of hitbox colliders
    public Collider[] hurtboxes; // array of hurtbox colliders
    [SerializeField] private string[] projectilePrefabLocations;
    private int projectileLayer; //assign it to be same as hitboxes
    [SerializeField] private float guardBreakKnockback; //knockback on gb success to make animation look better
    public float moveSpeed;
    private float defaultMoveSpeed;
    private float dashTimer;
    private bool isBlocking; // if blocking
    private bool isCrouched; // is crouching
    public bool isGrounded;
    public bool isKnockedDown; //hard knockdown when hit by low heavy, grabs, special moves, etc
    public float stunTimer; // frame count for stun, attacks, blocks, etc
    private CharacterController characterController;
    public Animator animator;
    public float health;
    private bool isWalking = false;
    private bool isDashing = false;
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
    public bool isInAttack;//to track current attack state
    public float crouchedSpecialCost; // changed in child class to set character specific costs
    public float aerialSpecialCost;
    public float neutralSpecialCost;

    // Attack properties of current attack //to be set in child class attack functions
    public float currentAttackDamage; // damage of current attack
    public float currentAttackStun; // stun duration of current attack
    public string currentAttackProperty; // property of current attack (high, low, launch, knockdown, etc)
    public string currentAttackProperty2;
    public float currentAttackKnockbackForce;
    public float currentAttackBlockStunDuration;
    public string currentAttackStatusEffect = "n/a";
    public float currentAttackStatusEffectDur = 0f;
    public float AttackReward; //bonus spe meter for hitting.
    public bool isGuardBreakAttacking;
    public bool FacingRight = true;
    public bool isInCounter = false;
    public bool notCancellable; //for attacks to cancel early.
    public BlockSFX blockEffect; // block effect.
    public PhotonView photonView; // to identify owner.
    private Coroutine doKnockback; // called when taking damage to execute knockback 
    // ------------------------------------------------------- Bars ----------------------------------------------------
    public ValBar healthBar;
    public ValBar blockBar;
    public ValBar specialBar;
    public TextMeshProUGUI specialBarNum;
    private int specialMeterInt; //for displaying special meter for player to see
    public TextMeshProUGUI comboCountIndicator;
    //-------------------------------------------------------- Character Color --------------------------------------------
    [SerializeField] private Renderer[] characterRenderers;
    [SerializeField] private Color guardBreakColor;
    private Color[] defaultColors;
    private Material[] cachedMaterials;

    //-------------------------------------------------------- Status Effects ------------------------------------------
    public float healTimer;
    public float dotTimer;  // for generic damage over time effects such as acid or fire and such.
    public float slowTimer; //slow effects like ice, slows movement
    public float shockTimer;//extra damage when hit
    public float weaknessTimer;
    public bool isAfflicted; //does the player have a status effect




    // Start is called before the first frame update

    void Start()
    {
        defaultMoveSpeed = moveSpeed; // sets the character's speed set in char script to be their max speed.
        photonView = GetComponent<PhotonView>();
        characterController = GetComponent<CharacterController>();
        isCrouched = false;
        animator = GetComponent<Animator>();
        stunTimer = 100f; // stunned until match starts.
        animator.SetBool("isGrounded", true);
        blockEffect = GetComponentInChildren<BlockSFX>();
        characterRenderers = GetComponentsInChildren<Renderer>();
        cachedMaterials = new Material[characterRenderers.Length];
        defaultColors = new Color[characterRenderers.Length];

        for (int i = 0; i < characterRenderers.Length; i++)
        {
            cachedMaterials[i] = characterRenderers[i].material; // cache material reference
            defaultColors[i] = cachedMaterials[i].color;         // cache default color
        }
    }

    [PunRPC]
    public void RPC_Spawn(int playerLayerMask) //called after manager has already spawned both players and assigned playerLayerMask
    {
        projectileLayer = playerLayerMask;
        foreach (var hb in hitboxes) //double check hitboxes and hurtboxes are on correct layer
        {
            if (hb != null)
                hb.gameObject.layer = playerLayerMask;
        }

        foreach (var hb in hurtboxes)
        {
            if (hb != null)
                hb.gameObject.layer = playerLayerMask;
        }

        FightingPlayerController[] players = FindObjectsOfType<FightingPlayerController>(); //automatically assign opponent
        foreach (var player in players)
        {
            if (player != this)
            {
                opponent = player;
                break;
            }
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (isGuardBreakAttacking)
        {
            float lerp = (Mathf.Sin(Time.time * 7f) + 1f) / 2f;
            for (int i = 0; i < cachedMaterials.Length; i++)
            {
                cachedMaterials[i].color = Color.Lerp(defaultColors[i], guardBreakColor, lerp);
            }

        }
        else
        {
            for (int i = 0; i < cachedMaterials.Length; i++)
            {
                cachedMaterials[i].color = defaultColors[i];
            }
        }
        
        if (!photonView.IsMine)
        {
            return;
        }


        else
        {
            if (Input.GetKeyDown("w") && characterController.isGrounded && !isJumping)//jump if grounded and not already queued to jump
            {
                isJumping = true;
                isDashing = false;
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                if (!isDashing && stunTimer <= 0f && !isJumping && characterController.isGrounded)
                {
                    photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"Dash");
                    isDashing = true;
                    stunTimer += 0.5f; // small stun during dash
                    dashTimer = 0.3f; // duration of dash
                }
            }
            if (isDashing)
            {
                dashTimer -= Time.deltaTime;
                if (dashTimer <= 0f)
                {
                    isDashing = false;
                }
            }
            updateChecks();// hitboxes,facing opponent, block, etc
            moveChar();
            if (stunTimer > 0 && !notCancellable) //can't do anything if stunned and current state isnt cancellable
            {
                stunTimer -= Time.deltaTime;
            }
            else
            {
                block(); //check for block input
                comboCount = 0; // reset combo count 
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
                else if (Input.GetKeyDown(KeyCode.Space)) //guard break attack
                {
                    photonView.RPC("RPC_GuardBreak", RpcTarget.All);
                }
            }
        }

    }

    [PunRPC]
    public void RPC_PlayAnimation(string animation) //animations without trigger.
    {
        animator.Play(animation);
    }

    void faceOpponent() //to be called in update
    {
        if (opponent != null)
        {
            if (opponent.transform.position.x < transform.position.x)
            {
                transform.rotation = Quaternion.Euler(0, -90, 0); // face left
                FacingRight = false; //for block input

            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 90, 0); // face right
                FacingRight = true;
            }
        }
    }

    void block() //to be called in update
    {
        if ((FacingRight && Input.GetAxis("Horizontal") < 0) || (!FacingRight && Input.GetAxis("Horizontal") > 0)) //only block if holding away from opponent
        {
            isBlocking = true;
            animator.SetBool("isBlocking", true);
        }
        else
        {
            isBlocking = false;
            animator.SetBool("isBlocking", false);
        }
    }

    void updateChecks() //done every update
    {
        faceOpponent(); //always face opponent even when stunned
        if (!isInAttack) // double checks if hitboxes and hurtboxes are correctly enabled/disabled outside of attack 
        {
            DisableAllHitboxes();
            EnableAllHurtboxes();
        }
        specialMeter += 0.5f * Time.deltaTime; // slowly gain special meter over time

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
        if (blockBar != null && specialBar != null)
        {
            blockBar.SetVal(blockMeter);
            specialBar.SetVal(specialMeter); //instant update for these bars
            updateSpecialMeterDisplay();

        }
        if (isAfflicted)
        {
            statusEffectUpdate();
        }
        if (health > maxHealth)
        { //caps hp
            health = maxHealth;
        }
        
       if (comboCountIndicator != null)
        {
            if (comboCount > 1)
            {
                comboCountIndicator.text = comboCount + "hits";
            }

            else
            {
                comboCountIndicator.text = "";
            }
        }

    }

    void moveChar()
    {
        if (isDashing)
        {
            float dashSpeed = moveSpeed * 3f; 
            Vector3 dashMovement = new Vector3(FacingRight ? 1f : -1f, 0f, 0f);
            characterController.Move(dashMovement * Time.deltaTime * dashSpeed);
        }
        else if (!isCrouched)
        {
            if (!characterController.isGrounded)
            {
                animator.SetBool("isGrounded", false);
                fallingspeed += Physics.gravity.y * gravityScale * Time.deltaTime;


            }

            else if (isJumping)
            {
                fallingspeed = 3f; // jump strength
                photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"Jump"); // play jump animation ONLY when jump is initiated
                isJumping = false;
                isDashing = false;//cant dash if jumping
            }
            else
            {
                fallingspeed = -1f; // small downward force to keep grounded
                animator.SetBool("isGrounded", true);
            }

            float horizontalInput = Input.GetAxis("Horizontal");
            if (stunTimer > 0f)
            {
                horizontalInput = 0f; // can't move if stunned
            }

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
    public void TakeDamage(float damage, float stunDuration, string attackProperty, string attackProperty2, float knockbackForce, float blockStunDuration,string attackStatusEffect,float attackStatusEffectDur) //called by attacker when attack hitbox connects
    {
        if (isInCounter)
        {
            isInCounter = false;
            CounterSuccess();
            return;
        }
        if (isInAttack) // if hit during own attack, it's a counter hit
        {
            isInAttack = false; // cancel attack if hit
            notCancellable = false; //animation cancel
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
                blockEffect.photonView.RPC("RPC_Appear", RpcTarget.All);
                lastBlockTime = Time.time; // reset last block time for regen delay

                float reducedDamage = damage * 0.1f;
                health -= reducedDamage;
                blockMeter -= damage; // Block meter depletes based on damage taken
                specialMeter += damage * 0.2f;
                TakeKnockback(knockbackForce*0.65f, false);//less knockback on block

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

                if (shockTimer > 0)//if shocked do more damage
                {
                    damageScale *= 1.5f; //50% extra damage taken
                }

                health -= damage * damageScale;
                stunTimer = stunDuration * stunScale;
                specialMeter += damage * 0.5f; // gain special meter when taking damage

                if (attackProperty == "launch" || attackProperty2 == "launch")
                {
                    photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "Launched"); ; // Play launched animation
                    TakeKnockback(knockbackForce, true);
                }
                else if (attackProperty == "knockdown" || attackProperty2 == "knockdown")
                {
                    isKnockedDown = true;
                    photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "HardKnockdown"); // Play knockdown animation

                }
                else
                {
                    photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"Damaged"); ;
                    TakeKnockback(knockbackForce, false);
                    if (attackProperty == "statusEffect" || attackProperty2 == "statusEffect")
                    {
                        photonView.RPC("RPC_SetStatusEffect", photonView.Owner, attackStatusEffect,attackStatusEffectDur);
                    }
                }

            }
        }
        healthBar.StartCoroutine(healthBar.ChangeToVal(health)); //recalibrate bars
        blockBar.SetVal(blockMeter);
        specialBar.SetVal(specialMeter);
        updateSpecialMeterDisplay();
    }

    [PunRPC]
    public void RPC_TakeDamage(float damage, float stunDuration, string attackProperty, string attackProperty2, float knockbackForce, float blockStunDuration,string attackStatusEffect,float attackStatusEffectDur) //called by attacker when attack hitbox connects
    {
        if (!photonView.IsMine) return;
        TakeDamage(damage, stunDuration, attackProperty, attackProperty2, knockbackForce, blockStunDuration,attackStatusEffect,attackStatusEffectDur);
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
                photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"CrouchedLightAttack");
                CrouchedLightAttack();
            }
            else if (!characterController.isGrounded)
            {
                photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"AerialLightAttack");
                AerialLightAttack();
            }
            else if (isWalking)
            {
                photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"ForwardLightAttack");
                ForwardLightAttack();
            }
            else
            {
                photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"LightAttack");
                NeutralLightAttack();
            }

        }
        else if (attackType == "heavy")
        {
            if (isCrouched)
            {
                photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"CrouchedHeavyAttack");
                CrouchedHeavyAttack();
            }
            else if (!characterController.isGrounded)
            {
                photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"AerialHeavyAttack");
                AerialHeavyAttack();
            }
            else if (isWalking)
            {
                photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"ForwardHeavyAttack");
                ForwardHeavyAttack();
            }
            else
            {
                photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"HeavyAttack");
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
                    photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"CrouchedSpecialAttack");
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
                    photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"AerialSpecialAttack");
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
                    photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"SpecialAttack");
                    NeutralSpecialAttack();
                }
                else
                {
                    return; // not enough meter, exit function
                }
            }
        }
    }

    // Hitbox and Hurtbox Management - called by animation events -- MESSY SINCE PHOTON ADDED

    [PunRPC]
    public void RPC_EnableHitbox(int index)
    {
        if (index >= 0 && index < hitboxes.Length)
            hitboxes[index].enabled = true;
    }

        public void EnableHitbox(int index)
    {
        photonView.RPC("RPC_EnableHitbox", RpcTarget.All,index);
    }

    [PunRPC]
    public void RPC_EnableAllHitboxes() //for full body hitboxes ONLY
    {
        foreach (var hb in hitboxes)
            hb.enabled = true;
    }

    public void EnableAllHitboxes()
    {
        photonView.RPC("RPC_EnableAllHitboxes", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_DisableAllHitboxes()
    {
        foreach (var hb in hitboxes)
            hb.enabled = false;
    }

    public void DisableAllHitboxes()
    {
        photonView.RPC("RPC_DisableAllHitboxes", RpcTarget.All);
    }
    public void EnableHurtbox(int index)
    {
        photonView.RPC("RPC_EnableHurtbox", RpcTarget.All, index);
    }
    [PunRPC]
    public void RPC_EnableHurtbox(int index)
    {
        if (index >= 0 && index < hurtboxes.Length)
            hurtboxes[index].enabled = true;
    }
    [PunRPC]
    public void RPC_DisableHurtbox(int index)
    {
        if (index >= 0 && index < hurtboxes.Length)
            hurtboxes[index].enabled = false;
    }

    public void DisableHurtbox(int index)
    {
        photonView.RPC("RPC_DisableHurtbox", RpcTarget.All, index);
    }

    [PunRPC]
     public void RPC_DisableAllHurtboxes()
    {
        foreach (var hb in hurtboxes)
            hb.enabled = false;
    }

    public void DisableAllHurtboxes()
    {
        photonView.RPC("RPC_DisableAllHurtboxes", RpcTarget.All);
    }
    [PunRPC]
    public void RPC_EnableAllHurtboxes()
    {
        foreach (var hb in hurtboxes)
            hb.enabled = true;
    }

    public void EnableAllHurtboxes()
    {
        photonView.RPC("RPC_EnableAllHurtboxes", RpcTarget.All);
    }

    //placeholder attack functions to be overridden by child class
    public abstract void NeutralLightAttack();
    public abstract void NeutralHeavyAttack();
    public abstract void NeutralSpecialAttack();
    public abstract void CrouchedLightAttack();
    public abstract void CrouchedHeavyAttack();
    public abstract void CrouchedSpecialAttack();
    public abstract void AerialLightAttack();
    public abstract void AerialHeavyAttack();
    public abstract void AerialSpecialAttack();
    public abstract void ForwardLightAttack();
    public abstract void ForwardHeavyAttack();
    public abstract void CounterSuccess(); //set to return if no counter i kit
    public void Finisher()//called when finisher completes
    {
        animator.SetBool("FinisherTriggered", false);
        stunTimer = 0f; //end stun on finisher hit
        notCancellable = false;
        if (photonView.IsMine)
        {
            opponent.photonView.RPC("RPC_TakeDamage", opponent.photonView.Owner, 10f, 5f, "unblockable", "knockdown", 3f, 10f, "n/a", 0f); //heavy damage and knockdown on finisher hit
        }
    }
    [PunRPC]
    public void RPC_GuardBreak()
    { //attack that stuns opponent and breaks guard meter
        isGuardBreakAttacking = true;
        if (!photonView.IsMine) return; //only owner does the rest.
        photonView.RPC("RPC_PlayAnimation",RpcTarget.All,"GuardBreakAttack");
        stunTimer = 3f; //framedata
        //GuardBreakHit will be called by opponent if it hits
    }

    [PunRPC]
    public void RPC_SetStun(float stun)
    {
        stunTimer = stun;
    }

    [PunRPC]
    public void RPC_Destroy()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
    }

    public void TargetHit(FightingPlayerController target) // called by hitbox when it collides with opponent
    {
        Debug.Log("Hit registered on " + target.name);

        if (isInAttack && target != null) //can only hit once per attack and verify target exists
        {
            specialMeter += AttackReward;//bonus special meter from attack.
            specialBar.SetVal(specialMeter);
            updateSpecialMeterDisplay();
            isInAttack = false; // reset attack state so can only hit once
            notCancellable = false; //animation cancel
            if (weaknessTimer > 0)
            {
                currentAttackDamage /= 1.3f; // 30% less damage while weak
            }
            target.photonView.RPC("RPC_TakeDamage",target.photonView.Owner, currentAttackDamage, currentAttackStun, currentAttackProperty, currentAttackProperty2, currentAttackKnockbackForce, currentAttackBlockStunDuration,currentAttackStatusEffect,currentAttackStatusEffectDur);
            //insert hit sound/visual effects
            DisableAllHitboxes(); // disable hitboxes after hit
        }


    }

    public void GuardBreakHit(FightingPlayerController enemy) //called when hit by guard break attack
    {
        if (!photonView.IsMine) return; //only owner to verify.
        if (!isGuardBreakAttacking) //cant be hit by guard break if you are doing one.
        {
            lastBlockTime = Time.time;
            blockMeter = 0; // break block meter
            blockBar.StartCoroutine(blockBar.ChangeToVal(blockMeter));
            TakeDamage(5f, 100f, "unblockable", "n/a", guardBreakKnockback, 0f, "n/a", 0f); //stun until hit by finisher
            enemy.photonView.RPC("RPC_GuardBreakSuccess", RpcTarget.All); //their guard break is successful
            enemy.photonView.RPC("RPC_EndGuardBreakAttack", RpcTarget.All); //end their guard break attack state
            photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "GuardBroken"); // play guard broken animation

        }
        else
        {
            enemy.photonView.RPC("RPC_EndGuardBreakAttack", RpcTarget.All); //their guard break fails and leaves them open
        }

    }
    [PunRPC]
    public void RPC_GuardBreakSuccess()
    {
        animator.SetBool("FinisherTriggered", true); //triggers the finisher after guard break is successful
    }
    [PunRPC]
    public void RPC_EndGuardBreakAttack()
    {
        isGuardBreakAttacking = false;
    }

    public void SpawnProjectile(int projID) //called by animation, generic version for most projectiles, specific ones will get a unique func
    {
        if (!photonView.IsMine) return;//1 proj spawned by owner
        Debug.Log("spawning Projectile");
        Vector3 pos = transform.position + new Vector3(FacingRight ? 1f : -1f, 1f, 0f);
        Quaternion rot = FacingRight ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);
        string projectilePrefabLocation = projectilePrefabLocations[projID];
        GameObject projObj = PhotonNetwork.Instantiate(projectilePrefabLocation, pos,rot);
        Projectile proj = projObj.GetComponent<Projectile>();
        proj.owner = this;
        proj.SetVar(FacingRight ? Vector3.right : Vector3.left,projectileLayer);
    }



    public void EndAttack() // called at end of attack animation to reset attack state
    {
        isInAttack = false;
        isInCounter = false;
        notCancellable = false;
        DisableAllHitboxes();
        EnableAllHurtboxes();
        Debug.Log("Attack ended");
        if (isGuardBreakAttacking)
        {
            photonView.RPC("RPC_EndGuardBreakAttack", RpcTarget.All);
        }
    }

 [PunRPC]
    public void RPC_SetStatusEffect(string statusEffect, float duration)
    {
        isAfflicted = true; // guaranteed affliction when this is called

        switch (statusEffect)
        {
            case "heal":
                if (healTimer < duration)
                {
                    healTimer = duration;
                }
                else
                {
                    // Give smaller instant heal if already healing
                    health += 1f * duration;
                    healthBar.SetVal(health);
                }
                break;

            case "dot":
                if (dotTimer < duration)
                {
                    dotTimer = duration;
                }
                else
                {
                    // Give extra damage if already burning
                    health -= 2f * duration;
                    healthBar.SetVal(health);
                }
                break;

            case "slow":
                if (slowTimer < duration)
                {
                    slowTimer = duration;
                    moveSpeed *= 0.5f; // needs 'f' since 0.5 is a double literal
                }
                break;

            case "shock":
                if (shockTimer < duration)
                {
                    shockTimer = duration;
                }
                break;

            case "weakness":
                if (weaknessTimer < duration)
                {
                    weaknessTimer = duration;
                }
                break;

            default:
                Debug.LogWarning($"Unknown status effect: {statusEffect}");
                break;
        }
    }


    private void statusEffectUpdate()
    {
        isAfflicted = false; // set true if still afflicted, afflicted triggers this function.
        if (healTimer > 0f)
        {// healing removes debuffs.
            dotTimer = 0f;
            slowTimer = 0f;
            shockTimer = 0f;
            weaknessTimer = 0f;
            healTimer -= Time.deltaTime;
            health += 3f * Time.deltaTime; // regen effect
            healthBar.SetVal(health);
            isAfflicted = true;
        } else // check debuffs since heals cleanses player
        {
            
            if (dotTimer > 0f)
            {
                dotTimer -= Time.deltaTime;
                health -= 3f * Time.deltaTime;
                healthBar.SetVal(health);
                isAfflicted = true;

            }

            if (slowTimer > 0f)
            {
                slowTimer -= Time.deltaTime;
                if (slowTimer <= 0f) //checks here since timer is decremented earlier
                {
                    moveSpeed = defaultMoveSpeed;// resets speed back to normal when slow ends. speed is changed when inflicted only.
                }
                else
                {
                    isAfflicted = true;
                }
            }
            
            if (shockTimer > 0f)
            {
                shockTimer -= Time.deltaTime;
                isAfflicted = true;
            }
            
            if (weaknessTimer > 0f)
            {
                weaknessTimer -= Time.deltaTime;
                isAfflicted = true;
            } 
        }
        
    }

    public void counterStart()
    {//in animation controller
        isInCounter = true;
    }


    public void TakeKnockback(float knockbackForce, bool isLaunch) //called by take damage
    {
        float verticalForce = 0f;
        if (doKnockback != null)
        {
            StopCoroutine(doKnockback);
        }
        if (isLaunch)
        {
            verticalForce = 7f;
        }
        float directionMult;

        if (FacingRight) //uses our knockback direction to know which direction to be knocked back and multiplies it to knockback correct directions
        {
            directionMult = -1f;  // player is facing right
        }
        else
        {
            directionMult = 1f; // player is facing left
        }
        Vector3 knockbackVector = new Vector3(directionMult * knockbackForce, verticalForce, 0f);
        doKnockback = StartCoroutine(KnockbackMovement(knockbackVector));
    }
    
    private IEnumerator KnockbackMovement(Vector3 velocity)
    {
        float duration = 0.2f; // how long knockback lasts
        float elapsed = 0f;

        while (elapsed < duration)
        {
            characterController.Move(velocity * Time.deltaTime);

            if (velocity.y > 0f)
            {
                velocity.y += Physics.gravity.y * gravityScale * Time.deltaTime;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        doKnockback = null;
    }

    public void SetBars(ValBar hp, ValBar block, ValBar spe,TextMeshProUGUI speText, TextMeshProUGUI comboCounter) //set bars from gamemode.
    {
        healthBar = hp;
        blockBar = block;
        specialBar = spe;
        specialBarNum = speText;
        comboCountIndicator = comboCounter;
        updateSpecialMeterDisplay();
        healthBar.SetMaxValue(maxHealth);
        blockBar.SetMaxValue(maxBlockMeter);
        specialBar.SetMaxValue(maxSpecialMeter);
        healthBar.SetVal(health);
        blockBar.SetVal(blockMeter);
        specialBar.SetVal(specialMeter);
    }

    public void CancellableMove() //moves that can be cancellable midway through
    {
        notCancellable = false; //animation cancel
    }

    private void updateSpecialMeterDisplay() //updates special meter text display
    {
        specialMeterInt = Mathf.FloorToInt(specialMeter);
        specialBarNum.text = specialMeterInt.ToString();
    }
    
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) //network functionality
{
        if (stream.IsWriting)
        {
            //send others ours
            stream.SendNext(health);
            stream.SendNext(blockMeter);
            stream.SendNext(specialMeter);
            stream.SendNext(isBlocking);
            stream.SendNext(isCrouched);
            stream.SendNext(stunTimer);
        }
        else
        {
            // get info if isnt ours.
            this.health = (float)stream.ReceiveNext();
            this.blockMeter = (float)stream.ReceiveNext();
            this.specialMeter = (float)stream.ReceiveNext();
            this.isBlocking = (bool)stream.ReceiveNext();
            this.isCrouched = (bool)stream.ReceiveNext();
            this.stunTimer = (float)stream.ReceiveNext();

            if (healthBar != null) healthBar.SetVal(this.health);
            if (blockBar != null) blockBar.SetVal(this.blockMeter);
            if (specialBar != null) specialBar.SetVal(this.specialMeter);
            if (specialBarNum != null) updateSpecialMeterDisplay();
            if (comboCountIndicator != null)
            {
                if (comboCount > 1)
                {
                    comboCountIndicator.text = comboCount + "hits";
                }
                else
                {
                    comboCountIndicator.text = "";
                }
            }

    }
}
}

