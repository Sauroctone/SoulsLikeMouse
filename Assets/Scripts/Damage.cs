using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour {

    public float damage;

	void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            HealthManager playerHealth = col.GetComponent<HealthManager>();
            playerHealth.TakeDamage(damage);
        }

        if (col.tag == "Enemy")
        {
            //hurt enemy
        }
    }
}
