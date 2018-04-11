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
    Vector2 direction;
    Vector2 movement;

    [Header("Attack")]

    public float dashCost;
    public float dashSpeed;
    //public float dashTime;
    public int dashFixedFrameCount;
    public float hitFreezeTime;
    public float minDashDistance;
    //public float maxDashDistance;
    public float dashFreeze;
    public float dashCooldown;
    bool canDash = true;
    bool leftClicked;

    [Header("Charge Attack")]
    public float minChargeTimeState;
    public float minChargeTimeRelease;
    float chargeTime;
    public float chargedDashCost;
    public float chargedDashSpeed;
    public int chargedFixedFrameCount;

    [Header("Dodge")]

    public float dodgeCost;
    public float dodgeSpeed;
    // public float dodgeTime;
    public int dodgeFixedFrameCount;
    public float minDodgeDistance;
    //public float maxDashDistance;
    public float dodgeRecovSpeed;
    public float dodgeRecovTime;
    public float dodgeCooldown;
    bool canDodge = true;
    public bool isDodging;
    bool rightClicked;
    public Color normalColor;
    public Color invColor;
    public Color recovColor;

    [Header("References")]
    public GameObject hbTop;
    public GameObject hbBot;
    public GameObject hbLeft;
    public GameObject hbRight;
    public Rigidbody2D rb;
    public StaminaManager staminaMan;
    Coroutine leftBuffer;
    Coroutine rightBuffer;
    public SpriteRenderer rend;

    void Start()
    {
        speed = moveSpeed;
        rend.color = normalColor;
    }

    void Update()
    {
        switch (state)
        {
            case PlayerStates.Normal:
                GetDirection();
                CheckLeftClick();
                CheckRightClick();
                GetOrientation();
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
        }

        //print(Vector3.Distance(mousePos, transform.position));
    }

    void FixedUpdate()
    {
        movement = direction * speed;

        switch (state)
        {
            case PlayerStates.Normal:
                if (Vector3.Distance(mousePos, transform.position) < minDistance)
                    rb.velocity = Vector2.zero;
                else
                    rb.velocity = movement;
                break;

            case PlayerStates.Dashing:
                rb.velocity = movement;
                break;

            case PlayerStates.Dodging:
                rb.velocity = movement;
                break;

            case PlayerStates.ChargingAttack:
                rb.velocity = Vector2.zero;
                break;

            case PlayerStates.Frozen:
                rb.velocity = Vector2.zero;
                break;
        }

        //print(rb.velocity);
    }

    void GetDirection()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos = new Vector3(mousePos.x, mousePos.y, 0f);

        direction = (mousePos - transform.position);
        direction = new Vector2(direction.x, direction.y).normalized;

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
                StartCoroutine(DashChargedCor());
            else
                StartCoroutine(DashCor());

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

            StartCoroutine(DodgeCor());
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
        StartCoroutine(FreezeCor());
    }

    IEnumerator DashCor()
    {
        staminaMan.UseStamina(dashCost);

        GameObject hitbox = null;

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

        //Vector3 startPos = transform.position;
        canDash = false;
        state = PlayerStates.Dashing;
        speed = dashSpeed;

        int frames = dashFixedFrameCount;
        while (frames > 0)
        {
            frames--;
            yield return new WaitForFixedUpdate();
        }
//        yield return new WaitForSeconds(dashTime);

        speed = 0;
        hitbox.SetActive(false);
        Vector3 endPos = transform.position;
        //print(Vector3.Distance(startPos, endPos));

        yield return new WaitForSeconds(dashFreeze);

        speed = moveSpeed;
        state = PlayerStates.Normal;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    IEnumerator DashChargedCor()
    {
        print("charged");

        staminaMan.UseStamina(chargedDashCost);

        GameObject hitbox = null;

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

        //Vector3 startPos = transform.position;
        canDash = false;
        state = PlayerStates.Dashing;
        speed = chargedDashSpeed;

        int frames = chargedFixedFrameCount;
        while (frames > 0)
        {
            frames--;
            yield return new WaitForFixedUpdate();
        }
        //        yield return new WaitForSeconds(dashTime);

        speed = 0;
        hitbox.SetActive(false);
        //Vector3 endPos = transform.position;
        //print(Vector3.Distance(startPos, endPos));

        yield return new WaitForSeconds(dashFreeze);

        speed = moveSpeed;
        state = PlayerStates.Normal;

        yield return new WaitForSeconds(dashCooldown);

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

        int frames = dodgeFixedFrameCount;
        while (frames > 0)
        {
            frames--;
            yield return new WaitForFixedUpdate();
        }
        //yield return new WaitForSeconds(dodgeTime);

        speed = dodgeRecovSpeed;
        isDodging = false;
        rend.color = recovColor;

        yield return new WaitForSeconds(dodgeRecovTime);

        speed = moveSpeed;
        state = PlayerStates.Normal;
        rend.color = normalColor;

        yield return new WaitForSeconds(dodgeCooldown);

        canDodge = true;
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