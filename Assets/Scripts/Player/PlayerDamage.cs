
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamage : MonoBehaviour {

    public float damage;
    public PlayerController player;

	void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Enemy")
        {
            player.Freeze();
            EnemyHealthManager enemyHealth = col.GetComponent<EnemyHealthManager>();
            enemyHealth.TakeDamage(damage);
        }
    }
}
