using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaManager : MonoBehaviour {

    [Header("Stamina")]
    public float stamina;
    public float maxStamina;
    float regenRate;
    public float regenNormal;
    public float regenCharging;
    public float regenTimer;
    public float emptyRegenTimer;
    bool canRegen = true;

    [Header("Feedback")]
    public float feedbackDelay;
    public float decayRate;

    [Header("References")]
    public Slider stamBar;
    public Slider feedbackBar;
    Coroutine regenCor;
    Coroutine feedbackCor;
    public PlayerController player;
    
    void Update()
    {
        if (player.state == PlayerStates.ChargingAttack)
            regenRate = regenCharging;
        else
            regenRate = regenNormal;

        //Regenerate stamina over time
        if (canRegen && stamina < maxStamina)
        {
            stamina += regenRate * Time.deltaTime;
            Mathf.Clamp(stamina, 0f, maxStamina);
            UpdateStaminaBar();
        }
    }

    public void UseStamina(float _cost)
    {
        //Setup feedback bar
        feedbackBar.value = stamina/maxStamina;
        if (feedbackCor != null)
            StopCoroutine(feedbackCor);

        stamina -= _cost;
        Mathf.Clamp(stamina, 0f, maxStamina);

        //Feedback on bars
        UpdateStaminaBar();
        feedbackCor = StartCoroutine(FeedbackBarCor(stamina));

        //Cooldown before regenerating stamina

        if (regenCor != null)
            StopCoroutine(regenCor);

        if (stamina == 0)
            regenCor = StartCoroutine(EmptyRegenCooldownCor());
        else
            regenCor = StartCoroutine(RegenCooldownCor());
    }

    void UpdateStaminaBar()
    {
        stamBar.value = stamina / maxStamina;
    }

    IEnumerator RegenCooldownCor()
    {
        canRegen = false;
        yield return new WaitForSeconds(regenTimer);
        canRegen = true;
    }

    IEnumerator EmptyRegenCooldownCor()
    {
        canRegen = false;
        yield return new WaitForSeconds(regenTimer);
        canRegen = true;
    }

    IEnumerator FeedbackBarCor(float _stamina)
    {
        yield return new WaitForSeconds(feedbackDelay);

        while (feedbackBar.value > _stamina/maxStamina)
        {
            feedbackBar.value = Mathf.Lerp(feedbackBar.value, _stamina/maxStamina, decayRate * Time.deltaTime);
            yield return null;
        }
    }
}
