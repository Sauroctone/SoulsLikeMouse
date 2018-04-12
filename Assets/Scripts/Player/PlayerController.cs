using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public PlayerStates state;
    public PlayerFacing facing;

    public float inputBuffer;

    [Header("Movement")]
    public float moveSpeed;
    public float minDistance;
    float speed;
    Vector3 mousePos;
    Vector3 direction;
    Vector3 movement;

    [Header("Attack")]
    //Initiation
    public float dashCost;
    public int initFrameCount;
    public int initFrameChain;
    public float initSpeed;

    //Attack
    public float dashSpeed;
    public int dashFrameCount;
    public float damageNormal;
    public float hitFreezeTime;

    //Recovery
    public float dashFreeze;
    public float dashFreezeChain;

    bool canDash = true;
    bool leftClicked;
    public bool chainingAttack;

    public Color initColor;
    public Color dashRecovColor;

    [Header("Charge Attack")]

    public float minChargeTimeState;
    public float minChargeTimeRelease;
    float chargeTime;
    public float chargedDashCost;
    public float chargedDashSpeed;
    public float damageCharged;
    public int chargedFixedFrameCount;

    [Header("Dodge")]

    public float dodgeCost;
    public float dodgeSpeed;
    public int dodgeFrameCount;
    public float minDodgeDistance;
    public float dodgeRecovSpeed;
    public float dodgeRecovTime;
    public float dodgeCooldown;
    bool canDodge = true;
    [HideInInspector]
    public bool isDodging;
    bool rightClicked;
    public Color normalColor;
    public Color invColor;
    public Color recovColor;

    [Header("Pushed Away")]
    public float pushSpeed;
    public int pushFrameCount;
    public Color hurtColor;

    [Header("References")]
    public GameObject hbTop;
    public GameObject hbBot;
    public GameObject hbLeft;
    public GameObject hbRight;
    public Rigidbody rb;
    public StaminaManager staminaMan;
    Coroutine leftBuffer;
    Coroutine rightBuffer;
    public SpriteRenderer rend;
    public ParticleSystem walkDust;
    public ParticleSystem dodgeDust;
    public PlayerDamage damager;
    Coroutine freezeCor;
    Coroutine dashCor;
    Coroutine dodgeCor;

    void Start()
    {
        speed = moveSpeed;
        rend.color = normalColor;
    }

    void Update()
    {
        //STATE MACHINE
        switch (state)
        {
            case PlayerStates.Normal:
                GetDirection();
                CheckLeftClick();
                CheckRightClick();
                GetOrientation();
                break;

            case PlayerStates.Initiating:
                break;

            case PlayerStates.Dashing:
                CheckLeftClick();
                CheckRightClick();
                break;

            case PlayerStates.Dodging:
                CheckLeftClick();
                CheckRightClick();
                break;

            case PlayerStates.ChargingAttack:
                GetDirection();
                CheckLeftClick();
                CheckRightClick();
                GetOrientation();
                break;

            case PlayerStates.Frozen:
                break;

            case PlayerStates.Hurt:
                CheckLeftClick();
                CheckRightClick();
                break;
        }

        //print(Vector3.Distance(mousePos, transform.position));
    }

    void FixedUpdate()
    {
        movement = direction * speed;

        //STATE MACHINE
        switch (state)
        {
            case PlayerStates.Normal:
                if (Vector3.Distance(mousePos, transform.position) < minDistance)
                {
                    rb.velocity = Vector3.zero;
                }

                else
                {
                    rb.velocity = movement;
                }

                //Start walk dust
                if (rb.velocity != Vector3.zero && movement != Vector3.zero && !walkDust.isPlaying)
                {
                    walkDust.Play();
                }

                //Stop walk dust
                if (rb.velocity == Vector3.zero && walkDust.isPlaying)
                {
                    walkDust.Stop();
                }

                break;

            case PlayerStates.Initiating:
                rb.velocity = movement;
                break;

            case PlayerStates.Dashing:
                rb.velocity = movement;
                break;

            case PlayerStates.Dodging:
                rb.velocity = movement;
                break;

            case PlayerStates.ChargingAttack:
                rb.velocity = Vector3.zero;
                break;

            case PlayerStates.Frozen:
                rb.velocity = Vector3.zero;
                break;

            case PlayerStates.Hurt:
                rb.velocity = movement;
                break;
        }

        //print(rb.velocity);
    }

    void GetDirection()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos = new Vector3(mousePos.x, mousePos.y, 0f);

        direction = (mousePos - transform.position);
        direction = new Vector3(direction.x, direction.y, 0f).normalized;

        //print(direction);
    }
    
    void CheckLeftClick()
    {
        //When the player holds down
        if (Input.GetMouseButton(0))
        {
            if (canDash)
                chargeTime += Time.deltaTime;
        }

        //If the player holds down long enough
        if (state != PlayerStates.ChargingAttack && chargeTime > minChargeTimeState)
        {
            state = PlayerStates.ChargingAttack;
        }

        //If the player releases 
        if (Input.GetMouseButtonUp(0))
        {
            //Reset and launch buffer
            leftClicked = true;

            if (leftBuffer != null)
                StopCoroutine(leftBuffer);
            leftBuffer = StartCoroutine(LeftClickBuffer());
        }

        //If the player isn't dashing, clicked and has enough stamina
        if (canDash && leftClicked && staminaMan.stamina > 0)
        {
            //Stop buffer
            leftClicked = false;
            if (leftBuffer != null)
                StopCoroutine(leftBuffer);

            //Normal or charged attack
            if (chargeTime >= minChargeTimeRelease)
                dashCor = StartCoroutine(DashChargedCor());
            else
                dashCor = StartCoroutine(DashCor());

            //reset charge time
            chargeTime = 0;
        } 
    }

    void CheckRightClick()
    {
        if (Input.GetMouseButtonDown(1))
        {
            rightClicked = true;

            //Reset buffer
            if (rightBuffer != null)
                StopCoroutine(rightBuffer);
            rightBuffer = StartCoroutine(RightClickBuffer());
        }

        if (canDodge 
            && rightClicked 
            && staminaMan.stamina > 0
            && Vector3.Distance(direction, mousePos) > minDodgeDistance)
        {
            rightClicked = false;
            if (rightBuffer != null)
                StopCoroutine(rightBuffer);

            dodgeCor = StartCoroutine(DodgeCor());
        }
    }

    void GetOrientation()
    {
        float angle = GetAngle();

        if (angle <= 45 && angle > -45)
        {
            facing = PlayerFacing.Right;
        }

        else if (angle <= 135 && angle >= 45)
        {
            facing = PlayerFacing.Top;
        }

        else if (angle <= -135 && angle > 180 || angle >= -180 && angle < -135)
        {
            facing = PlayerFacing.Left;
        }

        else if (angle <= -45 && angle > -135)
        {
            facing = PlayerFacing.Bottom;
        }

        //print angle;
    }

    float GetAngle()
    {
        return Mathf.Atan2(direction.y, direction.x) * 180 / Mathf.PI;
    }

    public void Freeze()
    {
        freezeCor = StartCoroutine(FreezeCor());
    }

    public void PushAway(Transform _pusher)
    {
        if (freezeCor != null)
            StopCoroutine(freezeCor);
        if (dashCor != null)
            StopCoroutine(dashCor);
        if (dodgeCor != null)
            StopCoroutine(dodgeCor);

        StartCoroutine(PushCor(_pusher));
    }

    IEnumerator DashCor()
    {
        //INITIATION

        print("begins dash");

        canDash = false;
        staminaMan.UseStamina(dashCost);

        state = PlayerStates.Initiating;
        speed = initSpeed;
        rend.color = initColor;

        int frames = initFrameCount;
        if (chainingAttack)
            frames = initFrameChain;
        while (frames > 0)
        {
            frames--;
            yield return new WaitForFixedUpdate();
        }

        //ATTACK

        damager.damage = damageNormal;
        GameObject hitbox = null;

        //Hitbox to activate
        switch (facing)
        {
            case PlayerFacing.Top:
                hbTop.SetActive(true);
                hitbox = hbTop;
                break;
            case PlayerFacing.Bottom:
                hbBot.SetActive(true);
                hitbox = hbBot;
                break;
            case PlayerFacing.Left:
                hbLeft.SetActive(true);
                hitbox = hbLeft;
                break;
            case PlayerFacing.Right:
                hbRight.SetActive(true);
                hitbox = hbRight;
                break;
        }

        state = PlayerStates.Dashing;
        speed = dashSpeed;
        rend.color = normalColor;

        frames = dashFrameCount;
        while (frames > 0)
        {
            frames--;
            yield return new WaitForFixedUpdate();
        }

        //RECOVERY

        speed = 0;
        hitbox.SetActive(false);
        rend.color = dashRecovColor;

        if (chainingAttack)
            yield return new WaitForSeconds(dashFreezeChain);
        else
            yield return new WaitForSeconds(dashFreeze);

        if (!leftClicked)
        {
            speed = moveSpeed;
            state = PlayerStates.Normal;
            rend.color = normalColor;
            chainingAttack = false;
        }

        else
        {
            chainingAttack = true;
        }

        canDash = true;
    }

    IEnumerator DashChargedCor()
    {
        // INITIATION

        staminaMan.UseStamina(chargedDashCost);

        // ATTACK

        damager.damage = damageCharged;
        GameObject hitbox = null;

        //Hitbox to activate
        switch (facing)
        {
            case PlayerFacing.Top:
                hbTop.SetActive(true);
                hitbox = hbTop;
                break;
            case PlayerFacing.Bottom:
                hbBot.SetActive(true);
                hitbox = hbBot;
                break;
            case PlayerFacing.Left:
                hbLeft.SetActive(true);
                hitbox = hbLeft;
                break;
            case PlayerFacing.Right:
                hbRight.SetActive(true);
                hitbox = hbRight;
                break;
        }
        
        canDash = false;
        state = PlayerStates.Dashing;
        speed = chargedDashSpeed;

        int frames = chargedFixedFrameCount;
        while (frames > 0)
        {
            frames--;
            yield return new WaitForFixedUpdate();
        }

        // RECOVERY

        speed = 0;
        hitbox.SetActive(false);

        yield return new WaitForSeconds(dashFreeze);

        speed = moveSpeed;
        state = PlayerStates.Normal;
        canDash = true;
    }

    IEnumerator FreezeCor()
    {
        PlayerStates prevState = state;
        state = PlayerStates.Frozen;
        yield return new WaitForSeconds(hitFreezeTime);
        state = prevState;
    }

    IEnumerator DodgeCor()
    {
        staminaMan.UseStamina(dodgeCost);

        canDodge = false;
        state = PlayerStates.Dodging;
        speed = dodgeSpeed;
        isDodging = true;
        rend.color = invColor;
        walkDust.Stop();
        dodgeDust.Play();

        int frames = dodgeFrameCount;
        while (frames > 0)
        {
            frames--;
            yield return new WaitForFixedUpdate();
        }

        speed = dodgeRecovSpeed;
        isDodging = false;
        rend.color = recovColor;

        yield return new WaitForSeconds(dodgeRecovTime);

        speed = moveSpeed;
        state = PlayerStates.Normal;
        rend.color = normalColor;

        yield return new WaitForSeconds(dodgeCooldown);
    }

    IEnumerator PushCor(Transform _pusher)
    {
        speed = pushSpeed;
        direction = (_pusher.position - transform.position);
        direction = new Vector3(direction.x, direction.y, 0f).normalized;
        rend.color = hurtColor;
        state = PlayerStates.Hurt;

        int frames = pushFrameCount;
        while (frames > 0)
        {
            frames--;
            yield return new WaitForFixedUpdate();
        }

        speed = moveSpeed;
        state = PlayerStates.Normal;
        rend.color = normalColor;
        
        //Reset stuff in case coroutines got interrupted
        canDodge = true;
        canDash = true;
        chainingAttack = false;
    }

    IEnumerator LeftClickBuffer()
    {
        yield return new WaitForSeconds(inputBuffer);
        leftClicked = false;
    }

    IEnumerator RightClickBuffer()
    {
        yield return new WaitForSeconds(inputBuffer);
        rightClicked = false;
    }
 }