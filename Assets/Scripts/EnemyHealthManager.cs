using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthManager : MonoBehaviour {

    [Header("Health")]
    public float health;
    public float maxHealth;

    [Header("Feedback")]
    public float feedbackDelay;
    public float decayRate;

    [Header("References")]
    public Slider healthBar;
    public Slider feedbackBar;
    Coroutine feedbackCor;

    public void TakeDamage(float _damage)
    {
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

    void UpdateHealthBar()
    {
        healthBar.value = health / maxHealth;
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
