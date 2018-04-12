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

    void Update()
    {
        if (health == maxHealth && healthBar.gameObject.activeSelf)
        {
            healthBar.gameObject.SetActive(false);
            feedbackBar.gameObject.SetActive(false);
        }

        else if (health < maxHealth && !healthBar.gameObject.activeSelf)
        {
            healthBar.gameObject.SetActive(true);
            feedbackBar.gameObject.SetActive(true);
        }
    }

    public void TakeDamage(float _damage)
    {
        //Setup feedback bar
        feedbackBar.value = health / maxHealth;
        if (feedbackCor != null)
            StopCoroutine(feedbackCor);

        health -= _damage;
        health = Mathf.Clamp(health, 0f, maxHealth);

        //Feedback on bars
        UpdateHealthBar();
        feedbackCor = StartCoroutine(FeedbackBarCor(health));

        if (health == 0)
        {
            Die();
        }
    }

    public void PushAway()
    {
        //Call push away method in the main enemybehaviour script
        print("pushed away! code it!");
    }

    void UpdateHealthBar()
    {
        healthBar.value = health / maxHealth;
    }

    void Die()
    {
        StartCoroutine(DieCor());
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

    IEnumerator DieCor()
    {
        //Trigger animation and wait until it's done
        yield return null;

        //Deactivate components

        //Until I have an animation :
        gameObject.SetActive(false);
    }
}
