using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour {

    public float damage;

	void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            HealthManager playerHealth = col.GetComponent<HealthManager>();
            playerHealth.TakeDamage(damage);
            playerHealth.PushAway(transform);
        }

        if (col.tag == "Enemy")
        {
            EnemyHealthManager enemyHealth = col.GetComponent<EnemyHealthManager>();
            enemyHealth.TakeDamage(damage);
        }
    }
}