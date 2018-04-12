using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour {
    
    [Header("Health")]
    public float health;
    public float maxHealth;
    bool isRecovering;
    public float recoveryTime;

    [Header("Feedback")]
    public float feedbackDelay;
    public float decayRate;

    [Header("References")]
    public Slider healthBar;
    public Slider feedbackBar;
    Coroutine feedbackCor;
    public PlayerController player;

    public void TakeDamage(float _damage)
    {
        if (!isRecovering && !player.isDodging)
        {
            //Invincibility
            isRecovering = true;
            StartCoroutine(RecoveryCor());

            //Setup feedback bar
            feedbackBar.value = health / maxHealth;
            if (feedbackCor != null)
                StopCoroutine(feedbackCor);

            health -= _damage;
            Mathf.Clamp(health, 0f, maxHealth);

            //Feedback on bars
            UpdateHealthBar();
            feedbackCor = StartCoroutine(FeedbackBarCor(health));
        }
    }

    void UpdateHealthBar()
    {
        healthBar.value = health / maxHealth;
    }

    IEnumerator RecoveryCor()
    {
        yield return new WaitForSeconds(recoveryTime);
        isRecovering = false;
    }

    IEnumerator FeedbackBarCor(float _health)
    {
        yield return new WaitForSeconds(feedbackDelay);

        while (feedbackBar.value > _health / maxHealth)
        {
            feedbackBar.value = Mathf.Lerp(feedbackBar.value, _health / maxHealth, decayRate * Time.deltaTime);
            yield return null;
        }
    }
}
